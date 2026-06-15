using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FileShareEntity = FtpCloud.Domain.Entities.FileShare;

namespace FtpCloud.Infrastructure.Persistence.Configurations;

public class FileShareConfiguration : IEntityTypeConfiguration<FileShareEntity>
{
    public void Configure(EntityTypeBuilder<FileShareEntity> b)
    {
        b.HasKey(fs => new { fs.FileId, fs.UserId });
        b.Property(fs => fs.Role)
            .HasConversion(r => r.ToString().ToLower(), s => Enum.Parse<FolderMemberRole>(s, true))
            .HasMaxLength(10).IsRequired();

        b.HasOne(fs => fs.File)
            .WithMany(f => f.FileShares)
            .HasForeignKey(fs => fs.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(fs => fs.User)
            .WithMany(u => u.FileShares)
            .HasForeignKey(fs => fs.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
