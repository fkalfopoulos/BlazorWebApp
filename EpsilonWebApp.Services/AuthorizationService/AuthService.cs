using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EpsilonWebApp.Services.AuthorizationService
{
    public interface IAuthService
    {
        Task<string?> GetUsernameAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private string? _cachedUsername;
        private readonly ILogger<AuthService> logger;

        public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            this.logger = logger;
        }

        /// <summary>
        /// Gets username from server by calling /api/auth/me
        /// The JWT cookie is sent automatically by the browser
        /// Server reads the JWT and returns the username from claims
        /// </summary>
        public async Task<string?> GetUsernameAsync()
        {
            // Return cached username if available
            if (!string.IsNullOrEmpty(_cachedUsername))
                return _cachedUsername;

            try
            {
                // Call server endpoint - cookie sent automatically
                var response = await _httpClient.GetAsync("api/auth/me");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
                    if (result != null)
                    {
                        _cachedUsername = result.Username;
                        return _cachedUsername;
                    }
                }
            }
            catch
            {
                logger.LogWarning("Failed to get username from /api/auth/me");
            }

            return null;
        }

        public void ClearCache()
        {
            _cachedUsername = null;
        }

        private class UserInfoResponse
        {
            public string Username { get; set; } = string.Empty;
        }
    }
}
