namespace FtpCloud.Application.Dtos;

public record CreateFolderRequest(string Name, Guid? ParentFolderId = null);
