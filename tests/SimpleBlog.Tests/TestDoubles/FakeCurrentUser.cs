using SimpleBlog.Application.Common;

namespace SimpleBlog.Tests.TestDoubles;

public class FakeCurrentUser : ICurrentUser
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public HashSet<string> RoleSet { get; } = new();

    public bool IsAuthenticated => Id is not null;
    public bool IsInRole(string role) => RoleSet.Contains(role);

    public static FakeCurrentUser As(string id, string name, params string[] roles)
    {
        var user = new FakeCurrentUser { Id = id, UserName = name };
        foreach (var role in roles)
            user.RoleSet.Add(role);
        return user;
    }

    public static FakeCurrentUser Anonymous() => new();
}
