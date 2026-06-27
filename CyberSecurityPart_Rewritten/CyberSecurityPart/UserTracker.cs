using System;

namespace CyberSecurityAwarenessBot.Models
{
    public class UserTask
    {
       
        public int Id { get; set; }

  
        public string Title { get; set; } = string.Empty;

       
        public string Description { get; set; } = string.Empty;

       
        public DateTime? ReminderDate { get; set; }

        public bool IsCompleted { get; set; }

        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
