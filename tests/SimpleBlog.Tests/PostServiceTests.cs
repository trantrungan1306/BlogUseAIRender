using Microsoft.EntityFrameworkCore;
using SimpleBlog.Application.Common;
using SimpleBlog.Application.Posts;
using SimpleBlog.Core.Constants;
using SimpleBlog.Infrastructure.Persistence;
using SimpleBlog.Tests.TestDoubles;
using Xunit;

namespace SimpleBlog.Tests;

public class PostServiceTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"blog-{Guid.NewGuid()}")
            .Options);

    private static PostService ServiceFor(AppDbContext db, FakeCurrentUser user) =>
        new(db, user, new InMemoryCacheService());

    private static CreatePostRequest SampleRequest() =>
        new("My first post", "A short summary", "# Hello\n\nBody content.", null, null);

    [Fact]
    public async Task CreatePost_Should_Create_Draft()
    {
        using var db = CreateContext();
        var blogger = FakeCurrentUser.As("u1", "Blogger One", Roles.Blogger);
        var service = ServiceFor(db, blogger);

        var post = await service.CreateAsync(SampleRequest());

        Assert.Equal("Draft", post.Status);
        Assert.Equal("u1", post.AuthorId);
    }

    [Fact]
    public async Task SubmitPost_Should_Change_Status_To_PendingReview()
    {
        using var db = CreateContext();
        var blogger = FakeCurrentUser.As("u1", "Blogger One", Roles.Blogger);
        var service = ServiceFor(db, blogger);

        var created = await service.CreateAsync(SampleRequest());
        var submitted = await service.SubmitAsync(created.Id);

        Assert.Equal("PendingReview", submitted.Status);
    }

    [Fact]
    public async Task Blogger_Should_Not_Be_Able_To_Approve()
    {
        using var db = CreateContext();
        var blogger = FakeCurrentUser.As("u1", "Blogger One", Roles.Blogger);
        var service = ServiceFor(db, blogger);

        var created = await service.CreateAsync(SampleRequest());
        await service.SubmitAsync(created.Id);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.ApproveAsync(created.Id));
    }

    [Fact]
    public async Task Admin_Should_Be_Able_To_Approve()
    {
        using var db = CreateContext();
        var blogger = FakeCurrentUser.As("u1", "Blogger One", Roles.Blogger);
        var bloggerService = ServiceFor(db, blogger);

        var created = await bloggerService.CreateAsync(SampleRequest());
        await bloggerService.SubmitAsync(created.Id);

        var admin = FakeCurrentUser.As("admin1", "Admin", Roles.Admin);
        var adminService = ServiceFor(db, admin);

        var approved = await adminService.ApproveAsync(created.Id);

        Assert.Equal("Published", approved.Status);
        Assert.NotNull(approved.PublishedAt);
    }

    [Fact]
    public async Task Reject_Should_Set_Status_And_Reason()
    {
        using var db = CreateContext();
        var blogger = FakeCurrentUser.As("u1", "Blogger One", Roles.Blogger);
        var bloggerService = ServiceFor(db, blogger);

        var created = await bloggerService.CreateAsync(SampleRequest());
        await bloggerService.SubmitAsync(created.Id);

        var admin = FakeCurrentUser.As("admin1", "Admin", Roles.Admin);
        var adminService = ServiceFor(db, admin);

        var rejected = await adminService.RejectAsync(created.Id, "Needs more detail");

        Assert.Equal("Rejected", rejected.Status);
        Assert.Equal("Needs more detail", rejected.RejectionReason);
    }

    [Fact]
    public async Task Viewer_Should_Not_Create_Post()
    {
        using var db = CreateContext();
        var viewer = FakeCurrentUser.As("v1", "Viewer", Roles.Viewer);
        var service = ServiceFor(db, viewer);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateAsync(SampleRequest()));
    }

    [Fact]
    public async Task Anonymous_Should_Not_Create_Post()
    {
        using var db = CreateContext();
        var service = ServiceFor(db, FakeCurrentUser.Anonymous());

        await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateAsync(SampleRequest()));
    }

    [Fact]
    public async Task GetPublished_Should_Only_Return_Published_Posts()
    {
        using var db = CreateContext();
        var blogger = FakeCurrentUser.As("u1", "Blogger One", Roles.Blogger);
        var bloggerService = ServiceFor(db, blogger);
        var admin = FakeCurrentUser.As("admin1", "Admin", Roles.Admin);
        var adminService = ServiceFor(db, admin);

        var draft = await bloggerService.CreateAsync(new CreatePostRequest("Draft one", "s", "c", null, null));

        var toPublish = await bloggerService.CreateAsync(new CreatePostRequest("Published one", "s", "c", null, null));
        await bloggerService.SubmitAsync(toPublish.Id);
        await adminService.ApproveAsync(toPublish.Id);

        var result = await bloggerService.GetPublishedAsync(new PostQuery(null, null));

        Assert.Single(result.Items);
        Assert.Equal("Published one", result.Items[0].Title);
    }
}
