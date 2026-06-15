using FtpCloud.Application.Dtos;

namespace FtpCloud.Application.Services;

public interface IFileService
{
    Task<FolderContentsDto> GetPersonalAsync(Guid userId, Guid? folderId);
    Task<FolderContentsDto> GetSharedAsync(Guid userId, Guid? folderId);
    Task<FolderContentsDto> GetGroupsAsync(Guid userId, Guid? folderId);
    Task<FolderDto> CreateFolderAsync(Guid userId, string name);
    Task<FolderDto> CreateGroupAsync(Guid userId, string name);
    Task DeleteFolderAsync(Guid userId, Guid folderId);
    Task<List<FolderMemberDto>> GetFolderMembersAsync(Guid userId, Guid folderId);
    Task<FolderMemberDto> AddFolderMemberAsync(Guid userId, Guid folderId, string username, string role);
    Task RemoveFolderMemberAsync(Guid userId, Guid folderId, Guid targetUserId);
    Task<FileItemDto> UploadFileAsync(Guid userId, Guid folderId, string fileName, string? contentType, long size, Stream content);
    Task<(Stream Stream, string FileName, string MimeType)> DownloadFileAsync(Guid userId, Guid fileId);
    Task DeleteFileAsync(Guid userId, Guid fileId);
}
