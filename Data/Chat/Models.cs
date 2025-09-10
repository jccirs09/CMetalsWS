using System;
using System.Collections.Generic;
using CMetalsWS.Data;

namespace CMetalsWS.Data.Chat
{
    // Entities

    public class MessageReaction
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public ChatMessage? Message { get; set; }
        public string? Emoji { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }

    public class MessageSeen
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public ChatMessage? Message { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PinnedThread
    {
        public int Id { get; set; }
        public string ThreadId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

    // DTOs

    public class ThreadSummary
    {
        public string? Id { get; set; } // Can be a user ID or a group ID
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{FirstName} {LastName}".Trim();
        public string? AvatarUrl { get; set; }
        public string? LastMessagePreview { get; set; }
        public int UnreadCount { get; set; }
        public bool IsPinned { get; set; }
        public List<string>? Participants { get; set; }
        public DateTime LastActivityAt { get; set; }
    }

    public class ThreadHeader
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Presence { get; set; }
        public int Members { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string? ThreadId { get; set; }
        public string? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    public bool IsPinned { get; set; }
        public Dictionary<string, HashSet<string>> Reactions { get; set; } = new();
        public Dictionary<string, string> ReactionUsers { get; set; } = new(); // UserId -> UserName
        public Dictionary<string, DateTime> SeenBy { get; set; } = new();
    }

    public class TypingDto
    {
        public string? ThreadId { get; set; }
        public string? UserId { get; set; }
        public bool IsTyping { get; set; }
    }

    public class PresenceDto
    {
        public string? UserId { get; set; }
        public string? Status { get; set; }
    }
}
