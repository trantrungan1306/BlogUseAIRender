namespace SimpleBlog.Core.Entities;

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }

    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
