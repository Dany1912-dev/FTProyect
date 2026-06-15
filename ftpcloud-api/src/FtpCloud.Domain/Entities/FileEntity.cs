namespace FtpCloud.Domain.Entities;

public class FileEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public long Size { get; set; }
    public string? MimeType { get; set; }
    public Guid FolderId { get; set; }
    public Guid? UploadedById { get; set; }
    public string StoragePath { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public Folder Folder { get; set; } = null!;
    public User? UploadedBy { get; set; }
}
