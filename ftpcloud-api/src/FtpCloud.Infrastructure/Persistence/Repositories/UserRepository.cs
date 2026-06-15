using FtpCloud.Application.Interfaces;
using FtpCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FtpCloud.Infrastructure.Persistence.Repositories;

public class UserRepository(FtpCloudDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByUsernameAsync(string username) =>
        db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public Task<List<User>> GetAllAsync() =>
        db.Users.ToListAsync();

    public Task<bool> UsernameOrEmailExistsAsync(string username, string email) =>
        db.Users.AnyAsync(u => u.Username == username || u.Email == email);

    public async Task AddAsync(User user) =>
        await db.Users.AddAsync(user);

    public void Remove(User user) =>
        db.Users.Remove(user);

    public Task SaveChangesAsync() =>
        db.SaveChangesAsync();
}
