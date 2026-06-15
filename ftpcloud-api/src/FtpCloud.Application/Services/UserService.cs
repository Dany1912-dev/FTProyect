using FtpCloud.Application.Common;
using FtpCloud.Application.Dtos;
using FtpCloud.Application.Interfaces;
using FtpCloud.Application.Mapping;
using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;

namespace FtpCloud.Application.Services;

public class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IFolderRepository folderRepository,
    IFileRepository fileRepository,
    IFileStorage fileStorage) : IUserService
{
    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(u => u.ToDto()).ToList();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, UserRole creatorRole)
    {
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var requestedRole))
            throw new ServiceException(400, "Rol inválido");

        if (requestedRole == UserRole.Owner)
            throw new ServiceException(403, "No se puede crear otro owner");

        if (creatorRole == UserRole.Admin && requestedRole != UserRole.User)
            throw new ServiceException(403, "Un admin solo puede crear usuarios normales");

        if (await userRepository.UsernameOrEmailExistsAsync(request.Username, request.Email))
            throw new ServiceException(409, "El usuario o email ya existe");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = requestedRole,
            CreatedAt = DateTime.UtcNow
        };
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        return user.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id)
            ?? throw new ServiceException(404, "Usuario no encontrado");

        if (user.Role == UserRole.Owner)
            throw new ServiceException(403, "No se puede eliminar al owner");

        var ownedFolders = await folderRepository.GetAllByOwnerAsync(id);
        foreach (var folder in ownedFolders)
        {
            var files = await fileRepository.GetByFolderAsync(folder.Id);
            foreach (var file in files)
                fileStorage.Delete(file.StoragePath);
        }

        userRepository.Remove(user);
        await userRepository.SaveChangesAsync();
    }
}
