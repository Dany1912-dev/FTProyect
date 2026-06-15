using FtpCloud.Domain.Enums;

namespace FtpCloud.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    public long StorageQuotaBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Folder> OwnedFolders { get; set; } = new List<Folder>();
    public ICollection<FolderMember> FolderMemberships { get; set; } = new List<FolderMember>();
    public ICollection<FileEntity> UploadedFiles { get; set; } = new List<FileEntity>();
    public ICollection<FileShare> FileShares { get; set; } = new List<FileShare>();
}
