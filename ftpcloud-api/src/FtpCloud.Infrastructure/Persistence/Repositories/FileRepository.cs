using FtpCloud.Application.Interfaces;
using FtpCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FtpCloud.Infrastructure.Persistence.Repositories;

public class FileRepository(FtpCloudDbContext db) : IFileRepository
{
    public Task<List<FileEntity>> GetByFolderAsync(Guid folderId) =>
        db.Files.Where(f => f.FolderId == folderId).OrderBy(f => f.Name).ToListAsync();

    public Task<FileEntity?> GetByIdAsync(Guid id) =>
        db.Files.Include(f => f.Folder).FirstOrDefaultAsync(f => f.Id == id);

    public async Task AddAsync(FileEntity file) =>
        await db.Files.AddAsync(file);

    public void Remove(FileEntity file) =>
        db.Files.Remove(file);

    public Task SaveChangesAsync() =>
        db.SaveChangesAsync();
}
