using System;

namespace CyberSecurityAwarenessBot.Forms
{
   
    
   
    public enum TaskStatus
    {
        Pending,
        Completed
    }

    
    /// Represents a single cybersecurity task/reminder. This is the in-memory

    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        //The date the user should be reminded about this task
        public DateTime? ReminderAt { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

      
        public bool IsReminderDue =>
            Status == TaskStatus.Pending &&
            ReminderAt.HasValue &&
            ReminderAt.Value.Date <= DateTime.Today;

        public string ReminderDisplay
        {
            get
            {
                if (!ReminderAt.HasValue) return "—";
                if (Status == TaskStatus.Completed) return ReminderAt.Value.ToString("dd MMM yy");
                if (ReminderAt.Value.Date < DateTime.Today) return $"⏰ Overdue ({ReminderAt.Value:dd MMM})";
                if (ReminderAt.Value.Date == DateTime.Today) return "⏰ Today!";
                return $"📅 {ReminderAt.Value:dd MMM yy}";
            }
        }

       
        public string StatusDisplay => Status == TaskStatus.Completed ? "✔ Done" : "Pending";
    }
}
