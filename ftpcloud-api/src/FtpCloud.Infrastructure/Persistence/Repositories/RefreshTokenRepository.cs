using FtpCloud.Application.Interfaces;
using FtpCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FtpCloud.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(FtpCloudDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token) =>
        db.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.Token == token);

    public async Task AddAsync(RefreshToken token) =>
        await db.RefreshTokens.AddAsync(token);

    public void Remove(RefreshToken token) =>
        db.RefreshTokens.Remove(token);

    public Task SaveChangesAsync() =>
        db.SaveChangesAsync();
}
