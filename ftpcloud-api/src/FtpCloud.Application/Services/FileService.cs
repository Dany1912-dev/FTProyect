using FtpCloud.Application.Common;
using FtpCloud.Application.Dtos;
using FtpCloud.Application.Interfaces;
using FtpCloud.Application.Mapping;
using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;
using FileShareEntity = FtpCloud.Domain.Entities.FileShare;

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
            return new FolderContentsDto(null, [], roots.Select(f => f.ToDto()).ToList(), []);
        }

        var folder = await folderRepository.GetByIdAsync(folderId.Value)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.OwnerId != userId)
            throw new ServiceException(403, "No tienes acceso a esta carpeta");

        return await BuildFolderContentsAsync(folder);
    }

    public async Task<FolderContentsDto> GetSharedAsync(Guid userId, Guid? folderId)
    {
        if (folderId is null)
        {
            var shared = await folderRepository.GetSharedWithUserAsync(userId);
            var sharedFiles = await fileRepository.GetFilesSharedWithUserAsync(userId);
            return new FolderContentsDto(null, [], shared.Select(f => f.ToDto()).ToList(), sharedFiles.Select(f => f.ToDto()).ToList());
        }

        var folder = await folderRepository.GetByIdAsync(folderId.Value)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.OwnerId == userId)
            throw new ServiceException(404, "Carpeta no encontrada");

        var membership = await folderRepository.GetMembershipAsync(folder.RootFolderId, userId)
            ?? throw new ServiceException(403, "No tienes acceso a esta carpeta");

        return await BuildFolderContentsAsync(folder, membership.Role.ToString().ToLowerInvariant());
    }

    public async Task<FolderContentsDto> GetGroupsAsync(Guid userId, Guid? folderId)
    {
        if (folderId is null)
        {
            var groups = await folderRepository.GetGroupsForUserAsync(userId);
            return new FolderContentsDto(null, [], groups.Select(f => f.ToDto()).ToList(), []);
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
            var membership = await folderRepository.GetMembershipAsync(folder.RootFolderId, userId)
                ?? throw new ServiceException(403, "No tienes acceso a este grupo");
            myRole = membership.Role.ToString().ToLowerInvariant();
        }

        return await BuildFolderContentsAsync(folder, myRole);
    }

    private async Task<FolderContentsDto> BuildFolderContentsAsync(Folder folder, string? myRole = null)
    {
        var subfolders = await folderRepository.GetSubfoldersAsync(folder.Id);
        var ancestors = await folderRepository.GetAncestorsAsync(folder.Id);
        var files = await fileRepository.GetByFolderAsync(folder.Id);

        return new FolderContentsDto(folder.ToDto(), ancestors.Select(f => f.ToDto()).ToList(),
            subfolders.Select(f => f.ToDto()).ToList(), files.Select(f => f.ToDto()).ToList(), myRole);
    }

    public Task<FolderDto> CreateFolderAsync(Guid userId, string name, Guid? parentFolderId) =>
        CreateFolderInternalAsync(userId, name, FolderType.Personal, parentFolderId);

    public Task<FolderDto> CreateGroupAsync(Guid userId, string name) =>
        CreateFolderInternalAsync(userId, name, FolderType.Group, null);

    private async Task<FolderDto> CreateFolderInternalAsync(Guid userId, string name, FolderType type, Guid? parentFolderId)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ServiceException(400, "El nombre es obligatorio");

        Folder? parent = null;
        var ownerId = userId;

        if (parentFolderId is not null)
        {
            parent = await folderRepository.GetByIdAsync(parentFolderId.Value)
                ?? throw new ServiceException(404, "Carpeta no encontrada");

            await EnsureAccessAsync(parent, userId, requireWrite: true);

            ownerId = parent.OwnerId;
            type = parent.Type;
        }

        if (await folderRepository.NameExistsInFolderAsync(parentFolderId, ownerId, type, name))
            throw new ServiceException(409, "Ya existe una carpeta con ese nombre aquí");

        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            OwnerId = ownerId,
            ParentFolderId = parentFolderId,
            CreatedAt = DateTime.UtcNow
        };
        folder.RootFolderId = parent is null ? folder.Id : parent.RootFolderId;

        await folderRepository.AddAsync(folder);
        await folderRepository.SaveChangesAsync();

        folder.Owner = (await userRepository.GetByIdAsync(ownerId))!;
        return folder.ToDto();
    }

    public async Task DeleteFolderAsync(Guid userId, Guid folderId)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.OwnerId != userId)
            throw new ServiceException(403, "No tienes acceso a esta carpeta");

        await SetTrashStateRecursivelyAsync(folder.Id, DateTime.UtcNow);
        await folderRepository.SaveChangesAsync();
    }

    private async Task SetTrashStateRecursivelyAsync(Guid folderId, DateTime? deletedAt)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        folder.DeletedAt = deletedAt;

        var files = await fileRepository.GetByFolderIncludingDeletedAsync(folder.Id);
        foreach (var file in files)
            file.DeletedAt = deletedAt;

        var subfolders = await folderRepository.GetSubfoldersIncludingDeletedAsync(folder.Id);
        foreach (var sub in subfolders)
            await SetTrashStateRecursivelyAsync(sub.Id, deletedAt);
    }

    private async Task DeleteFilesRecursivelyAsync(Guid folderId)
    {
        var files = await fileRepository.GetByFolderIncludingDeletedAsync(folderId);
        foreach (var file in files)
            fileStorage.Delete(file.StoragePath);

        var subfolders = await folderRepository.GetSubfoldersIncludingDeletedAsync(folderId);
        foreach (var sub in subfolders)
            await DeleteFilesRecursivelyAsync(sub.Id);
    }

    public async Task<TrashContentsDto> GetTrashAsync(Guid userId)
    {
        var folders = await folderRepository.GetTrashedAsync(userId);
        var files = await fileRepository.GetTrashedAsync(userId);
        return new TrashContentsDto(folders.Select(f => f.ToDto()).ToList(), files.Select(f => f.ToDto()).ToList());
    }

    public async Task RestoreFolderAsync(Guid userId, Guid folderId)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.DeletedAt is null)
            throw new ServiceException(404, "La carpeta no está en la papelera");

        await EnsureTrashOwnerAsync(folder.RootFolderId, userId);

        await SetTrashStateRecursivelyAsync(folder.Id, null);
        await folderRepository.SaveChangesAsync();
    }

    public async Task RestoreFileAsync(Guid userId, Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        if (file.DeletedAt is null)
            throw new ServiceException(404, "El archivo no está en la papelera");

        await EnsureTrashOwnerAsync(file.Folder.RootFolderId, userId);

        file.DeletedAt = null;
        await fileRepository.SaveChangesAsync();
    }

    public async Task PermanentlyDeleteFolderAsync(Guid userId, Guid folderId)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (folder.DeletedAt is null)
            throw new ServiceException(404, "La carpeta no está en la papelera");

        await EnsureTrashOwnerAsync(folder.RootFolderId, userId);

        await DeleteFilesRecursivelyAsync(folder.Id);
        folderRepository.Remove(folder); // cascade borra subcarpetas, FileEntity y FolderMember en la BD
        await folderRepository.SaveChangesAsync();
    }

    public async Task PermanentlyDeleteFileAsync(Guid userId, Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        if (file.DeletedAt is null)
            throw new ServiceException(404, "El archivo no está en la papelera");

        await EnsureTrashOwnerAsync(file.Folder.RootFolderId, userId);

        fileStorage.Delete(file.StoragePath);
        fileRepository.Remove(file);
        await fileRepository.SaveChangesAsync();
    }

    public async Task EmptyTrashAsync(Guid userId)
    {
        var folders = await folderRepository.GetTrashedAsync(userId);
        foreach (var folder in folders)
        {
            await DeleteFilesRecursivelyAsync(folder.Id);
            folderRepository.Remove(folder);
        }

        var files = await fileRepository.GetTrashedAsync(userId);
        foreach (var file in files)
        {
            fileStorage.Delete(file.StoragePath);
            fileRepository.Remove(file);
        }

        await folderRepository.SaveChangesAsync();
    }

    private async Task EnsureTrashOwnerAsync(Guid rootFolderId, Guid userId)
    {
        var root = await folderRepository.GetByIdAsync(rootFolderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        if (root.OwnerId != userId)
            throw new ServiceException(403, "No tienes acceso a la papelera de esta carpeta");
    }

    public async Task<FolderDto> RenameFolderAsync(Guid userId, Guid folderId, string newName)
    {
        newName = newName.Trim();
        if (string.IsNullOrWhiteSpace(newName))
            throw new ServiceException(400, "El nombre es obligatorio");

        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        await EnsureAccessAsync(folder, userId, requireWrite: true);

        if (await folderRepository.NameExistsInFolderAsync(folder.ParentFolderId, folder.OwnerId, folder.Type, newName, excludeId: folder.Id))
            throw new ServiceException(409, "Ya existe una carpeta con ese nombre aquí");

        folder.Name = newName;
        await folderRepository.SaveChangesAsync();
        return folder.ToDto();
    }

    public async Task<FolderDto> MoveFolderAsync(Guid userId, Guid folderId, Guid? targetFolderId)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        await EnsureAccessAsync(folder, userId, requireWrite: true);

        var tree = await folderRepository.GetTreeByOwnerAsync(folder.OwnerId);
        var childrenByParent = tree.Where(f => f.ParentFolderId.HasValue)
            .GroupBy(f => f.ParentFolderId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var descendants = new List<Folder>();
        void Collect(Guid id)
        {
            if (!childrenByParent.TryGetValue(id, out var kids)) return;
            foreach (var kid in kids)
            {
                descendants.Add(kid);
                Collect(kid.Id);
            }
        }
        Collect(folder.Id);

        Guid newRootId;
        if (targetFolderId is null)
        {
            newRootId = folder.Id;
        }
        else
        {
            if (targetFolderId == folder.Id || descendants.Any(d => d.Id == targetFolderId.Value))
                throw new ServiceException(400, "No puedes mover una carpeta dentro de sí misma");

            var target = await folderRepository.GetByIdAsync(targetFolderId.Value)
                ?? throw new ServiceException(404, "Carpeta de destino no encontrada");

            await EnsureAccessAsync(target, userId, requireWrite: true);

            if (target.OwnerId != folder.OwnerId)
                throw new ServiceException(400, "No puedes mover a una carpeta de otro dueño");

            newRootId = target.RootFolderId;
        }

        if (await folderRepository.NameExistsInFolderAsync(targetFolderId, folder.OwnerId, folder.Type, folder.Name, excludeId: folder.Id))
            throw new ServiceException(409, "Ya existe una carpeta con ese nombre en el destino");

        folder.ParentFolderId = targetFolderId;
        if (folder.RootFolderId != newRootId)
        {
            folder.RootFolderId = newRootId;
            foreach (var descendant in descendants)
                descendant.RootFolderId = newRootId;
        }

        await folderRepository.SaveChangesAsync();
        return folder.ToDto();
    }

    public async Task<List<FolderTreeNodeDto>> GetFolderTreeAsync(Guid userId, Guid rootFolderId)
    {
        var folder = await folderRepository.GetByIdAsync(rootFolderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        await EnsureAccessAsync(folder, userId, requireWrite: true);

        var nodes = await folderRepository.GetByRootIdAsync(folder.RootFolderId);
        return nodes.Select(f => f.ToTreeNodeDto()).ToList();
    }

    public async Task<List<FolderMemberDto>> GetFolderMembersAsync(Guid userId, Guid folderId)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        await EnsureAccessAsync(folder, userId, requireWrite: false);

        var members = await folderRepository.GetMembersAsync(folder.RootFolderId);
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

        if (await folderRepository.GetMembershipAsync(folder.RootFolderId, target.Id) is not null)
            throw new ServiceException(409, "Ese usuario ya tiene acceso");

        var member = new FolderMember { FolderId = folder.RootFolderId, UserId = target.Id, Role = parsedRole };
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

        var member = await folderRepository.GetMembershipAsync(folder.RootFolderId, targetUserId)
            ?? throw new ServiceException(404, "Ese usuario no tiene acceso a la carpeta");

        folderRepository.RemoveMember(member);
        await folderRepository.SaveChangesAsync();
    }

    public Task EnsureUploadAllowedAsync(Guid userId, Guid folderId, long size) =>
        EnsureUploadAllowedInternalAsync(userId, folderId, size);

    private async Task<Folder> EnsureUploadAllowedInternalAsync(Guid userId, Guid folderId, long size)
    {
        var folder = await folderRepository.GetByIdAsync(folderId)
            ?? throw new ServiceException(404, "Carpeta no encontrada");

        await EnsureAccessAsync(folder, userId, requireWrite: true);

        var owner = await userRepository.GetByIdAsync(folder.OwnerId)
            ?? throw new ServiceException(404, "Propietario no encontrado");

        var used = await folderRepository.GetTotalSizeForOwnerAsync(folder.OwnerId);
        if (used + size > owner.StorageQuotaBytes)
            throw new ServiceException(413, "Se alcanzó el límite de almacenamiento");

        return folder;
    }

    public async Task<FileItemDto> UploadFileAsync(Guid userId, Guid folderId, string fileName, string? contentType, long size, Stream content)
    {
        var folder = await EnsureUploadAllowedInternalAsync(userId, folderId, size);

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

        await EnsureFileReadAccessAsync(file, userId);

        return (fileStorage.OpenRead(file.StoragePath), file.Name, file.MimeType ?? "application/octet-stream");
    }

    public async Task DeleteFileAsync(Guid userId, Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        await EnsureAccessAsync(file.Folder, userId, requireWrite: true);

        file.DeletedAt = DateTime.UtcNow;
        await fileRepository.SaveChangesAsync();
    }

    public async Task<FileItemDto> RenameFileAsync(Guid userId, Guid fileId, string newName)
    {
        newName = newName.Trim();
        if (string.IsNullOrWhiteSpace(newName))
            throw new ServiceException(400, "El nombre es obligatorio");

        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        await EnsureAccessAsync(file.Folder, userId, requireWrite: true);

        file.Name = newName;
        await fileRepository.SaveChangesAsync();
        return file.ToDto();
    }

    public async Task<FileItemDto> MoveFileAsync(Guid userId, Guid fileId, Guid targetFolderId)
    {
        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        await EnsureAccessAsync(file.Folder, userId, requireWrite: true);

        var target = await folderRepository.GetByIdAsync(targetFolderId)
            ?? throw new ServiceException(404, "Carpeta de destino no encontrada");

        await EnsureAccessAsync(target, userId, requireWrite: true);

        if (target.OwnerId != file.Folder.OwnerId)
            throw new ServiceException(400, "No puedes mover a una carpeta de otro dueño");

        file.FolderId = target.Id;
        await fileRepository.SaveChangesAsync();
        return file.ToDto();
    }

    public async Task<SearchResultsDto> SearchAsync(Guid userId, string query)
    {
        query = query.Trim();
        if (string.IsNullOrWhiteSpace(query))
            return new SearchResultsDto([], 0, query);

        var folders = await folderRepository.SearchFoldersAsync(userId, query);
        var files = await fileRepository.SearchFilesAsync(userId, query);

        var items = new List<SearchResultItemDto>();

        foreach (var folder in folders)
        {
            var source = folder.Type == FolderType.Group ? "group"
                : folder.OwnerId == userId ? "own"
                : "shared";
            items.Add(new SearchResultItemDto(
                folder.Id, folder.Name, "folder", null, null,
                folder.ParentFolderId, null,
                folder.Owner.Username, source));
        }

        foreach (var file in files)
        {
            var hasDirectShare = await fileRepository.GetFileShareAsync(file.Id, userId) is not null;
            var source = hasDirectShare ? "file_share"
                : file.Folder.Type == FolderType.Group ? "group"
                : file.Folder.OwnerId == userId ? "own"
                : "shared";
            items.Add(new SearchResultItemDto(
                file.Id, file.Name, "file", file.Size, file.MimeType,
                file.FolderId, file.Folder.Name,
                file.Folder.Owner.Username, source));
        }

        return new SearchResultsDto(
            items.OrderBy(i => i.Name).ToList(),
            items.Count,
            query);
    }

    public async Task<List<FileShareDto>> GetFileSharesAsync(Guid userId, Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        if (file.Folder.OwnerId != userId)
            throw new ServiceException(403, "Solo el dueño puede ver los compartidos de este archivo");

        var shares = await fileRepository.GetFileSharesAsync(fileId);
        return shares.Select(s => s.ToDto()).ToList();
    }

    public async Task<FileShareDto> AddFileShareAsync(Guid userId, Guid fileId, string username, string role)
    {
        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        if (file.Folder.OwnerId != userId)
            throw new ServiceException(403, "Solo el dueño puede compartir este archivo");

        if (!Enum.TryParse<FolderMemberRole>(role, true, out var parsedRole) || parsedRole != FolderMemberRole.Viewer)
            throw new ServiceException(400, "Rol inválido. Solo se permite 'viewer' para archivos");

        var target = await userRepository.GetByUsernameAsync(username.Trim())
            ?? throw new ServiceException(404, "Usuario no encontrado");

        if (target.Id == file.Folder.OwnerId)
            throw new ServiceException(400, "No puedes compartir el archivo contigo mismo");

        if (await fileRepository.GetFileShareAsync(fileId, target.Id) is not null)
            throw new ServiceException(409, "Ese usuario ya tiene acceso a este archivo");

        var share = new FileShareEntity { FileId = fileId, UserId = target.Id, Role = parsedRole };
        await fileRepository.AddFileShareAsync(share);
        await fileRepository.SaveChangesAsync();

        share.User = target;
        return share.ToDto();
    }

    public async Task RemoveFileShareAsync(Guid userId, Guid fileId, Guid targetUserId)
    {
        var file = await fileRepository.GetByIdAsync(fileId)
            ?? throw new ServiceException(404, "Archivo no encontrado");

        if (file.Folder.OwnerId != userId && userId != targetUserId)
            throw new ServiceException(403, "No tienes permiso para hacer esto");

        var share = await fileRepository.GetFileShareAsync(fileId, targetUserId)
            ?? throw new ServiceException(404, "El usuario no tiene acceso a este archivo");

        fileRepository.RemoveFileShare(share);
        await fileRepository.SaveChangesAsync();
    }

    public async Task<List<UserSummaryDto>> GetShareableUsersAsync(Guid userId, string? query)
    {
        var users = await userRepository.SearchShareableUsersAsync(userId, query);
        return users.Select(u => new UserSummaryDto(u.Id, u.Username)).ToList();
    }

    private async Task EnsureFileReadAccessAsync(FileEntity file, Guid userId)
    {
        try
        {
            await EnsureAccessAsync(file.Folder, userId, requireWrite: false);
            return;
        }
        catch (ServiceException) { }

        var share = await fileRepository.GetFileShareAsync(file.Id, userId);
        if (share is null)
            throw new ServiceException(403, "No tienes acceso a este archivo");
    }

    private async Task EnsureAccessAsync(Folder folder, Guid userId, bool requireWrite)
    {
        if (folder.OwnerId == userId) return;

        var membership = await folderRepository.GetMembershipAsync(folder.RootFolderId, userId);
        if (membership is null)
            throw new ServiceException(403, "No tienes acceso a esta carpeta");

        if (requireWrite && membership.Role != FolderMemberRole.Editor)
            throw new ServiceException(403, "No tienes permiso de edición en esta carpeta");
    }
}
