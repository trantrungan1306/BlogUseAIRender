using Microsoft.EntityFrameworkCore;
using SimpleBlog.Application.Common;
using SimpleBlog.Core.Entities;

namespace SimpleBlog.Application.Notifications;

public record NotificationDto(int Id, string Title, string Message, string? LinkUrl, bool IsRead, DateTime CreatedAt);

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetMineAsync(CancellationToken ct = default);
    Task MarkReadAsync(int id, CancellationToken ct = default);
}

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public NotificationService(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetMineAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.Id ?? throw new ForbiddenException("Authentication required.");
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.LinkUrl, n.IsRead, n.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task MarkReadAsync(int id, CancellationToken ct = default)
    {
        var userId = _currentUser.Id ?? throw new ForbiddenException("Authentication required.");
        var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct)
            ?? throw new NotFoundException("Notification not found.");
        notification.IsRead = true;
        await _db.SaveChangesAsync(ct);
    }
}
