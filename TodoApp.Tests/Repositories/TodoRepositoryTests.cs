using Xunit;
using FluentAssertions;
using TodoApp.Infrastructure.Data;
using TodoApp.Infrastructure.Repositories;
using TodoApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Core.Enums;

namespace TodoApp.Tests.Repositories
{
    public class TodoRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TodoRepository _repository;

        public TodoRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new TodoRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTodoToDatabase()
        {
            // Arrange
            var todo = new TodoItem
            {
                Title = "Test Todo",
                UserId = 1,
                Status = TodoStatus.Active
            };

            // Act
            var result = await _repository.AddAsync(todo);
            await _context.SaveChangesAsync();

            // Assert
            result.Id.Should().BeGreaterThan(0);
            var savedTodo = await _context.TodoItems.FindAsync(result.Id);
            savedTodo.Should().NotBeNull();
            savedTodo!.Title.Should().Be("Test Todo");
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnOnlyUserTodos()
        {
            // Arrange
            await _context.TodoItems.AddRangeAsync(
                new TodoItem { Title = "User 1 Todo 1", UserId = 1, Status = TodoStatus.Active },
                new TodoItem { Title = "User 1 Todo 2", UserId = 1, Status = TodoStatus.Completed },
                new TodoItem { Title = "User 2 Todo", UserId = 2, Status = TodoStatus.Active }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(t => t.UserId.Should().Be(1));
        }

        [Fact]
        public async Task GetCompletedAsync_ShouldReturnOnlyCompletedTodos()
        {
            // Arrange
            await _context.TodoItems.AddRangeAsync(
                new TodoItem { Title = "Active", UserId = 1, Status = TodoStatus.Active },
                new TodoItem { Title = "Completed 1", UserId = 1, Status = TodoStatus.Completed },
                new TodoItem { Title = "Completed 2", UserId = 2, Status = TodoStatus.Completed }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCompletedAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(t => t.Status.Should().Be(TodoStatus.Completed));
        }

        [Fact]
        public async Task GetPendingAsync_ShouldReturnOnlyActiveTodos()
        {
            // Arrange
            await _context.TodoItems.AddRangeAsync(
                new TodoItem { Title = "Active 1", UserId = 1, Status = TodoStatus.Active },
                new TodoItem { Title = "Active 2", UserId = 1, Status = TodoStatus.Active },
                new TodoItem { Title = "Completed", UserId = 2, Status = TodoStatus.Completed }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPendingAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(t => t.Status.Should().Be(TodoStatus.Active));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTodo()
        {
            // Arrange
            var todo = new TodoItem
            {
                Title = "Original Title",
                UserId = 1,
                Status = TodoStatus.Active
            };
            await _context.TodoItems.AddAsync(todo);
            await _context.SaveChangesAsync();

            // Act
            todo.Title = "Updated Title";
            todo.Status = TodoStatus.Completed;
            await _repository.UpdateAsync(todo);
            await _context.SaveChangesAsync();

            // Assert
            var updatedTodo = await _context.TodoItems.FindAsync(todo.Id);
            updatedTodo!.Title.Should().Be("Updated Title");
            updatedTodo.Status.Should().Be(TodoStatus.Completed);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTodo()
        {
            // Arrange
            var todo = new TodoItem
            {
                Title = "Test Todo",
                UserId = 1,
                Status = TodoStatus.Active
            };
            await _context.TodoItems.AddAsync(todo);
            await _context.SaveChangesAsync();
            var todoId = todo.Id;

            // Act
            await _repository.DeleteAsync(todo);
            await _context.SaveChangesAsync();

            // Assert
            var deletedTodo = await _context.TodoItems.FindAsync(todoId);
            deletedTodo.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}