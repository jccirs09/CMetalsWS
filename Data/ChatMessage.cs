using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CMetalsWS.Data.Chat;

namespace CMetalsWS.Data
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsPinned { get; set; }

        // Foreign key for the sender
        public string? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }

        // Foreign key for the recipient (for direct messages)
        public string? RecipientId { get; set; }
        public ApplicationUser? Recipient { get; set; }

        // Foreign key for the chat group (for group messages)
        public int? ChatGroupId { get; set; }
        public ChatGroup? ChatGroup { get; set; }

        public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
        public ICollection<MessageSeen> SeenBy { get; set; } = new List<MessageSeen>();
    }
}
