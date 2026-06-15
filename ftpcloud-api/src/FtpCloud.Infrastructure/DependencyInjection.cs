using FtpCloud.Application.Interfaces;
using FtpCloud.Infrastructure.Persistence;
using FtpCloud.Infrastructure.Persistence.Repositories;
using FtpCloud.Infrastructure.Security;
using FtpCloud.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FtpCloud.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<FtpCloudDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection"))
               .UseSnakeCaseNamingConvention());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        services.AddSingleton<IJwtService, JwtService>();

        return services;
    }
}
