using Xunit;
using Moq;
using FluentAssertions;
using TodoApp.Core.Services;
using TodoApp.Core.Interfaces;
using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Core.Enums;
using AutoMapper;

namespace TodoApp.Tests.Services
{
    public class TodoServiceTests
    {
        private readonly Mock<ITodoRepository> _todoRepositoryMock;
        private readonly TodoService _todoService;
        private readonly Mock<IMapper> _mapperMock;

        public TodoServiceTests()
        {
            _todoRepositoryMock = new Mock<ITodoRepository>();
            _mapperMock = new Mock<IMapper>();
            _todoService = new TodoService(_todoRepositoryMock.Object, _mapperMock.Object);
        }
        [Fact]
        public async Task CreateTodoAsync_ShouldCreateTodoSuccessfully()
        {
            // Arrange
            var createDto = new CreateTodoDto
            {
                Title = "Test Todo"
            };
            var userId = 1;

            // Tạo mock TodoItem sẽ được trả về từ repository
            var createdTodo = new TodoItem
            {
                Id = 1,
                Title = createDto.Title,
                UserId = userId,
                Status = TodoStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            // Mock mapper cho CreateTodoDto -> TodoItem
            _mapperMock
                .Setup(x => x.Map<TodoItem>(createDto))
                .Returns(new TodoItem { Title = createDto.Title });

            _todoRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<TodoItem>()))
                .ReturnsAsync(createdTodo);  // Trả về TodoItem đã tạo

            // Mock mapper cho TodoItem -> TodoDto
            _mapperMock
                .Setup(x => x.Map<TodoDto>(createdTodo))
                .Returns(new TodoDto
                {
                    Id = 1,
                    Title = "Test Todo",
                    Status = "active",
                    UserId = userId
                });

            // Act
            var result = await _todoService.CreateTodoAsync(createDto, userId);

            // Assert
            result.Should().NotBeNull();  // Thêm check null
            result.Title.Should().Be("Test Todo");
            result.Status.Should().Be("active");
            result.UserId.Should().Be(userId);

            _todoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TodoItem>()), Times.Once);
        }
        [Fact]
        public async Task GetTodosByUserIdAsync_ShouldReturnUserTodos()
        {
            // Arrange
            var userId = 1;
            var todos = new List<TodoItem>
    {
        new TodoItem { Id = 1, Title = "Todo 1", UserId = userId, Status = TodoStatus.Active },
        new TodoItem { Id = 2, Title = "Todo 2", UserId = userId, Status = TodoStatus.Completed }
    };

            var todoDtos = new List<TodoDto>
    {
        new TodoDto { Id = 1, Title = "Todo 1", Status = "active", UserId = userId },
        new TodoDto { Id = 2, Title = "Todo 2", Status = "completed", UserId = userId }
    };

            _todoRepositoryMock
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(todos);

            // Setup mapper trả về đúng IEnumerable<TodoDto>
            _mapperMock
                .Setup(x => x.Map<IEnumerable<TodoDto>>(todos))
                .Returns(todoDtos);

            // Act
            var result = await _todoService.GetTodosByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(t => t.UserId.Should().Be(userId));
        }
        [Fact]
        public async Task ToggleTodoStatusAsync_FromActiveToCompleted_ShouldSetCompletedAt()
        {
            // Arrange
            var todoId = 1;
            var originalTodo = new TodoItem
            {
                Id = todoId,
                Title = "Test",
                Status = TodoStatus.Active,
                UserId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedTodo = new TodoItem
            {
                Id = todoId,
                Title = "Test",
                Status = TodoStatus.Completed,
                UserId = 1,
                CreatedAt = originalTodo.CreatedAt,
                CompletedAt = DateTime.UtcNow
            };

            var todoDto = new TodoDto
            {
                Id = todoId,
                Title = "Test",
                Status = "completed",
                UserId = 1,
                CompletedAt = updatedTodo.CompletedAt
            };

            _todoRepositoryMock
                .Setup(x => x.GetByIdAsync(todoId))
                .ReturnsAsync(originalTodo);

            _todoRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<TodoItem>()))
                .Returns(Task.CompletedTask)
                .Callback<TodoItem>(todo =>
                {
                    // Cập nhật originalTodo với status mới
                    originalTodo.Status = todo.Status;
                    originalTodo.CompletedAt = todo.CompletedAt;
                });

            // Mock mapper trả về TodoDto tương ứng
            _mapperMock
                .Setup(x => x.Map<TodoDto>(It.Is<TodoItem>(t => t.Status == TodoStatus.Completed)))
                .Returns(todoDto);

            // Act
            var result = await _todoService.ToggleTodoStatusAsync(todoId);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("completed");
            result.CompletedAt.Should().NotBeNull();

            _todoRepositoryMock.Verify(x => x.GetByIdAsync(todoId), Times.Once);
            _todoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TodoItem>()), Times.Once);
        }
    }
}