using Microsoft.Extensions.DependencyInjection;
using SimpleBlog.Application.Categories;
using SimpleBlog.Application.Notifications;
using SimpleBlog.Application.Posts;

namespace SimpleBlog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}
