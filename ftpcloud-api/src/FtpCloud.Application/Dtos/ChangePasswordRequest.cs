namespace FtpCloud.Application.Dtos;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
