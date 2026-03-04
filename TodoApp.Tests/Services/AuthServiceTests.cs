using Xunit;
using Moq;
using FluentAssertions;
using TodoApp.Core.Services;
using TodoApp.Core.Interfaces;
using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace TodoApp.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthService _authService;
        private readonly Mock<IEmailService> _emailServiceMock;
        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock.Setup(x => x["Jwt:Secret"])
                .Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
            _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("TodoApp");
            _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("TodoAppUsers");
            _configurationMock.Setup(x => x["Jwt:AccessTokenExpirationMinutes"]).Returns("15");
            _configurationMock.Setup(x => x["Jwt:RefreshTokenExpirationDays"]).Returns("7");

            _authService = new AuthService(_userRepositoryMock.Object, _configurationMock.Object,_emailServiceMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUserSuccessfully()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test@123"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerDto.Username))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => user);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.User.Username.Should().Be("testuser");
            result.User.Email.Should().Be("test@example.com");
            result.User.Role.Should().Be("User");
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "existing@example.com",
                Password = "Test@123"
            };

            var existingUser = new User { Id = 1, Email = "existing@example.com" };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var act = async () => await _authService.RegisterAsync(registerDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Email already exists");

            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ShouldThrowException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "existinguser",
                Email = "new@example.com",
                Password = "Test@123"
            };

            var existingUser = new User { Id = 1, Username = "existinguser" };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(registerDto.Username))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var act = async () => await _authService.RegisterAsync(registerDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Username already exists");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Test@123"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test@123");
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = hashedPassword,
                Role = "User",
                IsActive = true
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.User.Email.Should().Be("test@example.com");
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        //[Fact]
        //public async Task LoginAsync_WithInvalidEmail_ShouldThrowException()
        //{
        //    // Arrange
        //    var loginDto = new LoginDto
        //    {
        //        Email = "notfound@example.com",
        //        Password = "Test@123"
        //    };

        //    _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email))
        //        .ReturnsAsync((User?)null);

        //    // Act & Assert
        //    var act = async () => await _authService.LoginAsync(loginDto);
        //    await act.Should().ThrowAsync<InvalidOperationException>()
        //        .WithMessage("Invalid email or password");
        //}

        //[Fact]
        //public async Task LoginAsync_WithInvalidPassword_ShouldThrowException()
        //{
        //    // Arrange
        //    var loginDto = new LoginDto
        //    {
        //        Email = "test@example.com",
        //        Password = "WrongPassword"
        //    };

        //    var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        //    var user = new User
        //    {
        //        Id = 1,
        //        Email = "test@example.com",
        //        PasswordHash = hashedPassword,
        //        IsActive = true
        //    };

        //    _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email))
        //        .ReturnsAsync(user);

        //    // Act & Assert
        //    var act = async () => await _authService.LoginAsync(loginDto);
        //    await act.Should().ThrowAsync<InvalidOperationException>()
        //        .WithMessage("Invalid email or password");
        //}

        [Fact]
        public async Task LoginAsync_WithLockedAccount_ShouldThrowException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Test@123"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test@123");
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = hashedPassword,
                IsActive = false
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var act = async () => await _authService.LoginAsync(loginDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Your account has been locked. Please contact admin.");
        }
    }
}