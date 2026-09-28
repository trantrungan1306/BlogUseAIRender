namespace SimpleBlog.Application.Common;

public interface ICurrentUser
{
    string? Id { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
