namespace SimpleBlog.Core.Constants;

public static class Roles
{
    public const string Viewer = "Viewer";
    public const string Blogger = "Blogger";
    public const string Admin = "Admin";

    public static readonly string[] All = { Viewer, Blogger, Admin };
}
