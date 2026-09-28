namespace SimpleBlog.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken ct = default);
    Task<UserDto?> GetCurrentAsync(string userId, CancellationToken ct = default);
}
