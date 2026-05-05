using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace EqDemo.Client;

/// <summary>
/// Resolves the current user's authentication state by calling a small endpoint
/// on the server (which authenticates the request via the ASP.NET Core Identity
/// auth cookie). Replaces the OIDC-based remote authentication state provider
/// that came with Microsoft.AspNetCore.ApiAuthorization.IdentityServer.
/// </summary>
public sealed class HostAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string AuthenticationType = "Cookies";

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly HttpClient _http;

    public HostAuthenticationStateProvider(HttpClient http)
    {
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var response = await _http.GetAsync("_auth/me");
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return Anonymous;
            }

            response.EnsureSuccessStatusCode();

            var info = await response.Content.ReadFromJsonAsync<AuthInfo>();
            if (info is null || !info.IsAuthenticated)
            {
                return Anonymous;
            }

            var claims = (info.Claims ?? Array.Empty<AuthClaim>())
                .Select(c => new Claim(c.Type, c.Value));
            var identity = new ClaimsIdentity(claims, AuthenticationType, ClaimTypes.Name, ClaimTypes.Role);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (HttpRequestException)
        {
            return Anonymous;
        }
    }

    private sealed record AuthInfo(bool IsAuthenticated, AuthClaim[]? Claims);

    private sealed record AuthClaim(string Type, string Value);
}
