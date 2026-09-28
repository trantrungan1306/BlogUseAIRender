using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SimpleBlog.Application.Auth;
using SimpleBlog.Application.Common;
using SimpleBlog.Core.Constants;

namespace SimpleBlog.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService, IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new AppValidationException("Email and password are required.");

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Email.Split('@')[0] : request.DisplayName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            throw new AppValidationException(errors);
        }

        await _userManager.AddToRoleAsync(user, Roles.Blogger);
        return await BuildResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new AppValidationException("Invalid email or password.");

        return await BuildResponseAsync(user);
    }

    public async Task<UserDto?> GetCurrentAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        return ToDto(user, roles);
    }

    public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            throw new AppValidationException("A Google id token is required.");

        var clientId = _configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ConflictException("Google sign-in is not configured on the server.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { clientId } };
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch
        {
            throw new AppValidationException("Invalid Google token.");
        }

        var user = await _userManager.FindByLoginAsync("Google", payload.Subject);
        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(payload.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = true,
                    DisplayName = payload.Name ?? payload.Email.Split('@')[0],
                    GoogleId = payload.Subject,
                    AvatarUrl = payload.Picture,
                    CreatedAt = DateTime.UtcNow
                };
                var created = await _userManager.CreateAsync(user);
                if (!created.Succeeded)
                    throw new AppValidationException(created.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
                await _userManager.AddToRoleAsync(user, Roles.Blogger);
            }
            else
            {
                user.GoogleId = payload.Subject;
                user.AvatarUrl ??= payload.Picture;
                await _userManager.UpdateAsync(user);
            }

            await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));
        }

        return await BuildResponseAsync(user);
    }

    private async Task<AuthResponse> BuildResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenService.CreateToken(user.Id, user.Email!, user.DisplayName, roles);
        return new AuthResponse(token, expiresAt, ToDto(user, roles));
    }

    private static UserDto ToDto(ApplicationUser user, IEnumerable<string> roles) =>
        new(user.Id, user.Email ?? string.Empty, user.DisplayName, user.AvatarUrl, roles.ToList());
}
