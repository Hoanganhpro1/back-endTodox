using System;
using System.Collections.Generic;

namespace TodoApp.Core.DTOs
{
    // DTO cho thông tin user trong Admin panel
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TodoCount { get; set; } // Số lượng todos của user
    }

    // DTO cho thống kê Dashboard
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }
        public int TotalTodos { get; set; }
        public int CompletedTodos { get; set; }
        public int ActiveTodos { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int TodosCreatedToday { get; set; }
    }

    // DTO cho chi tiết user (optional - dùng sau)
    public class UserDetailDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TodoDto> Todos { get; set; } = new List<TodoDto>();
    }
}