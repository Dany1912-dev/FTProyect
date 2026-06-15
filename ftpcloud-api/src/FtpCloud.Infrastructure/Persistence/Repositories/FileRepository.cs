using FtpCloud.Application.Interfaces;
using FtpCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FtpCloud.Infrastructure.Persistence.Repositories;

public class FileRepository(FtpCloudDbContext db) : IFileRepository
{
    public Task<List<FileEntity>> GetByFolderAsync(Guid folderId) =>
        db.Files.Where(f => f.FolderId == folderId && f.DeletedAt == null).OrderBy(f => f.Name).ToListAsync();

    public Task<FileEntity?> GetByIdAsync(Guid id) =>
        db.Files.Include(f => f.Folder).FirstOrDefaultAsync(f => f.Id == id);

    public Task<List<FileEntity>> GetByFolderIncludingDeletedAsync(Guid folderId) =>
        db.Files.Where(f => f.FolderId == folderId).ToListAsync();

    public Task<List<FileEntity>> GetTrashedAsync(Guid userId)
    {
        var rootIds = db.Folders.Where(r => r.OwnerId == userId && r.ParentFolderId == null).Select(r => r.Id);
        return db.Files.Include(f => f.Folder)
            .Where(f => f.DeletedAt != null && f.Folder.DeletedAt == null && rootIds.Contains(f.Folder.RootFolderId))
            .OrderBy(f => f.Name).ToListAsync();
    }

    public async Task AddAsync(FileEntity file) =>
        await db.Files.AddAsync(file);

    public void Remove(FileEntity file) =>
        db.Files.Remove(file);

    public Task SaveChangesAsync() =>
        db.SaveChangesAsync();
}
