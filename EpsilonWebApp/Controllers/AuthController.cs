using Microsoft.AspNetCore.Mvc;
using EpsilonWebApp.Contracts.DTOs;
using EpsilonWebApp.Services.AuthorizationService;
using Microsoft.AspNetCore.Authorization;

namespace EpsilonWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;

        public AuthController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Simple validation (in production, validate against database)
            if (request.Username == "admin" && request.Password == "admin123")
            {
                // Generate JWT token
                var token = _jwtService.GenerateToken(request.Username);

                // Store JWT in HttpOnly cookie (XSS protection)
                Response.Cookies.Append("authToken", token, new CookieOptions
                {
                    HttpOnly = true,        // Cannot be accessed by JavaScript
                    Secure = true,          // Only sent over HTTPS (set to false in development if not using HTTPS)
                    SameSite = SameSiteMode.Strict,  // CSRF protection
                    Expires = DateTimeOffset.UtcNow.AddHours(2),
                    Path = "/"
                });

                // Return user info (NOT the token)
                return Ok(new LoginResponse
                {
                    Username = request.Username,
                });
            }

            return Unauthorized(new { Message = "Invalid credentials" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Remove the authentication cookie
            Response.Cookies.Delete("authToken");
            
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpGet("check")]
        public IActionResult CheckAuth()
        {
            // Check if user has valid auth cookie
            var token = Request.Cookies["authToken"];
            
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            // Optionally validate the token here
            return Ok(new { IsAuthenticated = true });
        }

        /// <summary>
        /// Gets current user's claims
        /// Allows client to read username without storing it separately
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var username = User.Identity?.Name;
            
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            return Ok(new { Username = username });
        }
    }
}
