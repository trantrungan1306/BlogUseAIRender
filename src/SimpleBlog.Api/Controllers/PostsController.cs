using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Application.Common;
using SimpleBlog.Application.Posts;
using SimpleBlog.Core.Constants;

namespace SimpleBlog.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _posts;

    public PostsController(IPostService posts)
    {
        _posts = posts;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PostListItemDto>>> GetPublished([FromQuery] PostQuery query, CancellationToken ct)
        => Ok(await _posts.GetPublishedAsync(query, ct));

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<PostListItemDto>>> Mine(CancellationToken ct)
        => Ok(await _posts.GetMineAsync(ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<PostListItemDto>>> Pending(CancellationToken ct)
        => Ok(await _posts.GetPendingAsync(ct));

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<PostDto>> GetBySlug(string slug, CancellationToken ct)
        => Ok(await _posts.GetBySlugAsync(slug, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetById(int id, CancellationToken ct)
        => Ok(await _posts.GetByIdAsync(id, ct));

    [Authorize(Roles = $"{Roles.Blogger},{Roles.Admin}")]
    [HttpPost]
    public async Task<ActionResult<PostDto>> Create(CreatePostRequest request, CancellationToken ct)
    {
        var post = await _posts.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostDto>> Update(int id, UpdatePostRequest request, CancellationToken ct)
        => Ok(await _posts.UpdateAsync(id, request, ct));

    [Authorize]
    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<PostDto>> Submit(int id, CancellationToken ct)
        => Ok(await _posts.SubmitAsync(id, ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<PostDto>> Approve(int id, CancellationToken ct)
        => Ok(await _posts.ApproveAsync(id, ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<PostDto>> Reject(int id, RejectPostRequest request, CancellationToken ct)
        => Ok(await _posts.RejectAsync(id, request.Reason, ct));

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _posts.DeleteAsync(id, ct);
        return NoContent();
    }
}
