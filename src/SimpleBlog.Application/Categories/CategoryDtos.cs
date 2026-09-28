namespace SimpleBlog.Application.Categories;

public record CategoryDto(int Id, string Name, string Slug, string? Description, int PostCount);

public record CreateCategoryRequest(string Name, string? Description);
