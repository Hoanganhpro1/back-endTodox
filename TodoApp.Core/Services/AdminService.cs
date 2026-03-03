using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Core.DTOs;
using TodoApp.Core.Interfaces;
using TodoApp.Core.Common;

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

        public async Task<PagedResult<AdminUserDto>> GetAllUsersAsync(PaginationParams paginationParams)
        {
            var users = await _userRepository.GetAllAsync();
            var todos = await _todoRepository.GetAllAsync();

            var usersQuery = users.Select(user => new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                TodoCount = todos.Count(t => t.UserId == user.Id)
            }).AsQueryable();

            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var searchTerm = paginationParams.SearchTerm.ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm)
                );
            }

            usersQuery = paginationParams.SortBy?.ToLower() switch
            {
                "username" => paginationParams.SortDescending
                    ? usersQuery.OrderByDescending(u => u.Username)
                    : usersQuery.OrderBy(u => u.Username),
                "email" => paginationParams.SortDescending
                    ? usersQuery.OrderByDescending(u => u.Email)
                    : usersQuery.OrderBy(u => u.Email),
                "createdat" => paginationParams.SortDescending
                    ? usersQuery.OrderByDescending(u => u.CreatedAt)
                    : usersQuery.OrderBy(u => u.CreatedAt),
                _ => usersQuery.OrderBy(u => u.CreatedAt)
            };

            var totalCount = usersQuery.Count();

            var items = usersQuery
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToList();

            return new PagedResult<AdminUserDto>
            {
                Items = items,
                Page = paginationParams.Page,
                PageSize = paginationParams.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
            };
        }

        public async Task<PagedResult<TodoDto>> GetAllTodosAsync(PaginationParams paginationParams)
        {
            var todos = await _todoRepository.GetAllAsync();

            var todosQuery = todos.Select(todo => new TodoDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Status = todo.Status.ToString().ToLower(),
                CompletedAt = todo.CompletedAt,
                CreatedAt = todo.CreatedAt,
                UserId = todo.UserId
            }).AsQueryable();

            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var searchTerm = paginationParams.SearchTerm.ToLower();
                todosQuery = todosQuery.Where(t => t.Title.ToLower().Contains(searchTerm));
            }

            todosQuery = paginationParams.SortBy?.ToLower() switch
            {
                "title" => paginationParams.SortDescending
                    ? todosQuery.OrderByDescending(t => t.Title)
                    : todosQuery.OrderBy(t => t.Title),
                "status" => paginationParams.SortDescending
                    ? todosQuery.OrderByDescending(t => t.Status)
                    : todosQuery.OrderBy(t => t.Status),
                "createdat" => paginationParams.SortDescending
                    ? todosQuery.OrderByDescending(t => t.CreatedAt)
                    : todosQuery.OrderBy(t => t.CreatedAt),
                _ => todosQuery.OrderByDescending(t => t.CreatedAt)
            };

            var totalCount = todosQuery.Count();

            var items = todosQuery
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToList();

            return new PagedResult<TodoDto>
            {
                Items = items,
                Page = paginationParams.Page,
                PageSize = paginationParams.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
            };
        }

        public async Task<AdminUserDto> ToggleUserStatusAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with id {userId} not found");
            }

            user.IsActive = !user.IsActive;
            await _userRepository.UpdateAsync(user);

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