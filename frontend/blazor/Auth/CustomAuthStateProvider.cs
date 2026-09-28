using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace SimpleBlog.Blazor.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "simpleblog_token";
    private readonly ILocalStorageService _localStorage;
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync(TokenKey);
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        var claims = ParseClaims(token);
        var expiry = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (expiry is not null && long.TryParse(expiry, out var seconds))
        {
            var expiryTime = DateTimeOffset.FromUnixTimeSeconds(seconds);
            if (expiryTime < DateTimeOffset.UtcNow)
            {
                await _localStorage.RemoveItemAsync(TokenKey);
                return Anonymous;
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await _localStorage.SetItemAsStringAsync(TokenKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static IEnumerable<Claim> ParseClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return Enumerable.Empty<Claim>();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims;
    }
}
