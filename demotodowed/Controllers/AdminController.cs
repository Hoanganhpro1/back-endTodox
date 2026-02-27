using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Core.Services;

namespace TodoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được truy cập
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/Admin/users/5
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserDetail(int id)
        {
            try
            {
                var userDetail = await _adminService.GetUserDetailAsync(id);
                return Ok(userDetail);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PATCH: api/Admin/users/5/toggle-status
        [HttpPatch("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                // Lấy userId của admin đang login
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Admin không thể khóa chính mình
                if (currentUserId == id.ToString())
                {
                    return BadRequest(new { message = "You cannot lock/unlock yourself" });
                }

                var user = await _adminService.ToggleUserStatusAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/Admin/users/5
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                // Lấy userId của admin đang login
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Admin không thể xóa chính mình
                if (currentUserId == id.ToString())
                {
                    return BadRequest(new { message = "You cannot delete yourself" });
                }

                await _adminService.DeleteUserAsync(id);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/Admin/stats
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
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}