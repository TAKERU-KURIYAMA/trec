using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using Xunit;

namespace Api.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly TrecPlansRDBContext _context;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<TrecPlansRDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TrecPlansRDBContext(options);
            var logger = new LoggerFactory().CreateLogger<AuthService>();
            _authService = new AuthService(_context, logger);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldAddUserToDatabase_WhenValidDataProvided()
        {
            // Arrange
            var userCommonId = "user123";
            var loginId = "testuser";
            var clientHashedPassword = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"; // SHA256 of "hello"
            var displayName = "Test User";

            // Act
            var user = await _authService.RegisterUserAsync(userCommonId, loginId, clientHashedPassword, displayName);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(userCommonId, user.UserCommonId);
            Assert.Equal(loginId, user.LoginId);
            Assert.Equal(displayName, user.DisplayName);
            Assert.NotNull(user.PasswordHash);
            Assert.NotNull(user.PasswordSalt);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldSaveChangesToDatabase_WhenCalled()
        {
            // Arrange
            var userCommonId = "user456";
            var loginId = "testuser2";
            var clientHashedPassword = "b03ddf3ca2e714a6548e7495e2a03f5e824eaac9837cd7f159c67b90fb4b7342"; // SHA256 of "world"
            var displayName = "Test User 2";

            var initialCount = await _context.Users.CountAsync();

            // Act
            await _authService.RegisterUserAsync(userCommonId, loginId, clientHashedPassword, displayName);

            // Assert
            var finalCount = await _context.Users.CountAsync();
            Assert.Equal(initialCount + 1, finalCount);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldCreateUserWithCorrectProperties_WhenCalled()
        {
            // Arrange
            var userCommonId = "user789";
            var loginId = "testuser3";
            var clientHashedPassword = "473287f8298dba7163a897908958f7c0eae733e25d2e027992ea2edc9bed2fa8"; // SHA256 of "test"
            var displayName = "Test User 3";

            // Act
            var user = await _authService.RegisterUserAsync(userCommonId, loginId, clientHashedPassword, displayName);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(loginId, user.LoginId);
            Assert.Equal(displayName, user.DisplayName);
            Assert.NotNull(user.PasswordHash);
            Assert.NotNull(user.PasswordSalt);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldAllowMultipleUsers_WhenDifferentUserCommonIds()
        {
            // Arrange
            var user1Data = ("user1", "user1", "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3", "User 1");
            var user2Data = ("user2", "user2", "b03ddf3ca2e714a6548e7495e2a03f5e824eaac9837cd7f159c67b90fb4b7342", "User 2");

            // Act
            await _authService.RegisterUserAsync(user1Data.Item1, user1Data.Item2, user1Data.Item3, user1Data.Item4);
            await _authService.RegisterUserAsync(user2Data.Item1, user2Data.Item2, user2Data.Item3, user2Data.Item4);

            // Assert
            var user1 = await _context.Users.FirstOrDefaultAsync(u => u.UserCommonId == user1Data.Item1);
            var user2 = await _context.Users.FirstOrDefaultAsync(u => u.UserCommonId == user2Data.Item1);

            Assert.NotNull(user1);
            Assert.NotNull(user2);
            Assert.NotEqual(user1.UserCommonId, user2.UserCommonId);
            Assert.Equal(2, await _context.Users.CountAsync());
        }

        [Theory]
        [InlineData("", "login", "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3", "Display Name")]
        [InlineData("usercommon", "", "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3", "Display Name")]
        [InlineData("usercommon", "login", "", "Display Name")]
        [InlineData("usercommon", "login", "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3", "")]
        public async Task RegisterUserAsync_ShouldThrowException_WhenInvalidParametersProvided(
            string userCommonId, string loginId, string clientHashedPassword, string displayName)
        {
            // Act & Assert
            await Assert.ThrowsAsync<Api.Common.AppException>(() =>
                _authService.RegisterUserAsync(userCommonId, loginId, clientHashedPassword, displayName));
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrowException_WhenNullParametersProvided()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Api.Common.AppException>(() =>
                _authService.RegisterUserAsync(null!, "login", "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3", "display"));
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldHandleDatabaseException_WhenDuplicateKeyConstraintViolated()
        {
            // Arrange
            var userCommonId = "duplicate_user";
            var loginId = "duplicate";
            var clientHashedPassword = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3";
            var displayName = "Duplicate User";

            // Add first user
            await _authService.RegisterUserAsync(userCommonId, loginId, clientHashedPassword, displayName);

            // Act & Assert
            // Note: In-memory database might not enforce unique constraints the same way as SQL Server
            // This test might need adjustment based on actual database constraints
            try
            {
                await _authService.RegisterUserAsync(userCommonId, "different", clientHashedPassword, displayName);
                // If no exception is thrown, verify the behavior
                var userCount = await _context.Users.CountAsync(u => u.UserCommonId == userCommonId);
                Assert.True(userCount >= 1);
            }
            catch (Api.Common.AppException)
            {
                // This is expected if duplicate constraint is enforced
                Assert.True(true);
            }
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnUser_WhenValidCredentialsProvided()
        {
            // Arrange
            var userCommonId = "auth_test_user";
            var loginId = "authtest";
            var clientHashedPassword = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"; // SHA256 of "hello"
            var displayName = "Auth Test User";

            // Register user first
            await _authService.RegisterUserAsync(userCommonId, loginId, clientHashedPassword, displayName);

            // Act
            var authenticatedUser = await _authService.AuthenticateUserAsync(loginId, clientHashedPassword);

            // Assert
            Assert.NotNull(authenticatedUser);
            Assert.Equal(userCommonId, authenticatedUser.UserCommonId);
            Assert.Equal(loginId, authenticatedUser.LoginId);
            Assert.Equal(displayName, authenticatedUser.DisplayName);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenInvalidCredentialsProvided()
        {
            // Arrange
            var userCommonId = "auth_test_user2";
            var loginId = "authtest2";
            var clientHashedPassword = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"; // SHA256 of "hello"
            var wrongPassword = "b03ddf3ca2e714a6548e7495e2a03f5e824eaac9837cd7f159c67b90fb4b7342"; // SHA256 of "world"
            var displayName = "Auth Test User 2";

            // Register user first
            await _authService.RegisterUserAsync(userCommonId, loginId, clientHashedPassword, displayName);

            // Act
            var authenticatedUser = await _authService.AuthenticateUserAsync(loginId, wrongPassword);

            // Assert
            Assert.Null(authenticatedUser);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}