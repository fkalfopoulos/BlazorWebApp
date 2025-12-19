using Blazored.SessionStorage;

namespace EpsilonWebApp.Services.AuthorizationService
{
    public interface IAuthService
    {
        Task<string?> GetTokenAsync();
        Task SetTokenAsync(string token);
        Task RemoveTokenAsync();
        Task<string?> GetUsernameAsync();
        Task SetUsernameAsync(string username);
    }

    public class AuthService : IAuthService
    {
        private readonly ISessionStorageService _sessionStorage;
        private const string TokenKey = "authToken";
        private const string UsernameKey = "username";

        public AuthService(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _sessionStorage.GetItemAsync<string>(TokenKey);
        }

        public async Task SetTokenAsync(string token)
        {
            await _sessionStorage.SetItemAsync(TokenKey, token);
        }

        public async Task RemoveTokenAsync()
        {
            await _sessionStorage.RemoveItemAsync(TokenKey);
            await _sessionStorage.RemoveItemAsync(UsernameKey);
        }

        public async Task<string?> GetUsernameAsync()
        {
            return await _sessionStorage.GetItemAsync<string>(UsernameKey);
        }

        public async Task SetUsernameAsync(string username)
        {
            await _sessionStorage.SetItemAsync(UsernameKey, username);
        }
    }
}
