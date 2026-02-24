using System;

namespace TodoApp.Core.DTOs
{
    public class TodoDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; } // ✅ Thêm dòng này
    }

    public class CreateTodoDto
    {
        public string Title { get; set; } = string.Empty;
    }

    public class UpdateTodoDto
    {
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
    }
}