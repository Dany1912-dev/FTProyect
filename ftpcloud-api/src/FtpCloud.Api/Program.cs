using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using FtpCloud.Api.Common;
using FtpCloud.Api.Middleware;
using FtpCloud.Api.Seed;
using FtpCloud.Api.Services;
using FtpCloud.Application;
using FtpCloud.Application.Common;
using FtpCloud.Application.Services;
using FtpCloud.Infrastructure;
using FtpCloud.Infrastructure.Persistence;
using FtpCloud.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory =
        _ => new BadRequestObjectResult(new { message = "Datos inválidos" }));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.TryGetValue(CookieHelper.AccessTokenCookie, out var token))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(builder.Configuration["Cors:AllowedOrigin"]!)
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    .WithExposedHeaders("Location", "Upload-Offset", "Upload-Length", "Tus-Resumable", "Upload-Expires")));

var tusTempPath = builder.Configuration["Storage:TusTempPath"] ?? "storage/tus-tmp";
Directory.CreateDirectory(tusTempPath);
var tusDiskStore = new TusDiskStore(tusTempPath);
builder.Services.AddSingleton(tusDiskStore);
builder.Services.AddHostedService<TusCleanupService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapTus("/api/files/tus", httpContext =>
{
    // El limite default de Kestrel (30MB) es menor que el chunkSize de TUS (50MB)
    httpContext.Features.Get<IHttpMaxRequestBodySizeFeature>()!.MaxRequestBodySize = null;

    return Task.FromResult(new DefaultTusConfiguration
{
    Store = tusDiskStore,
    Expiration = new AbsoluteExpiration(TimeSpan.FromHours(24)),
    Events = new Events
    {
        OnBeforeCreateAsync = async ctx =>
        {
            var userId = ctx.HttpContext.User.GetUserId();
            if (!ctx.Metadata.TryGetValue("folderId", out var folderIdMeta) ||
                !Guid.TryParse(folderIdMeta.GetString(Encoding.UTF8), out var folderId))
            {
                ctx.FailRequest(HttpStatusCode.BadRequest, "folderId inválido");
                return;
            }

            var fileService = ctx.HttpContext.RequestServices.GetRequiredService<IFileService>();
            try
            {
                await fileService.EnsureUploadAllowedAsync(userId, folderId, ctx.UploadLength);
            }
            catch (ServiceException ex)
            {
                ctx.FailRequest((HttpStatusCode)ex.StatusCode, ex.Message);
            }
        },
        OnFileCompleteAsync = async ctx =>
        {
            var userId = ctx.HttpContext.User.GetUserId();
            var fileService = ctx.HttpContext.RequestServices.GetRequiredService<IFileService>();

            var file = await ctx.GetFileAsync();
            var metadata = await file.GetMetadataAsync(ctx.CancellationToken);
            var folderId = Guid.Parse(metadata["folderId"].GetString(Encoding.UTF8));
            var fileName = metadata.TryGetValue("filename", out var fn) ? fn.GetString(Encoding.UTF8) : "archivo";
            var contentType = metadata.TryGetValue("filetype", out var ft) ? ft.GetString(Encoding.UTF8) : null;

            var length = await tusDiskStore.GetUploadLengthAsync(ctx.FileId, ctx.CancellationToken) ?? 0;
            await using var content = await file.GetContentAsync(ctx.CancellationToken);
            await fileService.UploadFileAsync(userId, folderId, fileName, contentType, length, content);
            await tusDiskStore.DeleteFileAsync(ctx.FileId, ctx.CancellationToken);
        }
    }
    });
}).RequireAuthorization();

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<FtpCloudDbContext>().Database.MigrateAsync();
await OwnerSeeder.SeedAsync(app.Services);

app.Run();
