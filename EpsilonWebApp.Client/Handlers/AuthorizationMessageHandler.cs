using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace EpsilonWebApp.Client.Handlers
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            // Enable credentials to allow HttpOnly cookies to be sent automatically
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
