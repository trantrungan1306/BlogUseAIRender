using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SimpleBlog.Blazor.Models;

namespace SimpleBlog.Blazor.Services;

public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
}

public class BlogApi
{
    private readonly HttpClient _http;

    public BlogApi(HttpClient http)
    {
        _http = http;
    }

    // ---- Auth ----
    public Task<AuthResponse> LoginAsync(LoginRequest req) => PostAsync<AuthResponse>("auth/login", req);
    public Task<AuthResponse> RegisterAsync(RegisterRequest req) => PostAsync<AuthResponse>("auth/register", req);

    // ---- Posts ----
    public async Task<PagedResult<PostListItem>> GetPublishedAsync(string? search, string? category, int page = 1, int pageSize = 9)
    {
        var query = $"posts?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(category)) query += $"&category={Uri.EscapeDataString(category)}";
        return await _http.GetFromJsonAsync<PagedResult<PostListItem>>(query)
            ?? new PagedResult<PostListItem>(new(), page, pageSize, 0, 0, false, false);
    }

    public Task<PostDetail?> GetBySlugAsync(string slug) => _http.GetFromJsonAsync<PostDetail>($"posts/slug/{slug}");
    public Task<PostDetail?> GetByIdAsync(int id) => _http.GetFromJsonAsync<PostDetail>($"posts/{id}");

    public async Task<List<PostListItem>> GetMineAsync() =>
        await _http.GetFromJsonAsync<List<PostListItem>>("posts/mine") ?? new();

    public async Task<List<PostListItem>> GetPendingAsync() =>
        await _http.GetFromJsonAsync<List<PostListItem>>("posts/pending") ?? new();

    public Task<PostDetail> CreateAsync(CreatePostRequest req) => PostAsync<PostDetail>("posts", req);
    public Task<PostDetail> UpdateAsync(int id, CreatePostRequest req) => PutAsync<PostDetail>($"posts/{id}", req);
    public Task<PostDetail> SubmitAsync(int id) => PostAsync<PostDetail>($"posts/{id}/submit", null);
    public Task<PostDetail> ApproveAsync(int id) => PostAsync<PostDetail>($"posts/{id}/approve", null);
    public Task<PostDetail> RejectAsync(int id, string reason) => PostAsync<PostDetail>($"posts/{id}/reject", new RejectRequest(reason));

    public async Task DeleteAsync(int id)
    {
        var res = await _http.DeleteAsync($"posts/{id}");
        await EnsureSuccess(res);
    }

    // ---- Categories ----
    public async Task<List<CategoryDto>> GetCategoriesAsync() =>
        await _http.GetFromJsonAsync<List<CategoryDto>>("categories") ?? new();

    // ---- helpers ----
    private async Task<T> PostAsync<T>(string url, object? body)
    {
        var res = await _http.PostAsJsonAsync(url, body ?? new { });
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<T>())!;
    }

    private async Task<T> PutAsync<T>(string url, object body)
    {
        var res = await _http.PutAsJsonAsync(url, body);
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task EnsureSuccess(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode) return;

        var message = res.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Please sign in to continue.",
            HttpStatusCode.Forbidden => "You don't have permission to do that.",
            _ => "Request failed."
        };

        try
        {
            var text = await res.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    message = title.GetString() ?? message;
            }
        }
        catch { /* keep default message */ }

        throw new ApiException(message);
    }
}
