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

            // Get username from server (via /api/auth/me)
            // The HttpOnly cookie is sent automatically with the request
            var username = await _authService.GetUsernameAsync();
            
            if (!string.IsNullOrEmpty(username))
            {
                // User is authenticated - cookie is managed by browser
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
            
            // Clear cached username
            if (_authService is AuthService authService)
            {
                authService.ClearCache();
            }
            
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymousState));
        }
    }
}
