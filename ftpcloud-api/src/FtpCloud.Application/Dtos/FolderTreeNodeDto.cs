namespace FtpCloud.Application.Dtos;

public record FolderTreeNodeDto(Guid Id, string Name, Guid? ParentFolderId);
