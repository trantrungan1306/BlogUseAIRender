using SimpleBlog.Core.Enums;

namespace SimpleBlog.Core.Entities;

public class Review
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }

    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public ReviewDecision Decision { get; set; }
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
