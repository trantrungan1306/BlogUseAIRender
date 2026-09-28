namespace SimpleBlog.Application.Auth;

public record RegisterRequest(string Email, string Password, string DisplayName);

public record LoginRequest(string Email, string Password);

public record GoogleLoginRequest(string IdToken);

public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    IReadOnlyList<string> Roles);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
