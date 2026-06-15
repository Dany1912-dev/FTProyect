using FtpCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FtpCloud.Infrastructure.Persistence;

public class FtpCloudDbContext(DbContextOptions<FtpCloudDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<FolderMember> FolderMembers => Set<FolderMember>();
    public DbSet<FileEntity> Files => Set<FileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FtpCloudDbContext).Assembly);
}
