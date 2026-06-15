using FtpCloud.Application.Dtos;

namespace FtpCloud.Application.Services;

public interface IFileService
{
    Task<FolderContentsDto> GetPersonalAsync(Guid userId, Guid? folderId);
    Task<FolderContentsDto> GetSharedAsync(Guid userId, Guid? folderId);
    Task<FolderContentsDto> GetGroupsAsync(Guid userId, Guid? folderId);
    Task<FolderDto> CreateFolderAsync(Guid userId, string name, Guid? parentFolderId);
    Task<FolderDto> CreateGroupAsync(Guid userId, string name);
    Task DeleteFolderAsync(Guid userId, Guid folderId);
    Task<FolderDto> RenameFolderAsync(Guid userId, Guid folderId, string newName);
    Task<FolderDto> MoveFolderAsync(Guid userId, Guid folderId, Guid? targetFolderId);
    Task<List<FolderTreeNodeDto>> GetFolderTreeAsync(Guid userId, Guid rootFolderId);
    Task<List<FolderMemberDto>> GetFolderMembersAsync(Guid userId, Guid folderId);
    Task<FolderMemberDto> AddFolderMemberAsync(Guid userId, Guid folderId, string username, string role);
    Task RemoveFolderMemberAsync(Guid userId, Guid folderId, Guid targetUserId);
    Task EnsureUploadAllowedAsync(Guid userId, Guid folderId, long size);
    Task<FileItemDto> UploadFileAsync(Guid userId, Guid folderId, string fileName, string? contentType, long size, Stream content);
    Task<(Stream Stream, string FileName, string MimeType)> DownloadFileAsync(Guid userId, Guid fileId);
    Task DeleteFileAsync(Guid userId, Guid fileId);
    Task<FileItemDto> RenameFileAsync(Guid userId, Guid fileId, string newName);
    Task<FileItemDto> MoveFileAsync(Guid userId, Guid fileId, Guid targetFolderId);
    Task<TrashContentsDto> GetTrashAsync(Guid userId);
    Task RestoreFolderAsync(Guid userId, Guid folderId);
    Task RestoreFileAsync(Guid userId, Guid fileId);
    Task PermanentlyDeleteFolderAsync(Guid userId, Guid folderId);
    Task PermanentlyDeleteFileAsync(Guid userId, Guid fileId);
    Task EmptyTrashAsync(Guid userId);
    Task<SearchResultsDto> SearchAsync(Guid userId, string query);
    Task<List<FileShareDto>> GetFileSharesAsync(Guid userId, Guid fileId);
    Task<FileShareDto> AddFileShareAsync(Guid userId, Guid fileId, string username, string role);
    Task RemoveFileShareAsync(Guid userId, Guid fileId, Guid targetUserId);
    Task<List<UserSummaryDto>> GetShareableUsersAsync(Guid userId, string? query);
}
