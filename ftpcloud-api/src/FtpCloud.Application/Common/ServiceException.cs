namespace FtpCloud.Application.Common;

public class ServiceException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
