using CMetalsWS.Data.Chat;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Data.Chat
{
    public class ChatRepository : IChatRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatRepository(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<ApplicationUser> userManager)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
        }

        public async Task<MessageDto?> AddReactionAsync(int messageId, string emoji, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var message = await context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                .Include(m => m.SeenBy)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                return null;
            }

            var existingReaction = await context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);

            if (existingReaction == null)
            {
                var reaction = new MessageReaction
                {
                    MessageId = messageId,
                    UserId = userId,
                    Emoji = emoji
                };
                context.MessageReactions.Add(reaction);
                await context.SaveChangesAsync();

                // Reload reactions for the DTO
                await context.Entry(message).Collection(m => m.Reactions).LoadAsync();
            }

            return ToMessageDto(message, userId);
        }

        public async Task<MessageDto> CreateMessageAsync(string threadId, string senderId, string content)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var message = new ChatMessage
            {
                SenderId = senderId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            if (int.TryParse(threadId, out var groupId))
            {
                message.ChatGroupId = groupId;
            }
            else
            {
                message.RecipientId = threadId;
            }

            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();

            // Reload the sender to include it in the DTO
            await context.Entry(message).Reference(m => m.Sender).LoadAsync();

            return ToMessageDto(message, senderId);
        }

        public async Task<bool> DeleteMessageForEveryoneAsync(int messageId, string currentUserId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var message = await context.ChatMessages.FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                return false;
            }

            // Add policy checks in the service/hub layer (e.g., only sender or admin can delete)
            if (message.SenderId != currentUserId)
            {
                 // For now, let's be strict. This should be a policy decision.
                 return false;
            }

            message.DeletedAt = DateTime.UtcNow;
            message.Content = "This message was deleted."; // Or set to null, depending on UI handling

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<MessageDto?> GetMessageAsync(int messageId, string currentUserId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var message = await context.ChatMessages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                .Include(m => m.SeenBy)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            return message == null ? null : ToMessageDto(message, currentUserId);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(string threadId, string currentUserId, DateTime? before = null, int take = 50)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.ChatMessages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                .Include(m => m.SeenBy)
                .AsQueryable();

            if (int.TryParse(threadId, out var groupId))
            {
                query = query.Where(m => m.ChatGroupId == groupId);
            }
            else
            {
                query = query.Where(m => (m.SenderId == currentUserId && m.RecipientId == threadId) || (m.SenderId == threadId && m.RecipientId == currentUserId));
            }

            if (before.HasValue)
            {
                query = query.Where(m => m.Timestamp < before.Value);
            }

            var messages = await query.OrderByDescending(m => m.Timestamp).Take(take).ToListAsync();

            // Reverse the list to have the newest message last
            messages.Reverse();

            return messages.Select(m => ToMessageDto(m, currentUserId));
        }

        private static MessageDto ToMessageDto(ChatMessage message, string currentUserId)
        {
            return new MessageDto
            {
                Id = message.Id,
                ThreadId = message.ChatGroupId?.ToString() ?? (message.SenderId == currentUserId ? message.RecipientId : message.SenderId),
                SenderId = message.SenderId,
                SenderName = message.Sender?.UserName,
                Content = message.Content,
                CreatedAt = message.Timestamp,
                EditedAt = message.EditedAt,
                DeletedAt = message.DeletedAt,
                Reactions = message.Reactions
                    .GroupBy(r => r.Emoji)
                    .ToDictionary(g => g.Key, g => g.Select(r => r.UserId).ToHashSet()),
                SeenBy = message.SeenBy.ToDictionary(s => s.UserId, s => s.Timestamp)
            };
        }

        public async Task<IEnumerable<ApplicationUser>> GetThreadParticipantsAsync(string threadId, string currentUserId)
        {
            if (int.TryParse(threadId, out var groupId))
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ChatGroupUsers
                    .Where(gu => gu.ChatGroupId == groupId)
                    .Select(gu => gu.User)
                    .Where(u => u != null)
                    .ToListAsync()!;
            }
            else
            {
                var otherUser = await _userManager.FindByIdAsync(threadId);
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                var result = new List<ApplicationUser>();
                if (otherUser != null) result.Add(otherUser);
                if (currentUser != null) result.Add(currentUser);
                return result;
            }
        }

        public async Task<IEnumerable<ThreadSummary>> GetThreadSummariesAsync(string userId, string? searchQuery = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Get all messages where the user is either the sender or recipient
            var userMessages = context.ChatMessages
                .Where(m => m.SenderId == userId || m.RecipientId == userId || (m.ChatGroupId.HasValue && m.ChatGroup.ChatGroupUsers.Any(gu => gu.UserId == userId)))
                .OrderByDescending(m => m.Timestamp)
                .Select(m => new
                {
                    ThreadId = m.ChatGroupId.HasValue ? m.ChatGroupId.ToString() : (m.SenderId == userId ? m.RecipientId : m.SenderId),
                    m.Content,
                    m.Timestamp,
                    IsRead = m.SeenBy.Any(s => s.UserId == userId) || m.SenderId == userId,
                    m.SenderId,
                    m.RecipientId,
                    m.ChatGroupId,
                    m.ChatGroup
                });

            // Group messages by thread to get the latest message and unread count for each thread
            var threadSummaries = await userMessages
                .GroupBy(m => m.ThreadId)
                .Select(g => new
                {
                    ThreadId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault()
                })
                .ToListAsync();

            var summaries = new List<ThreadSummary>();
            foreach (var summary in threadSummaries)
            {
                if (summary.LastMessage == null || summary.ThreadId == null) continue;

                var unreadCount = await context.ChatMessages
                    .CountAsync(m => (m.ChatGroupId.ToString() == summary.ThreadId || (m.SenderId == summary.ThreadId && m.RecipientId == userId)) && !m.SeenBy.Any(s => s.UserId == userId) && m.SenderId != userId);

                if (int.TryParse(summary.ThreadId, out var groupId))
                {
                    var group = await context.ChatGroups
                        .Include(g => g.ChatGroupUsers)
                        .ThenInclude(gu => gu.User)
                        .FirstOrDefaultAsync(g => g.Id == groupId);

                    if (group != null)
                    {
                        summaries.Add(new ThreadSummary
                        {
                            Id = group.Id.ToString(),
                            Title = group.Name,
                            AvatarUrl = null, // Groups don't have avatars in this schema
                            LastMessagePreview = summary.LastMessage.Content,
                            UnreadCount = unreadCount,
                            Participants = group.ChatGroupUsers.Select(gu => gu.User?.Id).Where(id => id != null).ToList()!,
                            LastActivityAt = summary.LastMessage.Timestamp
                        });
                    }
                }
                else
                {
                    var otherUserId = summary.ThreadId;
                    var otherUser = await _userManager.FindByIdAsync(otherUserId);
                    if (otherUser != null)
                    {
                        summaries.Add(new ThreadSummary
                        {
                            Id = otherUser.Id,
                            Title = otherUser.UserName,
                            AvatarUrl = otherUser.Avatar,
                            LastMessagePreview = summary.LastMessage.Content,
                            UnreadCount = unreadCount,
                            Participants = new List<string> { userId, otherUserId },
                            LastActivityAt = summary.LastMessage.Timestamp
                        });
                    }
                }
            }

            var pinnedThreads = await context.PinnedThreads
                .Where(p => p.UserId == userId)
                .Select(p => p.ThreadId)
                .ToListAsync();

            foreach (var summary in summaries)
            {
                if (pinnedThreads.Contains(summary.Id!))
                {
                    summary.IsPinned = true;
                }
            }

            return summaries.OrderByDescending(s => s.IsPinned).ThenByDescending(s => s.LastActivityAt);
        }

        public async Task MarkThreadAsReadAsync(string threadId, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var now = DateTime.UtcNow;

            IQueryable<ChatMessage> messagesToMark;
            if (int.TryParse(threadId, out var groupId))
            {
                messagesToMark = context.ChatMessages
                    .Where(m => m.ChatGroupId == groupId && m.SenderId != userId);
            }
            else
            {
                messagesToMark = context.ChatMessages
                    .Where(m => m.SenderId == threadId && m.RecipientId == userId);
            }

            var unseenMessageIds = await messagesToMark
                .Where(m => !m.SeenBy.Any(s => s.UserId == userId))
                .Select(m => m.Id)
                .ToListAsync();

            if (unseenMessageIds.Any())
            {
                var seenRecords = unseenMessageIds.Select(messageId => new MessageSeen
                {
                    MessageId = messageId,
                    UserId = userId,
                    Timestamp = now
                });
                await context.MessageSeens.AddRangeAsync(seenRecords);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> PinMessageAsync(int messageId, bool isPinned)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var messageToPin = await context.ChatMessages.FindAsync(messageId);
            if (messageToPin == null)
            {
                return false;
            }

            // Unpin any other message in the same thread
            var threadId = messageToPin.ChatGroupId?.ToString() ?? messageToPin.RecipientId ?? messageToPin.SenderId;
            if (threadId != null)
            {
                var currentlyPinned = await context.ChatMessages
                    .Where(m => (m.ChatGroupId.ToString() == threadId || (m.SenderId == threadId || m.RecipientId == threadId)) && m.IsPinned)
                    .ToListAsync();

                foreach (var msg in currentlyPinned)
                {
                    msg.IsPinned = false;
                }
            }

            messageToPin.IsPinned = isPinned;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PinThreadAsync(string threadId, string userId, bool isPinned)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingPin = await context.PinnedThreads.FirstOrDefaultAsync(p => p.ThreadId == threadId && p.UserId == userId);

            if (isPinned)
            {
                if (existingPin == null)
                {
                    context.PinnedThreads.Add(new PinnedThread { ThreadId = threadId, UserId = userId });
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            else
            {
                if (existingPin != null)
                {
                    context.PinnedThreads.Remove(existingPin);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<MessageDto?> RemoveReactionAsync(int messageId, string emoji, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var message = await context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                .Include(m => m.SeenBy)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                return null;
            }

            var reactionToRemove = await context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);

            if (reactionToRemove != null)
            {
                context.MessageReactions.Remove(reactionToRemove);
                await context.SaveChangesAsync();

                // Reload reactions for the DTO
                await context.Entry(message).Collection(m => m.Reactions).LoadAsync();
            }

            return ToMessageDto(message, userId);
        }

        public async Task<MessageDto?> UpdateMessageAsync(int messageId, string newContent, string currentUserId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var message = await context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                .Include(m => m.SeenBy)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null || message.SenderId != currentUserId)
            {
                return null;
            }

            // Optional: Add time limit check here or in a service layer
            // if ((DateTime.UtcNow - message.Timestamp).TotalMinutes > 15)
            // {
            //     return null;
            // }

            message.Content = newContent;
            message.EditedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return ToMessageDto(message, currentUserId);
        }

        public async Task<IEnumerable<ChatGroup>> GetAllGroupsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ChatGroups.Include(g => g.Branch).ToListAsync();
        }

        public async Task<ChatGroup> CreateGroupAsync(string name, int? branchId, List<string> userIds)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var group = new ChatGroup
            {
                Name = name,
                BranchId = branchId
            };

            context.ChatGroups.Add(group);
            await context.SaveChangesAsync();

            foreach (var userId in userIds)
            {
                context.ChatGroupUsers.Add(new ChatGroupUser { ChatGroupId = group.Id, UserId = userId });
            }
            await context.SaveChangesAsync();

            return group;
        }

        public async Task UpdateGroupAsync(ChatGroup group, List<string> userIds)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingGroup = await context.ChatGroups
                .Include(g => g.ChatGroupUsers)
                .FirstOrDefaultAsync(g => g.Id == group.Id);

            if (existingGroup != null)
            {
                existingGroup.Name = group.Name;
                existingGroup.BranchId = group.BranchId;

                existingGroup.ChatGroupUsers.Clear();
                foreach (var userId in userIds)
                {
                    existingGroup.ChatGroupUsers.Add(new ChatGroupUser { UserId = userId });
                }

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var group = await context.ChatGroups.FindAsync(groupId);
            if (group != null)
            {
                context.ChatGroups.Remove(group);
                await context.SaveChangesAsync();
            }
        }
    }
}
