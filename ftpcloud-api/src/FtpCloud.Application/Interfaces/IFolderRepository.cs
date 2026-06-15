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
    Task<bool> NameExistsForOwnerAndTypeAsync(Guid ownerId, string name, FolderType type);
    Task<FolderMember?> GetMembershipAsync(Guid folderId, Guid userId);
    Task<List<FolderMember>> GetMembersAsync(Guid folderId);
    Task AddMemberAsync(FolderMember member);
    void RemoveMember(FolderMember member);
    Task AddAsync(Folder folder);
    void Remove(Folder folder);
    Task SaveChangesAsync();
}
