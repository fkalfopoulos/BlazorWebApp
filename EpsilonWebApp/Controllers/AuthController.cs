using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EpsilonWebApp.Contracts.DTOs;
using EpsilonWebApp.Services.AuthorizationService;

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
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // Simple authentication - In production, use proper password hashing and user database
            if (request.Username == "admin" && request.Password == "admin123")
            {
                var token = _jwtService.GenerateToken(request.Username);

                // Create cookie for browser-based authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, request.Username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return Ok(new LoginResponse
                {
                    Token = token,
                    Username = request.Username
                });
            }

            return Unauthorized(new { message = "Invalid username or password" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("validate")]
        public IActionResult ValidateToken([FromQuery] string token)
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
                return Unauthorized();

            return Ok(new { valid = true, username = principal.Identity?.Name });
        }
    }
}
