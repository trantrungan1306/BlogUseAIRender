using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.Core.Constants;
using SimpleBlog.Core.Entities;
using SimpleBlog.Infrastructure.Identity;
using SimpleBlog.Infrastructure.Persistence;

namespace SimpleBlog.Worker;

// Turns an integration event into in-app notifications. Shared by the in-process
// outbox path and the Azure Service Bus consumer.
public class NotificationDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationDispatcher> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public NotificationDispatcher(IServiceScopeFactory scopeFactory, ILogger<NotificationDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task DispatchAsync(string type, string payload, CancellationToken ct = default)
    {
        if (type != "PostSubmitted")
            return;

        var evt = JsonSerializer.Deserialize<PostSubmittedEvent>(payload, JsonOptions);
        if (evt is null)
            return;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == evt.PostId, ct);
        if (post is null)
            return;

        var admins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        var added = 0;
        foreach (var admin in admins)
        {
            var already = await db.Notifications.AnyAsync(
                n => n.UserId == admin.Id && n.LinkUrl == "/admin" && n.Message.Contains(post.Title), ct);
            if (already)
                continue;

            db.Notifications.Add(new Notification
            {
                UserId = admin.Id,
                Title = "New post awaiting review",
                Message = $"\"{post.Title}\" by {post.AuthorName} was submitted for review.",
                LinkUrl = "/admin"
            });
            added++;
        }

        if (added > 0)
            await db.SaveChangesAsync(ct);

        _logger.LogInformation("Dispatched {Count} notification(s) for post {PostId}.", added, post.Id);
    }

    private record PostSubmittedEvent(string EventType, int PostId, string AuthorId);
}
