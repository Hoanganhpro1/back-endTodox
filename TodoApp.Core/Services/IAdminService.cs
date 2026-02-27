using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApp.Core.DTOs;

namespace TodoApp.Core.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
        Task<AdminUserDto> ToggleUserStatusAsync(int userId);
        Task<bool> DeleteUserAsync(int userId);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<UserDetailDto> GetUserDetailAsync(int userId);
    }
}