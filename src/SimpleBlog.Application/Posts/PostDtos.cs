using SimpleBlog.Core.Enums;

namespace SimpleBlog.Application.Posts;

public record PostListItemDto(
    int Id,
    string Title,
    string Slug,
    string Summary,
    string? CoverImageUrl,
    string Status,
    string AuthorName,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime? PublishedAt);

public record PostDto(
    int Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    string? CoverImageUrl,
    string Status,
    string AuthorId,
    string AuthorName,
    int? CategoryId,
    string? CategoryName,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt);

public record CreatePostRequest(
    string Title,
    string Summary,
    string Content,
    string? CoverImageUrl,
    int? CategoryId);

public record UpdatePostRequest(
    string Title,
    string Summary,
    string Content,
    string? CoverImageUrl,
    int? CategoryId);

public record RejectPostRequest(string Reason);

public record PostQuery(string? Search, string? Category, int Page = 1, int PageSize = 9);
