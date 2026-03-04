using Xunit;
using FluentAssertions;
using TodoApp.Infrastructure.Data;
using TodoApp.Infrastructure.Repositories;
using TodoApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace TodoApp.Tests.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Role = "User",
                IsActive = true
            };

            // Act
            var result = await _repository.AddAsync(user);
            await _context.SaveChangesAsync();

            // Assert
            result.Id.Should().BeGreaterThan(0);
            var savedUser = await _context.Users.FindAsync(result.Id);
            savedUser.Should().NotBeNull();
            savedUser!.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task GetByEmailAsync_WhenEmailExists_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User"
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByEmailAsync("test@example.com");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetByEmailAsync_WhenEmailNotExists_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByEmailAsync("notfound@example.com");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUsernameAsync_WhenUsernameExists_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User"
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUsernameAsync("testuser");

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User",
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            user.IsActive = false;
            await _repository.UpdateAsync(user);
            await _context.SaveChangesAsync();

            // Assert
            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveUser()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User"
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var userId = user.Id;

            // Act
            await _repository.DeleteAsync(user);
            await _context.SaveChangesAsync();

            // Assert
            var deletedUser = await _context.Users.FindAsync(userId);
            deletedUser.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}