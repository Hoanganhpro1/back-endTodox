using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Core.DTOs;
using TodoApp.Core.Interfaces;

namespace TodoApp.Core.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITodoRepository _todoRepository;

        public AdminService(IUserRepository userRepository, ITodoRepository todoRepository)
        {
            _userRepository = userRepository;
            _todoRepository = todoRepository;
        }

        public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var todos = await _todoRepository.GetAllAsync();

            return users.Select(user => new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                TodoCount = todos.Count(t => t.UserId == user.Id)
            }).OrderBy(u => u.CreatedAt);
        }

        public async Task<AdminUserDto> ToggleUserStatusAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {userId} not found");
            }

            // Toggle status
            user.IsActive = !user.IsActive;
            await _userRepository.UpdateAsync(user);

            // Get todo count
            var todos = await _todoRepository.GetByUserIdAsync(userId);

            return new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                TodoCount = todos.Count()
            };
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {userId} not found");
            }

            await _userRepository.DeleteAsync(user);
            return true;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var todos = await _todoRepository.GetAllAsync();

            var now = DateTime.UtcNow;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            var startOfDay = now.Date;

            return new DashboardStatsDto
            {
                TotalUsers = users.Count(),
                ActiveUsers = users.Count(u => u.IsActive),
                LockedUsers = users.Count(u => !u.IsActive),
                TotalTodos = todos.Count(),
                CompletedTodos = todos.Count(t => t.Status.ToString().ToLower() == "completed"),
                ActiveTodos = todos.Count(t => t.Status.ToString().ToLower() == "active"),
                NewUsersThisWeek = users.Count(u => u.CreatedAt >= startOfWeek),
                TodosCreatedToday = todos.Count(t => t.CreatedAt >= startOfDay)
            };
        }

        public async Task<UserDetailDto> GetUserDetailAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {userId} not found");
            }

            var todos = await _todoRepository.GetByUserIdAsync(userId);

            return new UserDetailDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Todos = todos.Select(t => new TodoDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status.ToString().ToLower(),
                    CompletedAt = t.CompletedAt,
                    CreatedAt = t.CreatedAt,
                    UserId = t.UserId
                }).ToList()
            };
        }
    }
}