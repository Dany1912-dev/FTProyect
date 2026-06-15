namespace FtpCloud.Application.Dtos;

public record FolderContentsDto(FolderDto? Folder, List<FolderDto> Folders, List<FileItemDto> Files, string? MyRole = null);
