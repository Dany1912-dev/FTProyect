using FtpCloud.Application.Interfaces;
using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;
using FtpCloud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FtpCloud.Api.Seed;

public static class OwnerSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<FtpCloudDbContext>();
        if (await db.Users.AnyAsync()) return;

        var config = sp.GetRequiredService<IConfiguration>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Username = config["Seed:OwnerUsername"]!,
            Email = config["Seed:OwnerEmail"]!,
            PasswordHash = hasher.Hash(config["Seed:OwnerPassword"]!),
            Role = UserRole.Owner,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
