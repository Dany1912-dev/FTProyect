namespace FtpCloud.Application.Dtos;

public record TrashContentsDto(List<FolderDto> Folders, List<FileItemDto> Files);
