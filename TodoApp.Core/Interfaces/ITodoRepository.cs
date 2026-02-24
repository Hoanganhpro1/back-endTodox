using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApp.Core.Entities;

namespace TodoApp.Core.Interfaces
{
    public interface ITodoRepository : IRepository<TodoItem>
    {
        Task<IEnumerable<TodoItem>> GetCompletedAsync();
        Task<IEnumerable<TodoItem>> GetPendingAsync();
        Task<IEnumerable<TodoItem>> GetByUserIdAsync(int userId); // ✅ Thêm dòng này
        Task<TodoItem?> GetByIdAndUserIdAsync(int id, int userId); // ✅ Thêm dòng này
    }
}