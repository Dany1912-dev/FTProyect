using FtpCloud.Api.Common;
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
        ApiOk(await fileService.CreateFolderAsync(User.GetUserId(), request.Name));

    [HttpDelete("folders/{id:guid}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
        await fileService.DeleteFolderAsync(User.GetUserId(), id);
        return ApiOkEmpty();
    }

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

    [HttpPost("upload"), RequestSizeLimit(500_000_000)]
    public async Task<ActionResult<ApiResponse<FileItemDto>>> Upload([FromForm] Guid folderId, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var dto = await fileService.UploadFileAsync(
            User.GetUserId(), folderId, file.FileName, file.ContentType, file.Length, stream);
        return ApiOk(dto);
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
}
