using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleBlog.Application.Common;
using SimpleBlog.Core.Entities;
using SimpleBlog.Infrastructure.Identity;

namespace SimpleBlog.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Post>(e =>
        {
            e.Property(p => p.Title).HasMaxLength(200).IsRequired();
            e.Property(p => p.Slug).HasMaxLength(220).IsRequired();
            e.Property(p => p.Summary).HasMaxLength(500).IsRequired();
            e.Property(p => p.AuthorName).HasMaxLength(120);
            e.HasIndex(p => p.Slug).IsUnique();
            e.HasIndex(p => p.Status);
            e.HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.Property(c => c.Slug).HasMaxLength(120).IsRequired();
            e.HasIndex(c => c.Slug).IsUnique();
        });

        builder.Entity<Comment>(e =>
        {
            e.Property(c => c.AuthorName).HasMaxLength(120);
            e.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Review>(e =>
        {
            e.Property(r => r.ReviewerName).HasMaxLength(120);
            e.HasOne(r => r.Post)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Notification>(e =>
        {
            e.Property(n => n.Title).HasMaxLength(160).IsRequired();
            e.HasIndex(n => n.UserId);
        });

        builder.Entity<OutboxMessage>(e =>
        {
            e.Property(o => o.Type).HasMaxLength(160).IsRequired();
            e.HasIndex(o => o.ProcessedAt);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);
}
