using FtpCloud.Application.Dtos;
using FtpCloud.Domain.Enums;

namespace FtpCloud.Application.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto> CreateAsync(CreateUserRequest request, UserRole creatorRole);
    Task DeleteAsync(Guid id);
    Task<UserDto> UpdateQuotaAsync(Guid id, long quotaBytes);
    Task<UserDto> UpdateRoleAsync(Guid id, string role, UserRole operatorRole);
    Task<UserDto> UpdatePasswordAsync(Guid id, string newPassword);
}
