using FtpCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FileShareEntity = FtpCloud.Domain.Entities.FileShare;

namespace FtpCloud.Infrastructure.Persistence;

public class FtpCloudDbContext(DbContextOptions<FtpCloudDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<FolderMember> FolderMembers => Set<FolderMember>();
    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<FileShareEntity> FileShares => Set<FileShareEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FtpCloudDbContext).Assembly);
}
