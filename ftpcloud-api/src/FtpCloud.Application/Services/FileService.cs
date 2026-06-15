using FtpCloud.Application.Common;
using FtpCloud.Application.Dtos;
using FtpCloud.Application.Interfaces;
using FtpCloud.Application.Mapping;
using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;

namespace FtpCloud.Application.Services;

public class FileService(
    IFolderRepository folderRepository,
    IFileRepository fileRepository,
    IFileStorage fileStorage,
    IUserRepository userRepository) : IFileService
{
    public async Task<FolderContentsDto> GetPersonalAsync(Guid userId, Guid? folderId)
    {
        if (folderId is null)
        {
            var roots = await folderRepository.GetByOwnerAndTypeAsync(userId, FolderType.Personal);
            return new FolderContentsDto(null, roots.Select(f => f.ToDto()).ToList(), []);
        }

        var folder = await folderRepository.GetByIdAsync(folderId.Value)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.OwnerId != userId)
            throw new ServiceException(403, "No tienes acceso a esta carpeta");

        var files = await fileRepository.GetByFolderAsync(folder.Id);
        return new FolderContentsDto(folder.ToDto(), [], files.Select(f => f.ToDto()).ToList());
    }

    public async Task<FolderContentsDto> GetSharedAsync(Guid userId, Guid? folderId)
    {
        if (folderId is null)
        {
            var shared = await folderRepository.GetSharedWithUserAsync(userId);
            return new FolderContentsDto(null, shared.Select(f => f.ToDto()).ToList(), []);
        }

        var folder = await folderRepository.GetByIdAsync(folderId.Value)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.OwnerId == userId)
            throw new ServiceException(404, "Carpeta no encontrada");

        var membership = await folderRepository.GetMembershipAsync(folder.Id, userId)
            ?? throw new ServiceException(403, "No tienes acceso a esta carpeta");

        var files = await fileRepository.GetByFolderAsync(folder.Id);
        return new FolderContentsDto(folder.ToDto(), [], files.Select(f => f.ToDto()).ToList(),
            membership.Role.ToString().ToLowerInvariant());
    }

    public async Task<FolderContentsDto> GetGroupsAsync(Guid userId, Guid? folderId)
    {
        if (folderId is null)
        {
            var groups = await folderRepository.GetGroupsForUserAsync(userId);
            return new FolderContentsDto(null, groups.Select(f => f.ToDto()).ToList(), []);
        }

        var folder = await folderRepository.GetByIdAsync(folderId.Value)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.Type != FolderType.Group)
            throw new ServiceException(404, "Carpeta no encontrada");

        string myRole;
        if (folder.OwnerId == userId)
        {
            myRole = "owner";
        }
        else
        {
            var membership = await folderRepository.GetMembershipAsync(folder.Id, userId)
                ?? throw new ServiceException(403, "No tienes acceso a este grupo");
            myRole = membership.Role.ToString().ToLowerInvariant();
        }

        var files = await fileRepository.GetByFolderAsync(folder.Id);
        return new FolderContentsDto(folder.ToDto(), [], files.Select(f => f.ToDto()).ToList(), myRole);
    }

    public Task<FolderDto> CreateFolderAsync(Guid userId, string name) =>
        CreateFolderInternalAsync(userId, name, FolderType.Personal);

    public Task<FolderDto> CreateGroupAsync(Guid userId, string name) =>
        CreateFolderInternalAsync(userId, name, FolderType.Group);

    private async Task<FolderDto> CreateFolderInternalAsync(Guid userId, string name, FolderType type)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ServiceException(400, "El nombre es obligatorio");

        if (await folderRepository.NameExistsForOwnerAndTypeAsync(userId, name, type))
            throw new ServiceException(409, "Ya existe una carpeta con ese nombre");

        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };
        await folderRepository.AddAsync(folder);
        await folderRepository.SaveChangesAsync();

        folder.Owner = (await userRepository.GetByIdAsync(userId))!;
        return folder.ToDto();
    }

    public async Task DeleteFolderAsync(Guid userId, Guid folderId)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.OwnerId != userId)
            throw new ServiceException(403, "No tienes acceso a esta carpeta");

        var files = await fileRepository.GetByFolderAsync(folderId);
        foreach (var file in files)
            fileStorage.Delete(file.StoragePath);

        folderRepository.Remove(folder); // cascade borra FileEntity y FolderMember en la BD
        await folderRepository.SaveChangesAsync();
    }

    public async Task<List<FolderMemberDto>> GetFolderMembersAsync(Guid userId, Guid folderId)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        await EnsureAccessAsync(folder, userId, requireWrite: false);

        var members = await folderRepository.GetMembersAsync(folderId);
        return members.Select(m => m.ToDto()).ToList();
    }

    public async Task<FolderMemberDto> AddFolderMemberAsync(Guid userId, Guid folderId, string username, string role)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.OwnerId != userId)
            throw new ServiceException(403, "Solo el dueño puede compartir esta carpeta");

        if (!Enum.TryParse<FolderMemberRole>(role, true, out var parsedRole))
            throw new ServiceException(400, "Rol inválido");

        var target = await userRepository.GetByUsernameAsync(username.Trim())
            ?? throw new ServiceException(404, "Usuario no encontrado");

        if (target.Id == folder.OwnerId)
            throw new ServiceException(400, "No puedes compartir la carpeta contigo mismo");

        if (await folderRepository.GetMembershipAsync(folderId, target.Id) is not null)
            throw new ServiceException(409, "Ese usuario ya tiene acceso");

        var member = new FolderMember { FolderId = folderId, UserId = target.Id, Role = parsedRole };
        await folderRepository.AddMemberAsync(member);
        await folderRepository.SaveChangesAsync();

        member.User = target;
        return member.ToDto();
    }

    public async Task RemoveFolderMemberAsync(Guid userId, Guid folderId, Guid targetUserId)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.OwnerId != userId && userId != targetUserId)
            throw new ServiceException(403, "No tienes permiso para hacer esto");

        var member = await folderRepository.GetMembershipAsync(folderId, targetUserId)
            ?? throw new ServiceException(404, "Ese usuario no tiene acceso a la carpeta");

        folderRepository.RemoveMember(member);
        await folderRepository.SaveChangesAsync();
    }

    public async Task<FileItemDto> UploadFileAsync(Guid userId, Guid folderId, string fileName, string? contentType, long size, Stream content)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        await EnsureAccessAsync(folder, userId, requireWrite: true);

        var file = new FileEntity
        {
            Id = Guid.NewGuid(),
            Name = fileName,
            Size = size,
            MimeType = contentType,
            FolderId = folder.Id,
            UploadedById = userId,
            CreatedAt = DateTime.UtcNow
        };
        file.StoragePath = await fileStorage.SaveAsync(file.Id, content);

        await fileRepository.AddAsync(file);
        await fileRepository.SaveChangesAsync();
        return file.ToDto();
    }

    public async Task<(Stream Stream, string FileName, string MimeType)> DownloadFileAsync(Guid userId, Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        await EnsureAccessAsync(file.Folder, userId, requireWrite: false);

        return (fileStorage.OpenRead(file.StoragePath), file.Name, file.MimeType ?? "application/octet-stream");
    }

    public async Task DeleteFileAsync(Guid userId, Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        await EnsureAccessAsync(file.Folder, userId, requireWrite: true);

        fileStorage.Delete(file.StoragePath);
        fileRepository.Remove(file);
        await fileRepository.SaveChangesAsync();
    }

    private async Task EnsureAccessAsync(Folder folder, Guid userId, bool requireWrite)
    {
        if (folder.OwnerId == userId) return;

        var membership = await folderRepository.GetMembershipAsync(folder.Id, userId);
        if (membership is null)
            throw new ServiceException(403, "No tienes acceso a esta carpeta");

        if (requireWrite && membership.Role != FolderMemberRole.Editor)
            throw new ServiceException(403, "No tienes permiso de edición en esta carpeta");
    }
}
