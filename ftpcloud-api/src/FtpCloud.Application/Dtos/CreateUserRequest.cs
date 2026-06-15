namespace FtpCloud.Application.Dtos;

public record CreateUserRequest(string Username, string Email, string Password, string Role);
