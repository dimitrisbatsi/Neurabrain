using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Neurabrain.AdminClient.Security
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
                // Ρωτάμε το API ποιος είναι συνδεδεμένος
                var userInfo = await _http.GetFromJsonAsync<UserInfo>("api/auth/me");

                if (userInfo != null && !string.IsNullOrEmpty(userInfo.Email))
                {
                    // Αν απαντήσει το API, φτιάχνουμε την ταυτότητα στο UI
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
            catch
            {
                // Αν ρίξει 401 Unauthorized το API, πιάνουμε το exception και συνεχίζουμε
            }

            // Αν φτάσουμε εδώ, ο χρήστης ΔΕΝ είναι συνδεδεμένος
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Αυτή τη μέθοδο θα την καλούμε εμείς όταν κάνει Login ή Logout για να κάνει refresh η οθόνη
        public void NotifyAuthState()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        // Βοηθητική κλάση για το JSON που επιστρέφει το /api/auth/me
        private class UserInfo
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}