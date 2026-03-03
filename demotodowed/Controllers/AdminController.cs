using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Core.Services;
using TodoApp.Core.Common;
using Serilog;

namespace TodoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false
        )
        {
            try
            {
                Log.Information("Admin fetching users. Page: {Page}, PageSize: {PageSize}, Search: {SearchTerm}", 
                    page, pageSize, searchTerm);

                var paginationParams = new PaginationParams
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = await _adminService.GetAllUsersAsync(paginationParams);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching users");
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("todos")]
        public async Task<IActionResult> GetAllTodos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false
        )
        {
            try
            {
                Log.Information("Admin fetching todos. Page: {Page}, PageSize: {PageSize}, Search: {SearchTerm}", 
                    page, pageSize, searchTerm);

                var paginationParams = new PaginationParams
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = await _adminService.GetAllTodosAsync(paginationParams);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching todos");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserDetail(int id)
        {
            try
            {
                Log.Information("Admin fetching user detail. UserId: {UserId}", id);
                
                var userDetail = await _adminService.GetUserDetailAsync(id);
                return Ok(userDetail);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching user detail");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPatch("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (currentUserId == id.ToString())
                {
                    return BadRequest(new { message = "You cannot lock/unlock yourself" });
                }

                Log.Warning("Admin toggling user status. UserId: {UserId}, AdminId: {AdminId}", 
                    id, currentUserId);

                var user = await _adminService.ToggleUserStatusAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error toggling user status");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (currentUserId == id.ToString())
                {
                    return BadRequest(new { message = "You cannot delete yourself" });
                }

                Log.Warning("Admin deleting user. UserId: {UserId}, AdminId: {AdminId}", 
                    id, currentUserId);

                await _adminService.DeleteUserAsync(id);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting user");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _adminService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching dashboard stats");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}