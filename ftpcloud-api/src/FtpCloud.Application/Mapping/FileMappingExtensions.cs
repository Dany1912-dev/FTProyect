using FtpCloud.Application.Dtos;
using FtpCloud.Domain.Entities;
using FileShareEntity = FtpCloud.Domain.Entities.FileShare;

namespace FtpCloud.Application.Mapping;

public static class FileMappingExtensions
{
    public static FolderDto ToDto(this Folder folder) =>
        new(folder.Id, folder.Name, folder.Type.ToString().ToLowerInvariant(), folder.OwnerId, folder.Owner.Username, folder.ParentFolderId, folder.CreatedAt, folder.DeletedAt);

    public static FolderTreeNodeDto ToTreeNodeDto(this Folder folder) =>
        new(folder.Id, folder.Name, folder.ParentFolderId);

    public static FileItemDto ToDto(this FileEntity file) =>
        new(file.Id, file.Name, file.Size, file.MimeType, file.FolderId, file.UploadedById, file.CreatedAt, file.DeletedAt);

    public static FolderMemberDto ToDto(this FolderMember member) =>
        new(member.UserId, member.User.Username, member.Role.ToString().ToLowerInvariant());

    public static FileShareDto ToDto(this FileShareEntity share) =>
        new(share.UserId, share.User.Username, share.Role.ToString().ToLowerInvariant());
}
