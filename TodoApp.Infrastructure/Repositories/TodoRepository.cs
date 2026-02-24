using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoApp.Core.Entities;
using TodoApp.Core.Enums;
using TodoApp.Core.Interfaces;
using TodoApp.Infrastructure.Data;

namespace TodoApp.Infrastructure.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly ApplicationDbContext _context;

        public TodoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TodoItem?> GetByIdAsync(int id)
        {
            return await _context.TodoItems.FindAsync(id);
        }

        // ✅ Thêm method mới
        public async Task<TodoItem?> GetByIdAndUserIdAsync(int id, int userId)
        {
            return await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<IEnumerable<TodoItem>> GetAllAsync()
        {
            return await _context.TodoItems.ToListAsync();
        }

        // ✅ Thêm method mới
        public async Task<IEnumerable<TodoItem>> GetByUserIdAsync(int userId)
        {
            return await _context.TodoItems
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TodoItem>> GetCompletedAsync()
        {
            return await _context.TodoItems
                .Where(t => t.Status == TodoStatus.Completed)
                .ToListAsync();
        }

        public async Task<IEnumerable<TodoItem>> GetPendingAsync()
        {
            return await _context.TodoItems
                .Where(t => t.Status == TodoStatus.Active)
                .ToListAsync();
        }

        public async Task<TodoItem> AddAsync(TodoItem entity)
        {
            await _context.TodoItems.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(TodoItem entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TodoItem entity)
        {
            _context.TodoItems.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}