using FtpCloud.Domain.Entities;

namespace FtpCloud.Application.Interfaces;

public interface IFileRepository
{
    Task<List<FileEntity>> GetByFolderAsync(Guid folderId);
    Task<FileEntity?> GetByIdAsync(Guid id);
    Task<List<FileEntity>> GetByFolderIncludingDeletedAsync(Guid folderId);
    Task<List<FileEntity>> GetTrashedAsync(Guid userId);
    Task AddAsync(FileEntity file);
    void Remove(FileEntity file);
    Task SaveChangesAsync();
}
