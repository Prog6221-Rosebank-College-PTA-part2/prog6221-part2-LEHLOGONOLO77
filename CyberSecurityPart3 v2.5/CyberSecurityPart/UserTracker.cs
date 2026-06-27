using System;

namespace CyberSecurityAwarenessBot.Models
{
    public class UserTask
    {
        // Unique ID (matches the MySQL primary key)
        public int Id { get; set; }

        // Task title
        public string Title { get; set; } = string.Empty;

        // Detailed description
        public string Description { get; set; } = string.Empty;

        // Optional reminder date
        public DateTime? ReminderDate { get; set; }

        // Indicates whether the task has been completed
        public bool IsCompleted { get; set; }

        // Date the task was created
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
