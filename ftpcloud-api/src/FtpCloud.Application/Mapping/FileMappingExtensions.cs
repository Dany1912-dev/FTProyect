using FtpCloud.Application.Dtos;
using FtpCloud.Domain.Entities;

namespace FtpCloud.Application.Mapping;

public static class FileMappingExtensions
{
    public static FolderDto ToDto(this Folder folder) =>
        new(folder.Id, folder.Name, folder.Type.ToString().ToLowerInvariant(), folder.OwnerId, folder.Owner.Username, folder.CreatedAt);

    public static FileItemDto ToDto(this FileEntity file) =>
        new(file.Id, file.Name, file.Size, file.MimeType, file.FolderId, file.UploadedById, file.CreatedAt);

    public static FolderMemberDto ToDto(this FolderMember member) =>
        new(member.UserId, member.User.Username, member.Role.ToString().ToLowerInvariant());
}
