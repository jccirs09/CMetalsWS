using System.ComponentModel.DataAnnotations.Schema;

namespace CMetalsWS.Data
{
    public class ChatGroupUser
    {
        // Foreign key for the user
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Foreign key for the chat group
        public int ChatGroupId { get; set; }
        public ChatGroup? ChatGroup { get; set; }
    }
}
