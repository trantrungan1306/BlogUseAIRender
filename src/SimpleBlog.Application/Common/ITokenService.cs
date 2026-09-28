using SimpleBlog.Application.Auth;

namespace SimpleBlog.Application.Common;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(string userId, string email, string displayName, IEnumerable<string> roles);
}
