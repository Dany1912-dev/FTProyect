using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;

namespace FtpCloud.Application.Interfaces;

public interface IFolderRepository
{
    Task<List<Folder>> GetByOwnerAndTypeAsync(Guid ownerId, FolderType type);
    Task<List<Folder>> GetAllByOwnerAsync(Guid ownerId);
    Task<Folder?> GetByIdAsync(Guid id);
    Task<List<Folder>> GetSharedWithUserAsync(Guid userId);
    Task<List<Folder>> GetGroupsForUserAsync(Guid userId);
    Task<List<Folder>> GetSubfoldersAsync(Guid parentFolderId);
    Task<List<Folder>> GetAncestorsAsync(Guid folderId);
    Task<List<Folder>> GetTreeByOwnerAsync(Guid ownerId);
    Task<List<Folder>> GetByRootIdAsync(Guid rootFolderId);
    Task<List<Folder>> GetSubfoldersIncludingDeletedAsync(Guid parentFolderId);
    Task<List<Folder>> GetTrashedAsync(Guid userId);
    Task<List<Folder>> SearchFoldersAsync(Guid userId, string query, int maxResults = 20);
    Task<long> GetTotalSizeForOwnerAsync(Guid ownerId);
    Task<bool> NameExistsInFolderAsync(Guid? parentFolderId, Guid ownerId, FolderType type, string name, Guid? excludeId = null);
    Task<FolderMember?> GetMembershipAsync(Guid folderId, Guid userId);
    Task<List<FolderMember>> GetMembersAsync(Guid folderId);
    Task AddMemberAsync(FolderMember member);
    void RemoveMember(FolderMember member);
    Task AddAsync(Folder folder);
    void Remove(Folder folder);
    Task SaveChangesAsync();
}
