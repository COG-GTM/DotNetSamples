using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace EqDemo.Client;

public class ServerAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;

    public ServerAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userInfo = await _httpClient.GetFromJsonAsync<UserInfo>("api/user");
            if (userInfo != null && userInfo.IsAuthenticated)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userInfo.UserName ?? string.Empty)
                };

                if (userInfo.Roles != null)
                {
                    foreach (var role in userInfo.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }

                var identity = new ClaimsIdentity(claims, "serverauth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
        }
        catch (Exception)
        {
            // Not authenticated or server unavailable
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public class UserInfo
    {
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public List<string>? Roles { get; set; }
    }
}
