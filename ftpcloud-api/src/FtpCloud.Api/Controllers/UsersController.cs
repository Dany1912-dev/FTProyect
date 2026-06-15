using FtpCloud.Api.Common;
using FtpCloud.Application.Dtos;
using FtpCloud.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FtpCloud.Api.Controllers;

[Route("api/users"), Authorize(Roles = "Owner,Admin")]
public class UsersController(IUserService userService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAll() =>
        ApiOk(await userService.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(CreateUserRequest request) =>
        ApiOk(await userService.CreateAsync(request, User.GetRole()));

    [HttpDelete("{id}"), Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await userService.DeleteAsync(id);
        return ApiOkEmpty();
    }
}
