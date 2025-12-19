using System.Net.Http.Headers;

namespace EpsilonWebApp.Services.AuthorizationService
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IAuthService _authService;

        public AuthorizationMessageHandler(IAuthService authService)
        {
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
