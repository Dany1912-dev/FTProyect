using FtpCloud.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FtpCloud.Infrastructure.Storage;

public class LocalFileStorage(IConfiguration config) : IFileStorage
{
    private readonly string _root = config["Storage:RootPath"] ?? "storage";

    public async Task<string> SaveAsync(Guid fileId, Stream content)
    {
        Directory.CreateDirectory(_root);
        var path = Path.Combine(_root, fileId.ToString("N"));
        await using var fs = File.Create(path);
        await content.CopyToAsync(fs);
        return path;
    }

    public Stream OpenRead(string storagePath) => File.OpenRead(storagePath);

    public void Delete(string storagePath)
    {
        if (File.Exists(storagePath)) File.Delete(storagePath);
    }
}
