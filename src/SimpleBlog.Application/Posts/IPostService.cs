using SimpleBlog.Application.Common;

namespace SimpleBlog.Application.Posts;

public interface IPostService
{
    Task<PagedResult<PostListItemDto>> GetPublishedAsync(PostQuery query, CancellationToken ct = default);
    Task<PostDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PostDto> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<PostListItemDto>> GetMineAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PostListItemDto>> GetPendingAsync(CancellationToken ct = default);

    Task<PostDto> CreateAsync(CreatePostRequest request, CancellationToken ct = default);
    Task<PostDto> UpdateAsync(int id, UpdatePostRequest request, CancellationToken ct = default);
    Task<PostDto> SubmitAsync(int id, CancellationToken ct = default);
    Task<PostDto> ApproveAsync(int id, CancellationToken ct = default);
    Task<PostDto> RejectAsync(int id, string reason, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
