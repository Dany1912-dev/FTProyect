using FtpCloud.Domain.Entities;

namespace FtpCloud.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken token);
    void Remove(RefreshToken token);
    Task SaveChangesAsync();
}
