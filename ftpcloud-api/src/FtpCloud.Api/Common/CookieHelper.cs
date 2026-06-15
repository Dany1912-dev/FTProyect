namespace FtpCloud.Api.Common;

public static class CookieHelper
{
    public const string AccessTokenCookie = "accessToken";
    public const string RefreshTokenCookie = "refreshToken";

    public static void SetAccessToken(HttpResponse response, string token, TimeSpan lifetime) =>
        response.Cookies.Append(AccessTokenCookie, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.Add(lifetime),
            Path = "/"
        });

    public static void SetRefreshToken(HttpResponse response, string token, DateTime expiresAt) =>
        response.Cookies.Append(RefreshTokenCookie, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt,
            Path = "/api/auth"
        });

    public static void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Delete(AccessTokenCookie, new CookieOptions { Path = "/" });
        response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = "/api/auth" });
    }
}
