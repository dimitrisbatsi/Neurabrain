using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Neurabrain.AppClient.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;

        public CustomAuthStateProvider(HttpClient http)
        {
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userInfo = await _http.GetFromJsonAsync<UserInfo>("api/auth/me");

                if (userInfo != null && !string.IsNullOrEmpty(userInfo.Email))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
                        new Claim(ClaimTypes.Name, userInfo.Name),
                        new Claim(ClaimTypes.Email, userInfo.Email),
                        new Claim(ClaimTypes.Role, userInfo.Role)
                    };

                    var identity = new ClaimsIdentity(claims, "ServerAuth");
                    var principal = new ClaimsPrincipal(identity);
                    return new AuthenticationState(principal);
                }
            }
            catch { }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void NotifyAuthState()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private class UserInfo
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}