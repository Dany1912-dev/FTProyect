using Microsoft.AspNetCore.Mvc;

namespace FtpCloud.Api.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> ApiOk<T>(T data, string? message = null) =>
        Ok(new ApiResponse<T>(data, message));

    protected ActionResult ApiOkEmpty(string? message = null) =>
        Ok(new ApiResponse<object?>(null, message));
}
