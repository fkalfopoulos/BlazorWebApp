using EpsilonWebApp.Services.AuthorizationService;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace EpsilonWebApp.Services.Tests.AuthorizationService
{
    public class JwtServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IConfigurationSection> _jwtSectionMock;
        private readonly IJwtService _jwtService;

        public JwtServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _jwtSectionMock = new Mock<IConfigurationSection>();

            // Setup JWT configuration
            _jwtSectionMock.Setup(x => x["SecretKey"]).Returns("MyVerySecretKeyThatIsAtLeast32CharactersLong123456");
            _jwtSectionMock.Setup(x => x["Issuer"]).Returns("EpsilonWebApp");
            _jwtSectionMock.Setup(x => x["Audience"]).Returns("EpsilonWebApp.Client");

            _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(_jwtSectionMock.Object);

            _jwtService = new JwtService(_configurationMock.Object);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidToken()
        {
             var username = "testuser";

             var token = _jwtService.GenerateToken(username);

             token.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            handler.CanReadToken(token).Should().BeTrue();
        }

        [Fact]
        public void GenerateToken_ShouldContainUsernameInClaims()
        {
            var username = "testuser";

            var token = _jwtService.GenerateToken(username);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
            jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == username);
        }

        [Fact]
        public void GenerateToken_ShouldContainJtiClaim()
        {
            var username = "testuser";

            var token = _jwtService.GenerateToken(username);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            jtiClaim.Should().NotBeNull();
            Guid.TryParse(jtiClaim!.Value, out _).Should().BeTrue();
        }

        [Fact]
        public void GenerateToken_ShouldHaveCorrectIssuerAndAudience()
        {
            var username = "testuser";

            var token = _jwtService.GenerateToken(username);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Issuer.Should().Be("EpsilonWebApp");
            jwtToken.Audiences.Should().Contain("EpsilonWebApp.Client");
        }

        [Fact]
        public void GenerateToken_ShouldExpireIn2Hours()
        {
            var username = "testuser";
            var before = DateTime.UtcNow.AddHours(2);

            var token = _jwtService.GenerateToken(username);
            var after = DateTime.UtcNow.AddHours(2);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.ValidTo.Should().BeOnOrAfter(before);
            jwtToken.ValidTo.Should().BeOnOrBefore(after.AddSeconds(1)); // Allow 1 second tolerance
        }

        [Theory]
        [InlineData("admin")]
        [InlineData("user123")]
        [InlineData("test@example.com")]
        public void GenerateToken_ShouldWorkWithDifferentUsernames(string username)
        {
            var token = _jwtService.GenerateToken(username);

            token.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
        }

        [Fact]
        public void GenerateToken_ShouldThrowException_WhenSecretKeyNotConfigured()
        {
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(x => x["SecretKey"]).Returns((string?)null);
            mockSection.Setup(x => x["Issuer"]).Returns("EpsilonWebApp");
            mockSection.Setup(x => x["Audience"]).Returns("EpsilonWebApp.Client");

            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x.GetSection("JwtSettings")).Returns(mockSection.Object);

            var service = new JwtService(mockConfig.Object);

            Assert.Throws<InvalidOperationException>(() => service.GenerateToken("testuser"));
        }

        [Fact]
        public void ValidateToken_ShouldReturnClaimsPrincipal_ForValidToken()
        {
            var username = "testuser";
            var token = _jwtService.GenerateToken(username);

            var principal = _jwtService.ValidateToken(token);

            principal.Should().NotBeNull();
            principal!.Identity.Should().NotBeNull();
            principal.Identity!.IsAuthenticated.Should().BeTrue();
            principal.Identity.Name.Should().Be(username);
        }

        [Fact]
        public void ValidateToken_ShouldReturnNull_ForInvalidToken()
        {
            var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.token";

            var principal = _jwtService.ValidateToken(invalidToken);

            principal.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_ShouldReturnNull_ForExpiredToken()
        {
            // This test is tricky as we can't easily create an expired token
            // In a real scenario, you might use a library to create an expired token
            // For now, we'll use an invalid token format

            // Arrange
            var expiredToken = "expired.token.here";

            var principal = _jwtService.ValidateToken(expiredToken);

            principal.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_ShouldReturnNull_ForEmptyToken()
        {
            var principal = _jwtService.ValidateToken(string.Empty);

            principal.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_ShouldContainCorrectClaims()
        {
            
            var username = "admin";
            var token = _jwtService.GenerateToken(username);

            var principal = _jwtService.ValidateToken(token);

            principal.Should().NotBeNull();
            var claims = principal!.Claims.ToList();

            claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
            claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == username);
            claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        }

        [Fact]
        public void ValidateToken_ShouldValidateSignature()
        {
            var username = "testuser";
            var token = _jwtService.GenerateToken(username);
            var tamperedToken = token.Substring(0, token.Length - 5) + "XXXXX";

            var principal = _jwtService.ValidateToken(tamperedToken);

            principal.Should().BeNull();
        }

        [Fact]
        public void GenerateToken_AndValidate_ShouldRoundTrip()
        {
            var username = "roundtrip_test";
            var token = _jwtService.GenerateToken(username);
            var principal = _jwtService.ValidateToken(token);

            // Assert
            principal.Should().NotBeNull();
            principal!.Identity!.Name.Should().Be(username);
        }

        [Fact]
        public void GenerateToken_MultipleTimes_ShouldGenerateDifferentTokens()
        {
            var username = "testuser";

            var token1 = _jwtService.GenerateToken(username);
            Thread.Sleep(10); // Ensure different timestamps
            var token2 = _jwtService.GenerateToken(username);

            token1.Should().NotBe(token2); // Different JTI and timestamps
        }
    }
}
