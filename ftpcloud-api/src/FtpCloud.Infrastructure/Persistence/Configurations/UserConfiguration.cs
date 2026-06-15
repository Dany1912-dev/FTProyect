using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FtpCloud.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);
        b.Property(u => u.Username).HasMaxLength(50).IsRequired();
        b.Property(u => u.Email).HasMaxLength(100).IsRequired();
        b.Property(u => u.PasswordHash).HasColumnName("password").HasMaxLength(255).IsRequired();
        b.Property(u => u.Role)
            .HasConversion(r => r.ToString().ToLower(), s => Enum.Parse<UserRole>(s, true))
            .HasMaxLength(10).IsRequired();
        b.Property(u => u.StorageQuotaBytes).HasDefaultValue(21474836480L);

        b.HasIndex(u => u.Username).IsUnique();
        b.HasIndex(u => u.Email).IsUnique();
    }
}
