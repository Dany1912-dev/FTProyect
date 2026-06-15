namespace FtpCloud.Api.Common;

public record ApiResponse<T>(T Data, string? Message = null);
