using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Neurabrain.AdminClient.Security
{
    public class CookieHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Αυτό λέει στον browser: "Στείλε το HttpOnly Cookie μαζί με το request"
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            return base.SendAsync(request, cancellationToken);
        }
    }
}