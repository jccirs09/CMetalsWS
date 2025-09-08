using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Foreign key for the sender
        public string? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }

        // Foreign key for the recipient (for direct messages)
        public string? RecipientId { get; set; }
        public ApplicationUser? Recipient { get; set; }

        // Foreign key for the chat group (for group messages)
        public int? ChatGroupId { get; set; }
        public ChatGroup? ChatGroup { get; set; }

        [NotMapped]
        public bool Sent { get; set; }

        [NotMapped]
        public bool Delivered { get; set; }

        [NotMapped]
        public bool Seen { get; set; }

        [NotMapped]
        public Dictionary<string, ApplicationUser> SeenBy { get; set; } = new();
    }
}
