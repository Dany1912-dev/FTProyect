using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FtpCloud.Infrastructure.Persistence.Configurations;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> b)
    {
        b.HasKey(f => f.Id);
        b.Property(f => f.Name).HasMaxLength(100).IsRequired();
        b.Property(f => f.Type)
            .HasConversion(t => t.ToString().ToLower(), s => Enum.Parse<FolderType>(s, true))
            .HasMaxLength(10).IsRequired();

        b.Property(f => f.RootFolderId).IsRequired();
        b.HasIndex(f => f.RootFolderId);

        b.HasOne(f => f.Owner)
            .WithMany(u => u.OwnedFolders)
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(f => f.ParentFolder)
            .WithMany(f => f.Subfolders)
            .HasForeignKey(f => f.ParentFolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
