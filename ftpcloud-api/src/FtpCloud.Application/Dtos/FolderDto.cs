namespace FtpCloud.Application.Dtos;

public record FolderDto(Guid Id, string Name, string Type, Guid OwnerId, string OwnerUsername, Guid? ParentFolderId, DateTime CreatedAt);
