namespace FtpCloud.Application.Dtos;

public record FileItemDto(Guid Id, string Name, long Size, string? MimeType, Guid FolderId, Guid? UploadedBy, DateTime CreatedAt);
