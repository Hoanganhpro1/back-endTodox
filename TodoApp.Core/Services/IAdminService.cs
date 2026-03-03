using System.Threading.Tasks;
using TodoApp.Core.DTOs;
using TodoApp.Core.Common;

namespace TodoApp.Core.Services
{
    public interface IAdminService
    {
        Task<PagedResult<AdminUserDto>> GetAllUsersAsync(PaginationParams paginationParams);
        Task<PagedResult<TodoDto>> GetAllTodosAsync(PaginationParams paginationParams);
        Task<AdminUserDto> ToggleUserStatusAsync(int userId);
        Task<bool> DeleteUserAsync(int userId);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<UserDetailDto> GetUserDetailAsync(int userId);
    }
}