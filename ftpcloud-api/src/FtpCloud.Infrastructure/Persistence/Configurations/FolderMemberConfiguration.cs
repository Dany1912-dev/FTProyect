using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FtpCloud.Infrastructure.Persistence.Configurations;

public class FolderMemberConfiguration : IEntityTypeConfiguration<FolderMember>
{
    public void Configure(EntityTypeBuilder<FolderMember> b)
    {
        b.HasKey(fm => new { fm.FolderId, fm.UserId });
        b.Property(fm => fm.Role)
            .HasConversion(r => r.ToString().ToLower(), s => Enum.Parse<FolderMemberRole>(s, true))
            .HasMaxLength(10).IsRequired();

        b.HasOne(fm => fm.Folder)
            .WithMany(f => f.Members)
            .HasForeignKey(fm => fm.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(fm => fm.User)
            .WithMany(u => u.FolderMemberships)
            .HasForeignKey(fm => fm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
