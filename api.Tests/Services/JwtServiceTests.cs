using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Api.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;
        private readonly string _secret = "this-is-a-very-long-secret-key-for-testing-purposes-123456789";
        private readonly string _issuer = "test-issuer";
        private readonly string _audience = "test-audience";

        public JwtServiceTests()
        {
            _jwtService = new JwtService(_secret, _issuer, _audience);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidJwtToken_WhenValidClaimsProvided()
        {
            // Arrange
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim("user_id", "123")
            };

            // Act
            var token = _jwtService.GenerateToken(claims);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.Contains(".", token); // JWT tokens contain dots
        }

        [Fact]
        public void GenerateToken_ShouldReturnTokenWithCorrectClaims_WhenValidClaimsProvided()
        {
            // Arrange
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim("user_id", "123")
            };

            // Act
            var token = _jwtService.GenerateToken(claims);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            Assert.Equal("testuser", jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value);
            Assert.Equal("test@example.com", jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value);
            Assert.Equal("123", jsonToken.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value);
        }

        [Fact]
        public void GenerateToken_ShouldReturnTokenWithCorrectIssuer_WhenCalled()
        {
            // Arrange
            var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };

            // Act
            var token = _jwtService.GenerateToken(claims);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            Assert.Equal(_issuer, jsonToken.Issuer);
        }

        [Fact]
        public void GenerateToken_ShouldReturnTokenWithCorrectAudience_WhenCalled()
        {
            // Arrange
            var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };

            // Act
            var token = _jwtService.GenerateToken(claims);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            Assert.Contains(_audience, jsonToken.Audiences);
        }

        [Fact]
        public void GenerateToken_ShouldReturnTokenWithExpirationTime_WhenCalled()
        {
            // Arrange
            var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };

            // Act
            var token = _jwtService.GenerateToken(claims);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            Assert.True(jsonToken.ValidTo > DateTime.UtcNow);
            Assert.True(jsonToken.ValidTo <= DateTime.UtcNow.AddHours(1)); // Default expiration
        }

        [Fact]
        public void GenerateToken_ShouldReturnDifferentTokens_WhenCalledMultipleTimes()
        {
            // Arrange
            var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };

            // Act
            var token1 = _jwtService.GenerateToken(claims);
            Thread.Sleep(1000); // Ensure different timestamp
            var token2 = _jwtService.GenerateToken(claims);

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void ValidateToken_ShouldReturnTrue_WhenValidTokenProvided()
        {
            // Arrange
            var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
            var token = _jwtService.GenerateToken(claims);

            // Act
            var isValid = _jwtService.ValidateToken(token);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateToken_ShouldReturnFalse_WhenInvalidTokenProvided()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var isValid = _jwtService.ValidateToken(invalidToken);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateToken_ShouldReturnFalse_WhenNullTokenProvided()
        {
            // Act
            var isValid = _jwtService.ValidateToken(null!);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateToken_ShouldReturnFalse_WhenEmptyTokenProvided()
        {
            // Act
            var isValid = _jwtService.ValidateToken(string.Empty);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateToken_ShouldReturnFalse_WhenTokenSignedWithDifferentSecret()
        {
            // Arrange
            var differentSecretService = new JwtService("different-secret-key-123456789", _issuer, _audience);
            var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
            var token = differentSecretService.GenerateToken(claims);

            // Act
            var isValid = _jwtService.ValidateToken(token);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void GetClaimsFromToken_ShouldReturnCorrectClaims_WhenValidTokenProvided()
        {
            // Arrange
            var originalClaims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim("user_id", "123")
            };
            var token = _jwtService.GenerateToken(originalClaims);

            // Act
            var retrievedClaims = _jwtService.GetClaimsFromToken(token);

            // Assert
            Assert.NotNull(retrievedClaims);
            Assert.Contains(retrievedClaims, c => c.Type == ClaimTypes.Name && c.Value == "testuser");
            Assert.Contains(retrievedClaims, c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
            Assert.Contains(retrievedClaims, c => c.Type == "user_id" && c.Value == "123");
        }

        [Fact]
        public void GetClaimsFromToken_ShouldReturnNull_WhenInvalidTokenProvided()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var claims = _jwtService.GetClaimsFromToken(invalidToken);

            // Assert
            Assert.Null(claims);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("short")]
        public void Constructor_ShouldThrowException_WhenInvalidSecretProvided(string secret)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new JwtService(secret, _issuer, _audience));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Constructor_ShouldThrowException_WhenInvalidIssuerProvided(string issuer)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new JwtService(_secret, issuer, _audience));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Constructor_ShouldThrowException_WhenInvalidAudienceProvided(string audience)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new JwtService(_secret, _issuer, audience));
        }
    }
}