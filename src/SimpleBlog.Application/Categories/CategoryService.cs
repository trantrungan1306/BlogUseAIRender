using Microsoft.EntityFrameworkCore;
using SimpleBlog.Application.Common;
using SimpleBlog.Core.Constants;
using SimpleBlog.Core.Entities;
using SimpleBlog.Core.Enums;

namespace SimpleBlog.Application.Categories;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CategoryService(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.Posts.Count(p => p.Status == PostStatus.Published)))
            .ToListAsync(ct);

        return categories;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (!_currentUser.IsInRole(Roles.Admin))
            throw new ForbiddenException("Admin role required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new AppValidationException("Category name is required.");

        var slug = SlugGenerator.Generate(request.Name);
        if (await _db.Categories.AnyAsync(c => c.Slug == slug, ct))
            throw new ConflictException("A category with a similar name already exists.");

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim()
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return category.ToDto(0);
    }
}
