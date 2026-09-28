using System.Text.Json;
using SimpleBlog.Application.Common;

namespace SimpleBlog.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title, errors) = ex switch
        {
            AppValidationException v => (StatusCodes.Status400BadRequest, v.Message, v.Errors),
            NotFoundException => (StatusCodes.Status404NotFound, ex.Message, null),
            ForbiddenException => (StatusCodes.Status403Forbidden, ex.Message, null),
            ConflictException => (StatusCodes.Status409Conflict, ex.Message, null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", (IDictionary<string, string[]>?)null)
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var payload = new
        {
            type = "about:blank",
            title,
            status,
            errors
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, options));
    }
}
