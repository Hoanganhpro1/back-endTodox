using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using TodoApp.Core.Enums;
using TodoApp.Core.Interfaces;

namespace TodoApp.Core.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;
        private readonly IMapper _mapper;

        public TodoService(ITodoRepository todoRepository, IMapper mapper)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TodoDto>> GetAllTodosAsync()
        {
            var todos = await _todoRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TodoDto>>(todos.OrderBy(t => t.CreatedAt));
        }

        // ✅ Thêm method mới
        public async Task<IEnumerable<TodoDto>> GetTodosByUserIdAsync(int userId)
        {
            var todos = await _todoRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<TodoDto>>(todos.OrderBy(t => t.CreatedAt));
        }

        public async Task<TodoDto> GetTodoByIdAsync(int id)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            if (todo == null)
            {
                throw new KeyNotFoundException($"Todo with id {id} not found");
            }
            return _mapper.Map<TodoDto>(todo);
        }

        public async Task<IEnumerable<TodoDto>> GetCompletedTodosAsync()
        {
            var todos = await _todoRepository.GetCompletedAsync();
            return _mapper.Map<IEnumerable<TodoDto>>(todos.OrderBy(t => t.CreatedAt));
        }

        public async Task<IEnumerable<TodoDto>> GetPendingTodosAsync()
        {
            var todos = await _todoRepository.GetPendingAsync();
            return _mapper.Map<IEnumerable<TodoDto>>(todos.OrderBy(t => t.CreatedAt));
        }

        // ✅ Thêm userId parameter
        public async Task<TodoDto> CreateTodoAsync(CreateTodoDto createTodoDto, int userId)
        {
            var todo = new TodoItem
            {
                Title = createTodoDto.Title,
                Status = TodoStatus.Active,
                CompletedAt = null,
                CreatedAt = DateTime.UtcNow,
                UserId = userId // ✅ Gán UserId
            };

            var createdTodo = await _todoRepository.AddAsync(todo);
            return _mapper.Map<TodoDto>(createdTodo);
        }

        public async Task<TodoDto> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            if (todo == null)
            {
                throw new KeyNotFoundException($"Todo with id {id} not found");
            }

            todo.Title = updateTodoDto.Title;

            if (!string.IsNullOrEmpty(updateTodoDto.Status))
            {
                todo.Status = updateTodoDto.Status.ToLower() == "completed"
                    ? TodoStatus.Completed
                    : TodoStatus.Active;
            }

            todo.CompletedAt = updateTodoDto.CompletedAt;

            await _todoRepository.UpdateAsync(todo);
            return _mapper.Map<TodoDto>(todo);
        }

        public async Task DeleteTodoAsync(int id)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            if (todo == null)
            {
                throw new KeyNotFoundException($"Todo with id {id} not found");
            }

            await _todoRepository.DeleteAsync(todo);
        }

        public async Task<TodoDto> ToggleTodoStatusAsync(int id)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            if (todo == null)
            {
                throw new KeyNotFoundException($"Todo with id {id} not found");
            }

            if (todo.Status == TodoStatus.Active)
            {
                todo.Status = TodoStatus.Completed;
                todo.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                todo.Status = TodoStatus.Active;
                todo.CompletedAt = null;
            }

            await _todoRepository.UpdateAsync(todo);
            return _mapper.Map<TodoDto>(todo);
        }
    }
}