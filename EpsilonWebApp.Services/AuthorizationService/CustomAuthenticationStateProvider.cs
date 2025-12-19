 using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;


namespace EpsilonWebApp.Services.AuthorizationService
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService _authService;
        private AuthenticationState _anonymousState = new(new ClaimsPrincipal(new ClaimsIdentity()));
        private AuthenticationState? _currentState;

        public CustomAuthenticationStateProvider(IAuthService authService)
        {
            _authService = authService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // If we already have a cached state, return it
            if (_currentState != null)
                return _currentState;

            // Check if there's a stored token (for session persistence)
            var token = await _authService.GetTokenAsync();
            var username = await _authService.GetUsernameAsync();

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
            {
                // User is authenticated - restore session
                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                _currentState = new AuthenticationState(user);
                return _currentState;
            }

            // User is not authenticated
            return _anonymousState;
        }

        public void MarkUserAsAuthenticated(string username)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _currentState = new AuthenticationState(user);
            NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
        }

        public void MarkUserAsLoggedOut()
        {
            _currentState = null;
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymousState));
        }
    }
}
