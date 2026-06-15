using FtpCloud.Application.Interfaces;
using FtpCloud.Domain.Entities;
using FtpCloud.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FtpCloud.Infrastructure.Persistence.Repositories;

public class FolderRepository(FtpCloudDbContext db) : IFolderRepository
{
    public Task<List<Folder>> GetByOwnerAndTypeAsync(Guid ownerId, FolderType type) =>
        db.Folders.Include(f => f.Owner)
            .Where(f => f.OwnerId == ownerId && f.Type == type)
            .OrderBy(f => f.Name).ToListAsync();

    public Task<List<Folder>> GetAllByOwnerAsync(Guid ownerId) =>
        db.Folders.Where(f => f.OwnerId == ownerId).ToListAsync();

    public Task<Folder?> GetByIdAsync(Guid id) =>
        db.Folders.Include(f => f.Owner).FirstOrDefaultAsync(f => f.Id == id);

    public Task<List<Folder>> GetSharedWithUserAsync(Guid userId) =>
        db.Folders.Include(f => f.Owner)
            .Where(f => f.Type == FolderType.Personal && f.OwnerId != userId
                        && f.Members.Any(m => m.UserId == userId))
            .OrderBy(f => f.Name).ToListAsync();

    public Task<List<Folder>> GetGroupsForUserAsync(Guid userId) =>
        db.Folders.Include(f => f.Owner)
            .Where(f => f.Type == FolderType.Group
                        && (f.OwnerId == userId || f.Members.Any(m => m.UserId == userId)))
            .OrderBy(f => f.Name).ToListAsync();

    public Task<bool> NameExistsForOwnerAndTypeAsync(Guid ownerId, string name, FolderType type) =>
        db.Folders.AnyAsync(f => f.OwnerId == ownerId && f.Type == type && f.Name == name);

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
