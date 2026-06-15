using FtpCloud.Application.Common;
using FtpCloud.Application.Dtos;
using FtpCloud.Application.Interfaces;
using FtpCloud.Application.Mapping;
using FtpCloud.Domain.Entities;

namespace FtpCloud.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IAuthService
{
    public async Task<(UserDto User, string AccessToken, RefreshToken RefreshToken)> LoginAsync(string username, string password)
    {
        var user = await userRepository.GetByUsernameAsync(username)
            ?? throw new ServiceException(401, "Usuario o contraseña incorrectos");

        if (!passwordHasher.Verify(password, user.PasswordHash))
            throw new ServiceException(401, "Usuario o contraseña incorrectos");

        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.Add(jwtService.RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow
        };
        await refreshTokenRepository.AddAsync(refreshToken);
        await refreshTokenRepository.SaveChangesAsync();

        return (user.ToDto(), accessToken, refreshToken);
    }

    public async Task<(UserDto User, string AccessToken, RefreshToken RefreshToken)> RefreshAsync(string refreshTokenValue)
    {
        var existing = await refreshTokenRepository.GetByTokenAsync(refreshTokenValue)
            ?? throw new ServiceException(401, "Refresh token inválido");

        if (existing.ExpiresAt < DateTime.UtcNow)
        {
            refreshTokenRepository.Remove(existing);
            await refreshTokenRepository.SaveChangesAsync();
            throw new ServiceException(401, "Refresh token expirado");
        }

        var user = existing.User;

        refreshTokenRepository.Remove(existing);

        var accessToken = jwtService.GenerateAccessToken(user);
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.Add(jwtService.RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow
        };
        await refreshTokenRepository.AddAsync(newRefreshToken);
        await refreshTokenRepository.SaveChangesAsync();

        return (user.ToDto(), accessToken, newRefreshToken);
    }

    public async Task LogoutAsync(string refreshTokenValue)
    {
        var existing = await refreshTokenRepository.GetByTokenAsync(refreshTokenValue);
        if (existing is null) return;

        refreshTokenRepository.Remove(existing);
        await refreshTokenRepository.SaveChangesAsync();
    }

    public async Task<UserDto> GetUserAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new ServiceException(401, "Usuario no encontrado");

        return user.ToDto();
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await userRepository.GetByIdAsync(userId)
            ?? throw new ServiceException(401, "Usuario no encontrado");

        if (!passwordHasher.Verify(currentPassword, user.PasswordHash))
            throw new ServiceException(401, "La contraseña actual es incorrecta");

        if (newPassword.Length < 8)
            throw new ServiceException(400, "La nueva contraseña debe tener al menos 8 caracteres");

        user.PasswordHash = passwordHasher.Hash(newPassword);
        await userRepository.SaveChangesAsync();
    }
}
