using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    public class ChatGroup
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        // Foreign key to associate a group with a branch (nullable for non-branch-specific groups)
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }

        // Navigation property for the members of the group
        public ICollection<ChatGroupUser> ChatGroupUsers { get; set; } = new List<ChatGroupUser>();

        // Navigation property for the messages in the group
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
