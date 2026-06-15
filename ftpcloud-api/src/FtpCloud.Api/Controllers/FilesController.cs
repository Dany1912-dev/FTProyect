using FtpCloud.Api.Common;
using FtpCloud.Application.Common;
using FtpCloud.Application.Dtos;
using FtpCloud.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FtpCloud.Api.Controllers;

[Route("api/files"), Authorize]
public class FilesController(IFileService fileService) : ApiControllerBase
{
    [HttpGet("personal")]
    public async Task<ActionResult<ApiResponse<FolderContentsDto>>> GetPersonal([FromQuery] Guid? folderId) =>
        ApiOk(await fileService.GetPersonalAsync(User.GetUserId(), folderId));

    [HttpGet("shared")]
    public async Task<ActionResult<ApiResponse<FolderContentsDto>>> GetShared([FromQuery] Guid? folderId) =>
        ApiOk(await fileService.GetSharedAsync(User.GetUserId(), folderId));

    [HttpGet("groups")]
    public async Task<ActionResult<ApiResponse<FolderContentsDto>>> GetGroups([FromQuery] Guid? folderId) =>
        ApiOk(await fileService.GetGroupsAsync(User.GetUserId(), folderId));

    [HttpPost("groups")]
    public async Task<ActionResult<ApiResponse<FolderDto>>> CreateGroup(CreateFolderRequest request) =>
        ApiOk(await fileService.CreateGroupAsync(User.GetUserId(), request.Name));

    [HttpPost("folders")]
    public async Task<ActionResult<ApiResponse<FolderDto>>> CreateFolder(CreateFolderRequest request) =>
        ApiOk(await fileService.CreateFolderAsync(User.GetUserId(), request.Name, request.ParentFolderId));

    [HttpDelete("folders/{id:guid}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
        await fileService.DeleteFolderAsync(User.GetUserId(), id);
        return ApiOkEmpty();
    }

    [HttpPut("folders/{id:guid}")]
    public async Task<ActionResult<ApiResponse<FolderDto>>> RenameFolder(Guid id, RenameRequest request) =>
        ApiOk(await fileService.RenameFolderAsync(User.GetUserId(), id, request.Name));

    [HttpPut("folders/{id:guid}/move")]
    public async Task<ActionResult<ApiResponse<FolderDto>>> MoveFolder(Guid id, MoveRequest request) =>
        ApiOk(await fileService.MoveFolderAsync(User.GetUserId(), id, request.TargetFolderId));

    [HttpGet("folders/{id:guid}/tree")]
    public async Task<ActionResult<ApiResponse<List<FolderTreeNodeDto>>>> GetFolderTree(Guid id) =>
        ApiOk(await fileService.GetFolderTreeAsync(User.GetUserId(), id));

    [HttpGet("folders/{id:guid}/members")]
    public async Task<ActionResult<ApiResponse<List<FolderMemberDto>>>> GetMembers(Guid id) =>
        ApiOk(await fileService.GetFolderMembersAsync(User.GetUserId(), id));

    [HttpPost("folders/{id:guid}/members")]
    public async Task<ActionResult<ApiResponse<FolderMemberDto>>> AddMember(Guid id, AddFolderMemberRequest request) =>
        ApiOk(await fileService.AddFolderMemberAsync(User.GetUserId(), id, request.Username, request.Role));

    [HttpDelete("folders/{id:guid}/members/{targetUserId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid targetUserId)
    {
        await fileService.RemoveFolderMemberAsync(User.GetUserId(), id, targetUserId);
        return ApiOkEmpty();
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var (stream, fileName, mimeType) = await fileService.DownloadFileAsync(User.GetUserId(), id);
        return File(stream, mimeType, fileName);
    }

    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id)
    {
        var (stream, _, mimeType) = await fileService.DownloadFileAsync(User.GetUserId(), id);
        return File(stream, mimeType);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        await fileService.DeleteFileAsync(User.GetUserId(), id);
        return ApiOkEmpty();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<FileItemDto>>> RenameFile(Guid id, RenameRequest request) =>
        ApiOk(await fileService.RenameFileAsync(User.GetUserId(), id, request.Name));

    [HttpPut("{id:guid}/move")]
    public async Task<ActionResult<ApiResponse<FileItemDto>>> MoveFile(Guid id, MoveRequest request)
    {
        if (request.TargetFolderId is null)
            throw new ServiceException(400, "Debes indicar la carpeta de destino");

        return ApiOk(await fileService.MoveFileAsync(User.GetUserId(), id, request.TargetFolderId.Value));
    }

    [HttpGet("trash")]
    public async Task<ActionResult<ApiResponse<TrashContentsDto>>> GetTrash() =>
        ApiOk(await fileService.GetTrashAsync(User.GetUserId()));

    [HttpPost("trash/folders/{id:guid}/restore")]
    public async Task<IActionResult> RestoreFolder(Guid id)
    {
        await fileService.RestoreFolderAsync(User.GetUserId(), id);
        return ApiOkEmpty();
    }

    [HttpPost("trash/files/{id:guid}/restore")]
    public async Task<IActionResult> RestoreFile(Guid id)
    {
        await fileService.RestoreFileAsync(User.GetUserId(), id);
        return ApiOkEmpty();
    }

    [HttpDelete("trash/folders/{id:guid}")]
    public async Task<IActionResult> PermanentlyDeleteFolder(Guid id)
    {
        await fileService.PermanentlyDeleteFolderAsync(User.GetUserId(), id);
        return ApiOkEmpty();
    }

    [HttpDelete("trash/files/{id:guid}")]
    public async Task<IActionResult> PermanentlyDeleteFile(Guid id)
    {
        await fileService.PermanentlyDeleteFileAsync(User.GetUserId(), id);
        return ApiOkEmpty();
    }

    [HttpDelete("trash")]
    public async Task<IActionResult> EmptyTrash()
    {
        await fileService.EmptyTrashAsync(User.GetUserId());
        return ApiOkEmpty();
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<SearchResultsDto>>> Search([FromQuery] string q) =>
        ApiOk(await fileService.SearchAsync(User.GetUserId(), q));

    [HttpGet("shareable-users")]
    public async Task<ActionResult<ApiResponse<List<UserSummaryDto>>>> GetShareableUsers([FromQuery] string? q) =>
        ApiOk(await fileService.GetShareableUsersAsync(User.GetUserId(), q));

    [HttpGet("{id:guid}/shares")]
    public async Task<ActionResult<ApiResponse<List<FileShareDto>>>> GetFileShares(Guid id) =>
        ApiOk(await fileService.GetFileSharesAsync(User.GetUserId(), id));

    [HttpPost("{id:guid}/shares")]
    public async Task<ActionResult<ApiResponse<FileShareDto>>> AddFileShare(Guid id, AddFileShareRequest request) =>
        ApiOk(await fileService.AddFileShareAsync(User.GetUserId(), id, request.Username, request.Role));

    [HttpDelete("{id:guid}/shares/{targetUserId:guid}")]
    public async Task<IActionResult> RemoveFileShare(Guid id, Guid targetUserId)
    {
        await fileService.RemoveFileShareAsync(User.GetUserId(), id, targetUserId);
        return ApiOkEmpty();
    }
}
