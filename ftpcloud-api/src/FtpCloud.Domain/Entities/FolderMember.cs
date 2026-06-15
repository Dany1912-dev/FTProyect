using FtpCloud.Domain.Enums;

namespace FtpCloud.Domain.Entities;

public class FolderMember
{
    public Guid FolderId { get; set; }
    public Guid UserId { get; set; }
    public FolderMemberRole Role { get; set; }

    public Folder Folder { get; set; } = null!;
    public User User { get; set; } = null!;
}
