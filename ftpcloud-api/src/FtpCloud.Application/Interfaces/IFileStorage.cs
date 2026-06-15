namespace FtpCloud.Application.Interfaces;

public interface IFileStorage
{
    Task<string> SaveAsync(Guid fileId, Stream content);
    Stream OpenRead(string storagePath);
    void Delete(string storagePath);
}
