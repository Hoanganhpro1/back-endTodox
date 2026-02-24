using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Core.DTOs;
using TodoApp.Core.Services;

namespace TodoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ✅ Yêu cầu đăng nhập cho tất cả endpoints
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodosController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        // ✅ User xem todos của mình, Admin xem tất cả
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoDto>>> GetTodos([FromQuery] string timeFilter = "all")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim);

            IEnumerable<TodoDto> todos;

            // Admin xem tất cả, User chỉ xem của mình
            if (userRole == "Admin")
            {
                todos = await _todoService.GetAllTodosAsync();
            }
            else
            {
                todos = await _todoService.GetTodosByUserIdAsync(userId);
            }

            // Filter theo thời gian
            var filteredTodos = FilterByTime(todos, timeFilter);

            return Ok(filteredTodos);
        }

        // ✅ User chỉ xem todo của mình
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoDto>> GetTodo(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = int.Parse(userIdClaim!);

                var todo = await _todoService.GetTodoByIdAsync(id);

                // Admin có thể xem tất cả, User chỉ xem của mình
                if (userRole != "Admin" && todo.UserId != userId)
                {
                    return Forbid();
                }

                return Ok(todo);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("completed")]
        public async Task<ActionResult<IEnumerable<TodoDto>>> GetCompletedTodos()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(userIdClaim!);

            IEnumerable<TodoDto> todos;

            if (userRole == "Admin")
            {
                todos = await _todoService.GetCompletedTodosAsync();
            }
            else
            {
                todos = (await _todoService.GetTodosByUserIdAsync(userId))
                    .Where(t => t.Status.ToLower() == "completed");
            }

            return Ok(todos);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<TodoDto>>> GetPendingTodos()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(userIdClaim!);

            IEnumerable<TodoDto> todos;

            if (userRole == "Admin")
            {
                todos = await _todoService.GetPendingTodosAsync();
            }
            else
            {
                todos = (await _todoService.GetTodosByUserIdAsync(userId))
                    .Where(t => t.Status.ToLower() == "active");
            }

            return Ok(todos);
        }

        // ✅ Tạo todo với UserId của user đang login
        [HttpPost]
        public async Task<ActionResult<TodoDto>> CreateTodo(CreateTodoDto createTodoDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdClaim!);

            var todo = await _todoService.CreateTodoAsync(createTodoDto, userId);
            return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
        }

        // ✅ User chỉ sửa todo của mình, Admin sửa được tất cả
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, UpdateTodoDto updateTodoDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = int.Parse(userIdClaim!);

                var todo = await _todoService.GetTodoByIdAsync(id);

                if (userRole != "Admin" && todo.UserId != userId)
                {
                    return Forbid();
                }

                await _todoService.UpdateTodoAsync(id, updateTodoDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ✅ User chỉ xóa todo của mình, Admin xóa được tất cả
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = int.Parse(userIdClaim!);

                var todo = await _todoService.GetTodoByIdAsync(id);

                if (userRole != "Admin" && todo.UserId != userId)
                {
                    return Forbid();
                }

                await _todoService.DeleteTodoAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ✅ User chỉ toggle todo của mình
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult<TodoDto>> ToggleTodoStatus(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = int.Parse(userIdClaim!);

                var todo = await _todoService.GetTodoByIdAsync(id);

                if (userRole != "Admin" && todo.UserId != userId)
                {
                    return Forbid();
                }

                var updatedTodo = await _todoService.ToggleTodoStatusAsync(id);
                return Ok(updatedTodo);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        private IEnumerable<TodoDto> FilterByTime(IEnumerable<TodoDto> todos, string timeFilter)
        {
            var now = DateTime.UtcNow;

            return timeFilter.ToLower() switch
            {
                "today" => todos.Where(t => t.CreatedAt.Date == now.Date),
                "week" => todos.Where(t =>
                {
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                    var endOfWeek = startOfWeek.AddDays(7);
                    return t.CreatedAt >= startOfWeek && t.CreatedAt < endOfWeek;
                }),
                "month" => todos.Where(t =>
                    t.CreatedAt.Year == now.Year && t.CreatedAt.Month == now.Month
                ),
                _ => todos
            };
        }
    }
}