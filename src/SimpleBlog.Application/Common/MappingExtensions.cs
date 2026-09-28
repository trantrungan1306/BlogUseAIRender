using SimpleBlog.Application.Categories;
using SimpleBlog.Application.Posts;
using SimpleBlog.Core.Entities;

namespace SimpleBlog.Application.Common;

public static class MappingExtensions
{
    public static PostDto ToDto(this Post p) => new(
        p.Id, p.Title, p.Slug, p.Summary, p.Content, p.CoverImageUrl,
        p.Status.ToString(), p.AuthorId, p.AuthorName,
        p.CategoryId, p.Category?.Name, p.RejectionReason,
        p.CreatedAt, p.UpdatedAt, p.PublishedAt);

    public static PostListItemDto ToListItem(this Post p) => new(
        p.Id, p.Title, p.Slug, p.Summary, p.CoverImageUrl,
        p.Status.ToString(), p.AuthorName, p.Category?.Name,
        p.CreatedAt, p.PublishedAt);

    public static CategoryDto ToDto(this Category c, int postCount) =>
        new(c.Id, c.Name, c.Slug, c.Description, postCount);
}
