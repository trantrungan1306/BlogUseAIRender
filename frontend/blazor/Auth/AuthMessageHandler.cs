using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace SimpleBlog.Blazor.Auth;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsStringAsync("simpleblog_token", cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
