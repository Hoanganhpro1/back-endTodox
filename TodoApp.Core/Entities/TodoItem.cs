using System;
using TodoApp.Core.Enums;

namespace TodoApp.Core.Entities
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TodoStatus Status { get; set; } = TodoStatus.Active;
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to User
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}