namespace FtpCloud.Application.Dtos;

public record UserDto(Guid Id, string Username, string Email, string Role, DateTime CreatedAt);
