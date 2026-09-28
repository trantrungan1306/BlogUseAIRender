namespace SimpleBlog.Blazor.Models;

public static class Roles
{
    public const string Viewer = "Viewer";
    public const string Blogger = "Blogger";
    public const string Admin = "Admin";
}

public record UserDto(string Id, string Email, string DisplayName, string? AvatarUrl, List<string> Roles);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string DisplayName);

public record PostListItem(
    int Id, string Title, string Slug, string Summary, string? CoverImageUrl,
    string Status, string AuthorName, string? CategoryName, DateTime CreatedAt, DateTime? PublishedAt);

public record PostDetail(
    int Id, string Title, string Slug, string Summary, string Content, string? CoverImageUrl,
    string Status, string AuthorId, string AuthorName, int? CategoryId, string? CategoryName,
    string? RejectionReason, DateTime CreatedAt, DateTime UpdatedAt, DateTime? PublishedAt);

public record PagedResult<T>(
    List<T> Items, int Page, int PageSize, int TotalCount, int TotalPages, bool HasPrevious, bool HasNext);

public record CategoryDto(int Id, string Name, string Slug, string? Description, int PostCount);

public record CreatePostRequest(string Title, string Summary, string Content, string? CoverImageUrl, int? CategoryId);

public record RejectRequest(string Reason);
