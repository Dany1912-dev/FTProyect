using FtpCloud.Domain.Enums;

namespace FtpCloud.Domain.Entities;

public class Folder
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public FolderType Type { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Owner { get; set; } = null!;
    public ICollection<FolderMember> Members { get; set; } = new List<FolderMember>();
    public ICollection<FileEntity> Files { get; set; } = new List<FileEntity>();
}
