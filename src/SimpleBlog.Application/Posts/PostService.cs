using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.Application.Common;
using SimpleBlog.Core.Constants;
using SimpleBlog.Core.Entities;
using SimpleBlog.Core.Enums;

namespace SimpleBlog.Application.Posts;

public class PostService : IPostService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public PostService(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PostListItemDto>> GetPublishedAsync(PostQuery query, CancellationToken ct = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 50 ? 9 : query.PageSize;

        var q = _db.Posts
            .Include(p => p.Category)
            .Where(p => p.Status == PostStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(p => p.Title.Contains(term) || p.Summary.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var cat = query.Category.Trim();
            q = q.Where(p => p.Category != null && p.Category.Slug == cat);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PostListItemDto>
        {
            Items = items.Select(p => p.ToListItem()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<PostDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var post = await _db.Posts.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Post {id} not found.");

        EnsureCanView(post);
        return post.ToDto();
    }

    public async Task<PostDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var post = await _db.Posts.Include(p => p.Category).FirstOrDefaultAsync(p => p.Slug == slug, ct)
            ?? throw new NotFoundException($"Post '{slug}' not found.");

        EnsureCanView(post);
        return post.ToDto();
    }

    public async Task<IReadOnlyList<PostListItemDto>> GetMineAsync(CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var items = await _db.Posts
            .Include(p => p.Category)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(ct);

        return items.Select(p => p.ToListItem()).ToList();
    }

    public async Task<IReadOnlyList<PostListItemDto>> GetPendingAsync(CancellationToken ct = default)
    {
        EnsureAdmin();
        var items = await _db.Posts
            .Include(p => p.Category)
            .Where(p => p.Status == PostStatus.PendingReview)
            .OrderBy(p => p.UpdatedAt)
            .ToListAsync(ct);

        return items.Select(p => p.ToListItem()).ToList();
    }

    public async Task<PostDto> CreateAsync(CreatePostRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        Validate(request.Title, request.Summary, request.Content);

        var post = new Post
        {
            Title = request.Title.Trim(),
            Slug = await UniqueSlugAsync(request.Title, ct),
            Summary = request.Summary.Trim(),
            Content = request.Content,
            CoverImageUrl = request.CoverImageUrl,
            CategoryId = request.CategoryId,
            AuthorId = userId,
            AuthorName = _currentUser.UserName ?? "Unknown",
            Status = PostStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(post.Id, ct);
    }

    public async Task<PostDto> UpdateAsync(int id, UpdatePostRequest request, CancellationToken ct = default)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Post {id} not found.");

        EnsureCanEdit(post);
        Validate(request.Title, request.Summary, request.Content);

        post.Title = request.Title.Trim();
        post.Summary = request.Summary.Trim();
        post.Content = request.Content;
        post.CoverImageUrl = request.CoverImageUrl;
        post.CategoryId = request.CategoryId;
        post.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(post.Id, ct);
    }

    public async Task<PostDto> SubmitAsync(int id, CancellationToken ct = default)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Post {id} not found.");

        EnsureOwnerOrAdmin(post);

        if (post.Status is not (PostStatus.Draft or PostStatus.Rejected))
            throw new ConflictException("Only draft or rejected posts can be submitted for review.");

        post.Status = PostStatus.PendingReview;
        post.RejectionReason = null;
        post.UpdatedAt = DateTime.UtcNow;

        _db.OutboxMessages.Add(new OutboxMessage
        {
            Type = "PostSubmitted",
            Payload = JsonSerializer.Serialize(new { eventType = "PostSubmitted", postId = post.Id, authorId = post.AuthorId })
        });

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(post.Id, ct);
    }

    public async Task<PostDto> ApproveAsync(int id, CancellationToken ct = default)
    {
        EnsureAdmin();
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Post {id} not found.");

        if (post.Status != PostStatus.PendingReview)
            throw new ConflictException("Only posts pending review can be approved.");

        post.Status = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;

        _db.Reviews.Add(new Review
        {
            PostId = post.Id,
            ReviewerId = RequireUserId(),
            ReviewerName = _currentUser.UserName ?? "Admin",
            Decision = ReviewDecision.Approved
        });

        _db.Notifications.Add(new Notification
        {
            UserId = post.AuthorId,
            Title = "Post published",
            Message = $"Your post \"{post.Title}\" has been approved and published.",
            LinkUrl = $"/blog/{post.Slug}"
        });

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(post.Id, ct);
    }

    public async Task<PostDto> RejectAsync(int id, string reason, CancellationToken ct = default)
    {
        EnsureAdmin();
        if (string.IsNullOrWhiteSpace(reason))
            throw new AppValidationException("A rejection reason is required.");

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Post {id} not found.");

        if (post.Status != PostStatus.PendingReview)
            throw new ConflictException("Only posts pending review can be rejected.");

        post.Status = PostStatus.Rejected;
        post.RejectionReason = reason.Trim();
        post.UpdatedAt = DateTime.UtcNow;

        _db.Reviews.Add(new Review
        {
            PostId = post.Id,
            ReviewerId = RequireUserId(),
            ReviewerName = _currentUser.UserName ?? "Admin",
            Decision = ReviewDecision.Rejected,
            Note = reason.Trim()
        });

        _db.Notifications.Add(new Notification
        {
            UserId = post.AuthorId,
            Title = "Post needs changes",
            Message = $"Your post \"{post.Title}\" was rejected: {reason.Trim()}",
            LinkUrl = "/dashboard/posts"
        });

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(post.Id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException($"Post {id} not found.");

        EnsureOwnerOrAdmin(post);
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync(ct);
    }

    // ---- helpers -------------------------------------------------------

    private static void Validate(string title, string summary, string content)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3)
            errors["title"] = new[] { "Title must be at least 3 characters." };
        if (string.IsNullOrWhiteSpace(summary))
            errors["summary"] = new[] { "Summary is required." };
        if (string.IsNullOrWhiteSpace(content))
            errors["content"] = new[] { "Content is required." };
        if (errors.Count > 0)
            throw new AppValidationException(errors);
    }

    private async Task<string> UniqueSlugAsync(string title, CancellationToken ct)
    {
        var baseSlug = SlugGenerator.Generate(title);
        var slug = baseSlug;
        var i = 1;
        while (await _db.Posts.AnyAsync(p => p.Slug == slug, ct))
            slug = $"{baseSlug}-{i++}";
        return slug;
    }

    private string RequireUserId() =>
        _currentUser.Id ?? throw new ForbiddenException("Authentication required.");

    private void EnsureAdmin()
    {
        if (!_currentUser.IsInRole(Roles.Admin))
            throw new ForbiddenException("Admin role required.");
    }

    private void EnsureCanView(Post post)
    {
        if (post.Status == PostStatus.Published) return;
        if (_currentUser.IsInRole(Roles.Admin)) return;
        if (_currentUser.Id is not null && _currentUser.Id == post.AuthorId) return;
        throw new ForbiddenException("This post is not available.");
    }

    private void EnsureCanEdit(Post post)
    {
        if (_currentUser.IsInRole(Roles.Admin)) return;
        if (_currentUser.Id == post.AuthorId && post.Status != PostStatus.Published) return;
        throw new ForbiddenException("You can only edit your own posts before they are published.");
    }

    private void EnsureOwnerOrAdmin(Post post)
    {
        if (_currentUser.IsInRole(Roles.Admin)) return;
        if (_currentUser.Id == post.AuthorId) return;
        throw new ForbiddenException("You can only manage your own posts.");
    }
}
