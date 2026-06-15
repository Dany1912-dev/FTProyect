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
}
