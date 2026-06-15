using FtpCloud.Application.Dtos;
using FtpCloud.Domain.Entities;

namespace FtpCloud.Application.Services;

public interface IAuthService
{
    Task<(UserDto User, string AccessToken, RefreshToken RefreshToken)> LoginAsync(string username, string password);
    Task<(UserDto User, string AccessToken, RefreshToken RefreshToken)> RefreshAsync(string refreshTokenValue);
    Task LogoutAsync(string refreshTokenValue);
    Task<UserDto> GetUserAsync(Guid userId);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}
