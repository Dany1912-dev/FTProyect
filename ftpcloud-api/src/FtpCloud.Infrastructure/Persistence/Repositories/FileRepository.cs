using FtpCloud.Application.Interfaces;
using FtpCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FileShareEntity = FtpCloud.Domain.Entities.FileShare;

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

    public Task<List<FileShareEntity>> GetFileSharesAsync(Guid fileId) =>
        db.FileShares.Include(fs => fs.User)
            .Where(fs => fs.FileId == fileId)
            .OrderBy(fs => fs.User.Username)
            .ToListAsync();

    public Task<FileShareEntity?> GetFileShareAsync(Guid fileId, Guid userId) =>
        db.FileShares.FirstOrDefaultAsync(fs => fs.FileId == fileId && fs.UserId == userId);

    public async Task AddFileShareAsync(FileShareEntity share) =>
        await db.FileShares.AddAsync(share);

    public void RemoveFileShare(FileShareEntity share) =>
        db.FileShares.Remove(share);

    public Task<List<FileEntity>> SearchFilesAsync(Guid userId, string query, int maxResults = 20)
    {
        var pattern = $"%{query}%";
        return db.Files
            .Include(f => f.Folder).ThenInclude(fld => fld.Owner)
            .Where(f => f.DeletedAt == null
                && EF.Functions.ILike(f.Name, pattern)
                && (
                    f.Folder.OwnerId == userId
                    || db.FolderMembers.Any(fm => fm.FolderId == f.Folder.RootFolderId && fm.UserId == userId)
                    || db.FileShares.Any(fs => fs.FileId == f.Id && fs.UserId == userId)
                ))
            .OrderBy(f => f.Name)
            .Take(maxResults)
            .ToListAsync();
    }

    public Task SaveChangesAsync() =>
        db.SaveChangesAsync();
}
