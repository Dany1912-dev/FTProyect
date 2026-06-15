using FtpCloud.Domain.Entities;
using FileShareEntity = FtpCloud.Domain.Entities.FileShare;

namespace FtpCloud.Application.Interfaces;

public interface IFileRepository
{
    Task<List<FileEntity>> GetByFolderAsync(Guid folderId);
    Task<FileEntity?> GetByIdAsync(Guid id);
    Task<List<FileEntity>> GetByFolderIncludingDeletedAsync(Guid folderId);
    Task<List<FileEntity>> GetTrashedAsync(Guid userId);
    Task<List<FileShareEntity>> GetFileSharesAsync(Guid fileId);
    Task<FileShareEntity?> GetFileShareAsync(Guid fileId, Guid userId);
    Task<List<FileEntity>> GetFilesSharedWithUserAsync(Guid userId);
    Task AddFileShareAsync(FileShareEntity share);
    void RemoveFileShare(FileShareEntity share);
    Task<List<FileEntity>> SearchFilesAsync(Guid userId, string query, int maxResults = 20);
    Task AddAsync(FileEntity file);
    void Remove(FileEntity file);
    Task SaveChangesAsync();
}
