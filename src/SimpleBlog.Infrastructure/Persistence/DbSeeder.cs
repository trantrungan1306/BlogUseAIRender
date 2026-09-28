using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.Core.Constants;
using SimpleBlog.Core.Entities;
using SimpleBlog.Core.Enums;
using SimpleBlog.Infrastructure.Identity;

namespace SimpleBlog.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Creates the database + schema from the model when no migrations are present.
        // Swap for db.Database.MigrateAsync() once you add EF Core migrations.
        await db.Database.EnsureCreatedAsync();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var admin = await EnsureUserAsync(userManager, "admin@simpleblog.local", "Admin123!$", "Site Admin", Roles.Admin);
        var author = await EnsureUserAsync(userManager, "blogger@simpleblog.local", "Blogger123!$", "Jane Writer", Roles.Blogger);

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Engineering", Slug = "engineering", Description = "Deep dives into building software." },
                new Category { Name = "Design", Slug = "design", Description = "Product design and UX." },
                new Category { Name = "Career", Slug = "career", Description = "Growth and the craft of working." });
            await db.SaveChangesAsync();
        }

        if (!await db.Posts.AnyAsync())
        {
            var engineering = await db.Categories.FirstAsync(c => c.Slug == "engineering");
            var design = await db.Categories.FirstAsync(c => c.Slug == "design");

            db.Posts.AddRange(
                new Post
                {
                    Title = "Designing a Clean Architecture API",
                    Slug = "designing-a-clean-architecture-api",
                    Summary = "How we structured SimpleBlog into Core, Application, Infrastructure and API layers.",
                    Content = "# Clean Architecture\n\nSeparating concerns keeps the domain pure and the edges replaceable. In this post we walk through the four layers and why the dependencies only ever point inward.\n\n- **Core** holds entities and rules.\n- **Application** holds use-cases.\n- **Infrastructure** talks to the database.\n- **API** exposes HTTP endpoints.\n\nThe result is a codebase that is easy to test and easy to grow.",
                    CoverImageUrl = "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?auto=format&fit=crop&w=1200&q=80",
                    Status = PostStatus.Published,
                    AuthorId = author.Id,
                    AuthorName = author.DisplayName,
                    CategoryId = engineering.Id,
                    PublishedAt = DateTime.UtcNow.AddDays(-3),
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Post
                {
                    Title = "Building a Responsive Design System",
                    Slug = "building-a-responsive-design-system",
                    Summary = "A practical guide to tokens, spacing and components that scale across screens.",
                    Content = "# Responsive by default\n\nA good design system starts with tokens: color, spacing, typography. From there, components compose predictably and every screen adapts from mobile to desktop.\n\nConsistency is a feature.",
                    CoverImageUrl = "https://images.unsplash.com/photo-1507842217343-583bb7270b66?auto=format&fit=crop&w=1200&q=80",
                    Status = PostStatus.Published,
                    AuthorId = author.Id,
                    AuthorName = author.DisplayName,
                    CategoryId = design.Id,
                    PublishedAt = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Post
                {
                    Title = "Draft: Notes on the Outbox Pattern",
                    Slug = "notes-on-the-outbox-pattern",
                    Summary = "Reliable messaging by writing events in the same transaction as your data.",
                    Content = "Work in progress. The outbox pattern avoids the dual-write problem between the database and the message broker.",
                    Status = PostStatus.PendingReview,
                    AuthorId = author.Id,
                    AuthorName = author.DisplayName,
                    CategoryId = engineering.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    UpdatedAt = DateTime.UtcNow.AddHours(-2)
                });

            await db.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string displayName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null) return user;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
