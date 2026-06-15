using FtpCloud.Application.Interfaces;
using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FtpCloud.Infrastructure.Persistence.Repositories;

public class FolderRepository(FtpCloudDbContext db) : IFolderRepository
{
    public Task<List<Folder>> GetByOwnerAndTypeAsync(Guid ownerId, FolderType type) =>
        db.Folders.Include(f => f.Owner)
            .Where(f => f.OwnerId == ownerId && f.Type == type && f.ParentFolderId == null)
            .OrderBy(f => f.Name).ToListAsync();

    public Task<List<Folder>> GetAllByOwnerAsync(Guid ownerId) =>
        db.Folders.Where(f => f.OwnerId == ownerId).ToListAsync();

    public Task<Folder?> GetByIdAsync(Guid id) =>
        db.Folders.Include(f => f.Owner).FirstOrDefaultAsync(f => f.Id == id);

    public Task<List<Folder>> GetSharedWithUserAsync(Guid userId) =>
        db.Folders.Include(f => f.Owner)
            .Where(f => f.Type == FolderType.Personal && f.OwnerId != userId && f.ParentFolderId == null
                        && f.Members.Any(m => m.UserId == userId))
            .OrderBy(f => f.Name).ToListAsync();

    public Task<List<Folder>> GetGroupsForUserAsync(Guid userId) =>
        db.Folders.Include(f => f.Owner)
            .Where(f => f.Type == FolderType.Group && f.ParentFolderId == null
                        && (f.OwnerId == userId || f.Members.Any(m => m.UserId == userId)))
            .OrderBy(f => f.Name).ToListAsync();

    public Task<List<Folder>> GetSubfoldersAsync(Guid parentFolderId) =>
        db.Folders.Include(f => f.Owner)
            .Where(f => f.ParentFolderId == parentFolderId)
            .OrderBy(f => f.Name).ToListAsync();

    public async Task<List<Folder>> GetAncestorsAsync(Guid folderId)
    {
        var folder = await db.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == folderId);
        if (folder is null) return [];

        var all = await db.Folders.Include(f => f.Owner)
            .Where(f => f.OwnerId == folder.OwnerId).ToListAsync();
        var byId = all.ToDictionary(f => f.Id);

        var ancestors = new List<Folder>();
        var current = folder;
        while (current.ParentFolderId is not null && byId.TryGetValue(current.ParentFolderId.Value, out var parent))
        {
            ancestors.Add(parent);
            current = parent;
        }
        ancestors.Reverse();
        return ancestors;
    }

    public Task<List<Folder>> GetTreeByOwnerAsync(Guid ownerId) =>
        db.Folders.Where(f => f.OwnerId == ownerId).ToListAsync();

    public Task<List<Folder>> GetByRootIdAsync(Guid rootFolderId) =>
        db.Folders.Where(f => f.RootFolderId == rootFolderId).ToListAsync();

    public async Task<long> GetTotalSizeForOwnerAsync(Guid ownerId) =>
        await db.Files.Where(f => f.Folder.OwnerId == ownerId).SumAsync(f => (long?)f.Size) ?? 0;

    public Task<bool> NameExistsInFolderAsync(Guid? parentFolderId, Guid ownerId, FolderType type, string name, Guid? excludeId = null) =>
        db.Folders.AnyAsync(f => f.ParentFolderId == parentFolderId && f.OwnerId == ownerId && f.Type == type
                                  && f.Name == name && f.Id != (excludeId ?? Guid.Empty));

    public Task<FolderMember?> GetMembershipAsync(Guid folderId, Guid userId) =>
        db.FolderMembers.FirstOrDefaultAsync(m => m.FolderId == folderId && m.UserId == userId);

    public Task<List<FolderMember>> GetMembersAsync(Guid folderId) =>
        db.FolderMembers.Include(m => m.User)
            .Where(m => m.FolderId == folderId).OrderBy(m => m.User.Username).ToListAsync();

    public async Task AddMemberAsync(FolderMember member) =>
        await db.FolderMembers.AddAsync(member);

    public void RemoveMember(FolderMember member) =>
        db.FolderMembers.Remove(member);

    public async Task AddAsync(Folder folder) =>
        await db.Folders.AddAsync(folder);

    public void Remove(Folder folder) =>
        db.Folders.Remove(folder);

    public Task SaveChangesAsync() =>
        db.SaveChangesAsync();
}
