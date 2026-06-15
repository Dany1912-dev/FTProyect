using FtpCloud.Api.Common;
using FtpCloud.Application.Common;
using FtpCloud.Application.Dtos;
using FtpCloud.Application.Interfaces;
using FtpCloud.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FtpCloud.Api.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService authService, IJwtService jwtService) : ApiControllerBase
{
    [HttpPost("login"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserDto>>> Login(LoginRequest request)
    {
        var (user, access, refresh) = await authService.LoginAsync(request.Username, request.Password);
        CookieHelper.SetAccessToken(Response, access, jwtService.AccessTokenLifetime);
        CookieHelper.SetRefreshToken(Response, refresh.Token, refresh.ExpiresAt);
        return ApiOk(user);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue(CookieHelper.RefreshTokenCookie, out var token))
            await authService.LogoutAsync(token);

        CookieHelper.ClearAuthCookies(Response);
        return ApiOkEmpty();
    }

    [HttpPost("refresh"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserDto>>> Refresh()
    {
        if (!Request.Cookies.TryGetValue(CookieHelper.RefreshTokenCookie, out var token))
            throw new ServiceException(401, "No hay sesión activa");

        var (user, access, refresh) = await authService.RefreshAsync(token);
        CookieHelper.SetAccessToken(Response, access, jwtService.AccessTokenLifetime);
        CookieHelper.SetRefreshToken(Response, refresh.Token, refresh.ExpiresAt);
        return ApiOk(user);
    }

    [HttpGet("me"), Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me() =>
        ApiOk(await authService.GetUserAsync(User.GetUserId()));

    [HttpPost("change-password"), Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        await authService.ChangePasswordAsync(User.GetUserId(), request.CurrentPassword, request.NewPassword);
        return ApiOkEmpty("Contraseña actualizada");
    }
}
