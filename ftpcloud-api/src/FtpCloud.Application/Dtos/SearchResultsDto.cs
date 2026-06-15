namespace FtpCloud.Application.Dtos;

public record SearchResultItemDto(
    Guid Id,
    string Name,
    string Kind,
    long? Size,
    string? MimeType,
    Guid? ParentId,
    string? ParentName,
    string OwnerUsername,
    string Source
);

public record SearchResultsDto(
    List<SearchResultItemDto> Items,
    int TotalCount,
    string Query
);
