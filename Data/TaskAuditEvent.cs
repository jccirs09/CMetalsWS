
using System;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class TaskAuditEvent
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }

        [Required]
        public TaskType TaskType { get; set; }

        [Required]
        public AuditEventType EventType { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public string? Notes { get; set; }
    }

}



