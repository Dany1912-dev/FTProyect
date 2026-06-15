using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FtpCloud.Domain.Enums;

namespace FtpCloud.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    public static UserRole GetRole(this ClaimsPrincipal user) =>
        Enum.Parse<UserRole>(user.FindFirstValue(ClaimTypes.Role)!);
}
