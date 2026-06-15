using FtpCloud.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FtpCloud.Infrastructure.Persistence.Configurations;

public class FileEntityConfiguration : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> b)
    {
        b.HasKey(f => f.Id);
        b.Property(f => f.Name).HasMaxLength(255).IsRequired();
        b.Property(f => f.MimeType).HasMaxLength(100);
        b.Property(f => f.StoragePath).HasMaxLength(500).IsRequired();

        b.HasOne(f => f.Folder)
            .WithMany(folder => folder.Files)
            .HasForeignKey(f => f.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(f => f.UploadedBy)
            .WithMany(u => u.UploadedFiles)
            .HasForeignKey(f => f.UploadedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
