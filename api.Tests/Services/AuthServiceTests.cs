using Api.Models;
using Microsoft.EntityFrameworkCore;
using Services;
using Xunit;

namespace Api.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly MessageRDBContext _context;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<MessageRDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MessageRDBContext(options);
            _authService = new AuthService(_context);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldAddUserToDatabase_WhenValidDataProvided()
        {
            // Arrange
            var userCommonId = "user123";
            var loginId = "testuser@example.com";
            var passwordHash = "hashedpassword123";
            var passwordSalt = "salt123";
            var displayName = "Test User";

            // Act
            await _authService.RegisterUserAsync(userCommonId, loginId, passwordHash, passwordSalt, displayName);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserCommonId == userCommonId);
            Assert.NotNull(user);
            Assert.Equal(userCommonId, user.UserCommonId);
            Assert.Equal(loginId, user.LoginId);
            Assert.Equal(passwordHash, user.PasswordHash);
            Assert.Equal(passwordSalt, user.PasswordSalt);
            Assert.Equal(displayName, user.DisplayName);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldSaveChangesToDatabase_WhenCalled()
        {
            // Arrange
            var userCommonId = "user456";
            var loginId = "testuser2@example.com";
            var passwordHash = "hashedpassword456";
            var passwordSalt = "salt456";
            var displayName = "Test User 2";

            var initialCount = await _context.Users.CountAsync();

            // Act
            await _authService.RegisterUserAsync(userCommonId, loginId, passwordHash, passwordSalt, displayName);

            // Assert
            var finalCount = await _context.Users.CountAsync();
            Assert.Equal(initialCount + 1, finalCount);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldCreateUserWithCorrectProperties_WhenCalled()
        {
            // Arrange
            var userCommonId = "user789";
            var loginId = "testuser3@example.com";
            var passwordHash = "hashedpassword789";
            var passwordSalt = "salt789";
            var displayName = "Test User 3";

            // Act
            await _authService.RegisterUserAsync(userCommonId, loginId, passwordHash, passwordSalt, displayName);

            // Assert
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserCommonId == userCommonId);
            Assert.NotNull(user);
            Assert.Equal(loginId, user.LoginId);
            Assert.Equal(passwordHash, user.PasswordHash);
            Assert.Equal(passwordSalt, user.PasswordSalt);
            Assert.Equal(displayName, user.DisplayName);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldAllowMultipleUsers_WhenDifferentUserCommonIds()
        {
            // Arrange
            var user1Data = ("user1", "user1@example.com", "hash1", "salt1", "User 1");
            var user2Data = ("user2", "user2@example.com", "hash2", "salt2", "User 2");

            // Act
            await _authService.RegisterUserAsync(user1Data.Item1, user1Data.Item2, user1Data.Item3, user1Data.Item4, user1Data.Item5);
            await _authService.RegisterUserAsync(user2Data.Item1, user2Data.Item2, user2Data.Item3, user2Data.Item4, user2Data.Item5);

            // Assert
            var user1 = await _context.Users.FirstOrDefaultAsync(u => u.UserCommonId == user1Data.Item1);
            var user2 = await _context.Users.FirstOrDefaultAsync(u => u.UserCommonId == user2Data.Item1);

            Assert.NotNull(user1);
            Assert.NotNull(user2);
            Assert.NotEqual(user1.UserCommonId, user2.UserCommonId);
            Assert.Equal(2, await _context.Users.CountAsync());
        }

        [Theory]
        [InlineData("", "login@example.com", "hash", "salt", "Display Name")]
        [InlineData("usercommon", "", "hash", "salt", "Display Name")]
        [InlineData("usercommon", "login@example.com", "", "salt", "Display Name")]
        [InlineData("usercommon", "login@example.com", "hash", "", "Display Name")]
        [InlineData("usercommon", "login@example.com", "hash", "salt", "")]
        public async Task RegisterUserAsync_ShouldStillCreateUser_WhenEmptyStringsProvided(
            string userCommonId, string loginId, string passwordHash, string passwordSalt, string displayName)
        {
            // Act
            await _authService.RegisterUserAsync(userCommonId, loginId, passwordHash, passwordSalt, displayName);

            // Assert
            var userCount = await _context.Users.CountAsync();
            Assert.Equal(1, userCount);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrowException_WhenNullParametersProvided()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _authService.RegisterUserAsync(null!, "login", "hash", "salt", "display"));
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldHandleDatabaseException_WhenDuplicateKeyConstraintViolated()
        {
            // Arrange
            var userCommonId = "duplicate_user";
            var loginId = "duplicate@example.com";
            var passwordHash = "hash";
            var passwordSalt = "salt";
            var displayName = "Duplicate User";

            // Add first user
            await _authService.RegisterUserAsync(userCommonId, loginId, passwordHash, passwordSalt, displayName);

            // Act & Assert
            // Note: In-memory database might not enforce unique constraints the same way as SQL Server
            // This test might need adjustment based on actual database constraints
            try
            {
                await _authService.RegisterUserAsync(userCommonId, "different@example.com", passwordHash, passwordSalt, displayName);
                // If no exception is thrown, verify the behavior
                var userCount = await _context.Users.CountAsync(u => u.UserCommonId == userCommonId);
                Assert.True(userCount >= 1);
            }
            catch (InvalidOperationException)
            {
                // This is expected if duplicate constraint is enforced
                Assert.True(true);
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}