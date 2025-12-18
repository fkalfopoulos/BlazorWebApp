using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace EpsilonWebApp.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState _anonymousState = new(new ClaimsPrincipal(new ClaimsIdentity()));
        private AuthenticationState? _currentState;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(_currentState ?? _anonymousState);
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
