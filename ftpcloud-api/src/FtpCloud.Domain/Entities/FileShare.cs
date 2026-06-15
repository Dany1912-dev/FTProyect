using FtpCloud.Domain.Enums;

namespace FtpCloud.Domain.Entities;

public class FileShare
{
    public Guid FileId { get; set; }
    public Guid UserId { get; set; }
    public FolderMemberRole Role { get; set; }

    public FileEntity File { get; set; } = null!;
    public User User { get; set; } = null!;
}
