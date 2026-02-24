using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApp.Core.DTOs;

namespace TodoApp.Core.Services
{
    public interface ITodoService
    {
        Task<IEnumerable<TodoDto>> GetAllTodosAsync();
        Task<IEnumerable<TodoDto>> GetTodosByUserIdAsync(int userId);
        Task<TodoDto> GetTodoByIdAsync(int id);
        Task<IEnumerable<TodoDto>> GetCompletedTodosAsync();
        Task<IEnumerable<TodoDto>> GetPendingTodosAsync();
        Task<TodoDto> CreateTodoAsync(CreateTodoDto createTodoDto, int userId);
        Task<TodoDto> UpdateTodoAsync(int id, UpdateTodoDto updateTodoDto);
        Task DeleteTodoAsync(int id);
        Task<TodoDto> ToggleTodoStatusAsync(int id);
    }
}