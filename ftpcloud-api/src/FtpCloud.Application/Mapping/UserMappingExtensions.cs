using FtpCloud.Application.Dtos;
using FtpCloud.Domain.Entities;

namespace FtpCloud.Application.Mapping;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user, long usedBytes) =>
        new(user.Id, user.Username, user.Email, user.Role.ToString().ToLowerInvariant(), user.CreatedAt,
            usedBytes, user.StorageQuotaBytes);
}
