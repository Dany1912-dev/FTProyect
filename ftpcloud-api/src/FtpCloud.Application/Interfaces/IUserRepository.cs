using FtpCloud.Domain.Entities;

namespace FtpCloud.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<List<User>> GetAllAsync();
    Task<List<User>> SearchShareableUsersAsync(Guid excludeUserId, string? query, int maxResults = 20);
    Task<bool> UsernameOrEmailExistsAsync(string username, string email);
    Task AddAsync(User user);
    void Remove(User user);
    Task SaveChangesAsync();
}
