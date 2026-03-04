using Xunit;
using Moq;
using FluentAssertions;
using TodoApp.Core.Services;
using TodoApp.Core.Interfaces;
using TodoApp.Core.Entities;
using TodoApp.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Core.Enums;

namespace TodoApp.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITodoRepository> _todoRepositoryMock;
        private readonly AdminService _adminService;

        public AdminServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _todoRepositoryMock = new Mock<ITodoRepository>();
            _adminService = new AdminService(_userRepositoryMock.Object, _todoRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithPagination_ShouldReturnPagedResult()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1", Email = "user1@test.com", Role = "User", IsActive = true },
                new User { Id = 2, Username = "user2", Email = "user2@test.com", Role = "User", IsActive = true },
                new User { Id = 3, Username = "admin", Email = "admin@test.com", Role = "Admin", IsActive = true }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);
            _todoRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<TodoItem>());

            var paginationParams = new PaginationParams
            {
                Page = 1,
                PageSize = 2
            };

            // Act
            var result = await _adminService.GetAllUsersAsync(paginationParams);

            // Assert
            result.Items.Should().HaveCount(2);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(2);
            result.TotalCount.Should().Be(3);
            result.TotalPages.Should().Be(2);
            result.HasNext.Should().BeTrue();
            result.HasPrevious.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllUsersAsync_WithSearchTerm_ShouldFilterUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "john", Email = "john@test.com" },
                new User { Id = 2, Username = "jane", Email = "jane@test.com" },
                new User { Id = 3, Username = "admin", Email = "admin@test.com" }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);
            _todoRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<TodoItem>());

            var paginationParams = new PaginationParams
            {
                Page = 1,
                PageSize = 10,
                SearchTerm = "john"
            };

            // Act
            var result = await _adminService.GetAllUsersAsync(paginationParams);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Username.Should().Be("john");
        }

        [Fact]
        public async Task ToggleUserStatusAsync_ShouldToggleIsActive()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@test.com",
                IsActive = true
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _todoRepositoryMock.Setup(x => x.GetByUserIdAsync(1))
                .ReturnsAsync(new List<TodoItem>());

            // Act
            var result = await _adminService.ToggleUserStatusAsync(1);

            // Assert
            result.IsActive.Should().BeFalse();
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(
                u => u.IsActive == false
            )), Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatusAsync_WhenUserNotFound_ShouldThrowException()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var act = async () => await _adminService.ToggleUserStatusAsync(999);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser()
        {
            // Arrange
            var user = new User { Id = 1, Username = "testuser" };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _adminService.DeleteUserAsync(1);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(x => x.DeleteAsync(It.Is<User>(
                u => u.Id == 1
            )), Times.Once);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_ShouldReturnCorrectStats()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new User { Id = 2, IsActive = false, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new User { Id = 3, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-2) }
            };

            var todos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Status = TodoStatus.Active, CreatedAt = DateTime.UtcNow },
                new TodoItem { Id = 2, Status = TodoStatus.Completed, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new TodoItem { Id = 3, Status = TodoStatus.Completed, CreatedAt = DateTime.UtcNow }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(users);
            _todoRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(todos);

            // Act
            var result = await _adminService.GetDashboardStatsAsync();

            // Assert
            result.TotalUsers.Should().Be(3);
            result.ActiveUsers.Should().Be(2);
            result.LockedUsers.Should().Be(1);
            result.TotalTodos.Should().Be(3);
            result.CompletedTodos.Should().Be(2);
            result.ActiveTodos.Should().Be(1);
            result.TodosCreatedToday.Should().Be(2);
        }

        [Fact]
        public async Task GetUserDetailAsync_ShouldReturnUserWithTodos()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@test.com",
                Role = "User",
                IsActive = true
            };

            var todos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Todo 1", UserId = 1, Status = TodoStatus.Active },
                new TodoItem { Id = 2, Title = "Todo 2", UserId = 1, Status = TodoStatus.Completed }
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);
            _todoRepositoryMock.Setup(x => x.GetByUserIdAsync(1))
                .ReturnsAsync(todos);

            // Act
            var result = await _adminService.GetUserDetailAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("testuser");
            result.Todos.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllTodosAsync_WithPagination_ShouldReturnPagedTodos()
        {
            // Arrange
            var todos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Todo 1", UserId = 1, Status = TodoStatus.Active },
                new TodoItem { Id = 2, Title = "Todo 2", UserId = 2, Status = TodoStatus.Completed },
                new TodoItem { Id = 3, Title = "Todo 3", UserId = 1, Status = TodoStatus.Active }
            };

            _todoRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(todos);

            var paginationParams = new PaginationParams
            {
                Page = 1,
                PageSize = 2
            };

            // Act
            var result = await _adminService.GetAllTodosAsync(paginationParams);

            // Assert
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(3);
            result.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetAllTodosAsync_WithSearchTerm_ShouldFilterTodos()
        {
            // Arrange
            var todos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Buy groceries", UserId = 1 },
                new TodoItem { Id = 2, Title = "Learn React", UserId = 2 },
                new TodoItem { Id = 3, Title = "Buy books", UserId = 1 }
            };

            _todoRepositoryMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(todos);

            var paginationParams = new PaginationParams
            {
                Page = 1,
                PageSize = 10,
                SearchTerm = "buy"
            };

            // Act
            var result = await _adminService.GetAllTodosAsync(paginationParams);

            // Assert
            result.Items.Should().HaveCount(2);
            result.Items.Should().AllSatisfy(t =>
                t.Title.ToLower().Should().Contain("buy")
            );
        }
    }
}