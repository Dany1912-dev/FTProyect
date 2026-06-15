using FtpCloud.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FtpCloud.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFileService, FileService>();
        return services;
    }
}
