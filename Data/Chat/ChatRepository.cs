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
        private readonly UserManager<ApplicationUser> _userManager; // kept only for non-query helpers if you need later

        public ChatRepository(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<ApplicationUser> userManager)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
        }

        // -----------------------------
        // Helpers
        // -----------------------------
        private static MessageDto ToMessageDto(ChatMessage message, string currentUserId, Dictionary<string, string> reactionUsers)
        {
            var reactions = (message.Reactions ?? Enumerable.Empty<MessageReaction>())
                .Where(r => r.Emoji != null && r.UserId != null)
                .GroupBy(r => r.Emoji!)
                .ToDictionary(g => g.Key, g => g.Select(r => r.UserId!).ToHashSet());

            var seenBy = (message.SeenBy ?? Enumerable.Empty<MessageSeen>())
                .Where(s => s.UserId != null)
                .ToDictionary(s => s.UserId!, s => s.Timestamp);

            return new MessageDto
            {
                Id = message.Id,
                ThreadId = message.ChatGroupId?.ToString()
                           ?? (message.SenderId == currentUserId ? message.RecipientId : message.SenderId),
                SenderId = message.SenderId,
                SenderName = message.Sender?.UserName,
                Content = message.Content,
                CreatedAt = message.Timestamp,
                EditedAt = message.EditedAt,
                DeletedAt = message.DeletedAt,
                IsPinned = message.IsPinned,
                Reactions = reactions,
                ReactionUsers = reactionUsers,
                SeenBy = seenBy
            };
        }

        private async Task<Dictionary<string, string>> LookupUserNamesAsync(IEnumerable<string?> ids)
        {
            var list = ids.Where(id => !string.IsNullOrEmpty(id)).Cast<string>().Distinct().ToList();
            if (list.Count == 0) return new();

            await using var uctx = await _contextFactory.CreateDbContextAsync();
            return await uctx.Users
                .AsNoTracking()
                .Where(u => list.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "Unknown");
        }

        // -----------------------------
        // Messages
        // -----------------------------
        public async Task<MessageDto?> AddReactionAsync(int messageId, string emoji, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var message = await context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                .Include(m => m.SeenBy)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null) return null;

            var existing = await context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);

            if (existing == null)
            {
                context.MessageReactions.Add(new MessageReaction
                {
                    MessageId = messageId,
                    UserId = userId,
                    Emoji = emoji
                });
                await context.SaveChangesAsync();
                await context.Entry(message).Collection(m => m.Reactions).LoadAsync();
            }

            var reactionUsers = await LookupUserNamesAsync((message.Reactions ?? []).Select(r => r.UserId));
            return ToMessageDto(message, userId, reactionUsers);
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
                message.ChatGroupId = groupId;
            else
                message.RecipientId = threadId;

            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();

            await context.Entry(message).Reference(m => m.Sender).LoadAsync();
            await context.Entry(message).Collection(m => m.Reactions).LoadAsync();
            await context.Entry(message).Collection(m => m.SeenBy).LoadAsync();

            return ToMessageDto(message, senderId, new());
        }

        public async Task<bool> DeleteMessageForEveryoneAsync(int messageId, string currentUserId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var message = await context.ChatMessages.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null || message.SenderId != currentUserId) return false;

            message.DeletedAt = DateTime.UtcNow;
            message.Content = "This message was deleted.";
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<MessageDto?> GetMessageAsync(int messageId, string currentUserId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var message = await context.ChatMessages.AsNoTracking()
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                .Include(m => m.SeenBy)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null) return null;

            var reactionUsers = await LookupUserNamesAsync((message.Reactions ?? []).Select(r => r.UserId));
            return ToMessageDto(message, currentUserId, reactionUsers);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(string threadId, string currentUserId, DateTime? before = null, int take = 50)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.ChatMessages.AsNoTracking()
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
                query = query.Where(m =>
                    (m.SenderId == currentUserId && m.RecipientId == threadId) ||
                    (m.SenderId == threadId && m.RecipientId == currentUserId));
            }

            if (before.HasValue)
                query = query.Where(m => m.Timestamp < before.Value);

            var messages = await query
                .OrderByDescending(m => m.Timestamp)
                .Take(take)
                .ToListAsync();

            messages.Reverse();

            var reactionUserIds = messages
                .SelectMany(m => (m.Reactions ?? Enumerable.Empty<MessageReaction>())
                    .Select(r => r.UserId))
                .Where(id => id != null)
                .Cast<string>()
                .Distinct()
                .ToList();

            var reactionUsers = await LookupUserNamesAsync(reactionUserIds);

            return messages.Select(m => ToMessageDto(m, currentUserId, reactionUsers));
        }

        // -----------------------------
        // Participants
        // -----------------------------
        public async Task<IEnumerable<ApplicationUser>> GetThreadParticipantsAsync(string threadId, string currentUserId)
        {
            if (int.TryParse(threadId, out var groupId))
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ChatGroupUsers
                    .AsNoTracking()
                    .Where(gu => gu.ChatGroupId == groupId)
                    .Include(gu => gu.User)
                    .Select(gu => gu.User!)
                    .ToListAsync();
            }
            else
            {
                await using var uctx = await _contextFactory.CreateDbContextAsync();
                var other = await uctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == threadId);
                var me = await uctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == currentUserId);

                var list = new List<ApplicationUser>();
                if (other != null) list.Add(other);
                if (me != null) list.Add(me);
                return list;
            }
        }

        // -----------------------------
        // Thread summaries (no cross-context compose; no parallel ops)
        // -----------------------------
        public async Task<IEnumerable<ThreadSummary>> GetThreadSummariesAsync(string userId, string? searchQuery = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // 1) GROUPS user belongs to
            var groupIds = await context.ChatGroupUsers
                .AsNoTracking()
                .Where(gu => gu.UserId == userId)
                .Select(gu => gu.ChatGroupId)
                .ToListAsync();

            var groups = await context.ChatGroups
                .AsNoTracking()
                .Where(g => groupIds.Contains(g.Id))
                .Select(g => new { g.Id, g.Name })
                .ToListAsync();

            var lastGroupMsgs = await context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatGroupId.HasValue && groupIds.Contains(m.ChatGroupId.Value))
                .GroupBy(m => m.ChatGroupId!.Value)
                .Select(g => new
                {
                    GroupId = g.Key,
                    Last = g.OrderByDescending(m => m.Timestamp)
                            .Select(m => new { m.Content, m.Timestamp })
                            .FirstOrDefault()
                })
                .ToListAsync();

            var unreadGroupRows = await context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatGroupId.HasValue && groupIds.Contains(m.ChatGroupId.Value) && m.SenderId != userId)
                .Select(m => new { G = m.ChatGroupId!.Value, Seen = m.SeenBy.Any(s => s.UserId == userId) })
                .ToListAsync();

            var unreadGroupCounts = unreadGroupRows
                .Where(x => !x.Seen)
                .GroupBy(x => x.G)
                .ToDictionary(g => g.Key, g => g.Count());

            var groupParticipants = await context.ChatGroupUsers
                .AsNoTracking()
                .Where(gu => groupIds.Contains(gu.ChatGroupId))
                .GroupBy(gu => gu.ChatGroupId)
                .Select(g => new { GroupId = g.Key, UserIds = g.Select(gu => gu.UserId).ToList() })
                .ToListAsync();

            var groupSummaries = groups.Select(g =>
            {
                var last = lastGroupMsgs.FirstOrDefault(x => x.GroupId == g.Id)?.Last;
                var unread = unreadGroupCounts.TryGetValue(g.Id, out var c) ? c : 0;
                var parts = groupParticipants.FirstOrDefault(x => x.GroupId == g.Id)?.UserIds ?? new List<string>();
                return new ThreadSummary
                {
                    Id = g.Id.ToString(),
                    Title = g.Name,
                    AvatarUrl = null,
                    LastMessagePreview = last?.Content,
                    UnreadCount = unread,
                    Participants = parts,
                    LastActivityAt = last?.Timestamp ?? DateTime.MinValue
                };
            }).ToList();

            // 2) DM partners
            var dmOtherIds = await context.ChatMessages
                .AsNoTracking()
                .Where(m =>
                    (m.SenderId == userId && m.RecipientId != null) ||
                    (m.RecipientId == userId && m.SenderId != null))
                .Select(m => m.SenderId == userId ? m.RecipientId! : m.SenderId!)
                .Distinct()
                .ToListAsync();

            // Include search-matching users even with zero prior messages
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                await using var uctxSearch = await _contextFactory.CreateDbContextAsync();
                var more = await uctxSearch.Users.AsNoTracking()
                    .Where(u => u.Id != userId &&
                                ((u.UserName != null && u.UserName.Contains(searchQuery)) ||
                                 (u.Email != null && u.Email.Contains(searchQuery))))
                    .Select(u => u.Id)
                    .ToListAsync();

                dmOtherIds = dmOtherIds.Union(more).Distinct().ToList();
            }

            var lastDmMsgs = await context.ChatMessages
                .AsNoTracking()
                .Where(m =>
                    (m.SenderId == userId && m.RecipientId != null && dmOtherIds.Contains(m.RecipientId)) ||
                    (m.RecipientId == userId && m.SenderId != null && dmOtherIds.Contains(m.SenderId)))
                .GroupBy(m => (m.SenderId == userId ? m.RecipientId! : m.SenderId!))
                .Select(g => new
                {
                    OtherId = g.Key,
                    Last = g.OrderByDescending(m => m.Timestamp)
                            .Select(m => new { m.Content, m.Timestamp })
                            .FirstOrDefault()
                })
                .ToListAsync();

            var unreadDmRows = await context.ChatMessages
                .AsNoTracking()
                .Where(m => m.RecipientId == userId && m.SenderId != null && dmOtherIds.Contains(m.SenderId))
                .Select(m => new { Sender = m.SenderId!, Seen = m.SeenBy.Any(s => s.UserId == userId) })
                .ToListAsync();

            var unreadDmCounts = unreadDmRows
                .Where(x => !x.Seen)
                .GroupBy(x => x.Sender)
                .ToDictionary(g => g.Key, g => g.Count());

            // Resolve DM partner display info via a separate context (no shared DbContext)
            await using var uctx = await _contextFactory.CreateDbContextAsync();
            var dmUsers = await uctx.Users.AsNoTracking()
                .Where(u => dmOtherIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Avatar })
                .ToListAsync();

            var dmUserDict = dmUsers.ToDictionary(u => u.Id, u => new { u.UserName, u.Avatar });

            var dmSummaries = new List<ThreadSummary>(dmOtherIds.Count);
            foreach (var otherId in dmOtherIds)
            {
                dmUserDict.TryGetValue(otherId, out var uinfo);
                var last = lastDmMsgs.FirstOrDefault(x => x.OtherId == otherId)?.Last;
                unreadDmCounts.TryGetValue(otherId, out var unread);

                dmSummaries.Add(new ThreadSummary
                {
                    Id = otherId,
                    Title = uinfo?.UserName,
                    AvatarUrl = uinfo?.Avatar,
                    LastMessagePreview = last?.Content,
                    UnreadCount = unread,
                    Participants = new List<string> { userId, otherId },
                    LastActivityAt = last?.Timestamp ?? DateTime.MinValue
                });
            }

            var summaries = groupSummaries.Concat(dmSummaries).ToList();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                summaries = summaries.Where(s =>
                    (s.Title?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.LastMessagePreview?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            var pinnedIds = await context.PinnedThreads
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => p.ThreadId)
                .ToListAsync();

            foreach (var s in summaries)
                s.IsPinned = s.Id != null && pinnedIds.Contains(s.Id);

            return summaries
                .OrderByDescending(s => s.IsPinned)
                .ThenByDescending(s => s.LastActivityAt);
        }

        // -----------------------------
        // Read markers / pinning
        // -----------------------------
        public async Task MarkThreadAsReadAsync(string threadId, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;

            IQueryable<ChatMessage> q;

            if (int.TryParse(threadId, out var groupId))
                q = context.ChatMessages.Where(m => m.ChatGroupId == groupId && m.SenderId != userId);
            else
                q = context.ChatMessages.Where(m => m.SenderId == threadId && m.RecipientId == userId);

            var unseenIds = await q
                .Where(m => !m.SeenBy.Any(s => s.UserId == userId))
                .Select(m => m.Id)
                .ToListAsync();

            if (unseenIds.Count > 0)
            {
                var rows = unseenIds.Select(id => new MessageSeen
                {
                    MessageId = id,
                    UserId = userId,
                    Timestamp = now
                });
                await context.MessageSeens.AddRangeAsync(rows);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> PinMessageAsync(int messageId, bool isPinned)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var message = await context.ChatMessages.FindAsync(messageId);
            if (message == null) return false;

            if (message.ChatGroupId.HasValue)
            {
                var gid = message.ChatGroupId.Value;
                var current = await context.ChatMessages
                    .Where(m => m.ChatGroupId == gid && m.IsPinned)
                    .ToListAsync();
                foreach (var m in current) m.IsPinned = false;
            }
            else
            {
                var otherId = message.RecipientId ?? message.SenderId!;
                var current = await context.ChatMessages
                    .Where(m => (m.SenderId == otherId || m.RecipientId == otherId) && m.IsPinned)
                    .ToListAsync();
                foreach (var m in current) m.IsPinned = false;
            }

            message.IsPinned = isPinned;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PinThreadAsync(string threadId, string userId, bool isPinned)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.PinnedThreads
                .FirstOrDefaultAsync(p => p.ThreadId == threadId && p.UserId == userId);

            if (isPinned)
            {
                if (existing == null)
                {
                    context.PinnedThreads.Add(new PinnedThread { ThreadId = threadId, UserId = userId });
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            else
            {
                if (existing != null)
                {
                    context.PinnedThreads.Remove(existing);
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

            if (message == null) return null;

            var toRemove = await context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);

            if (toRemove != null)
            {
                context.MessageReactions.Remove(toRemove);
                await context.SaveChangesAsync();
                await context.Entry(message).Collection(m => m.Reactions).LoadAsync();
            }

            var reactionUsers = await LookupUserNamesAsync((message.Reactions ?? []).Select(r => r.UserId));
            return ToMessageDto(message, userId, reactionUsers);
        }

        public async Task<MessageDto?> UpdateMessageAsync(int messageId, string newContent, string currentUserId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var message = await context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Reactions)
                .Include(m => m.SeenBy)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null || message.SenderId != currentUserId) return null;

            message.Content = newContent;
            message.EditedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            var reactionUsers = await LookupUserNamesAsync((message.Reactions ?? []).Select(r => r.UserId));
            return ToMessageDto(message, currentUserId, reactionUsers);
        }

        // -----------------------------
        // Groups CRUD
        // -----------------------------
        public async Task<IEnumerable<ChatGroup>> GetAllGroupsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ChatGroups
                .AsNoTracking()
                .Include(g => g.Branch)
                .ToListAsync();
        }

        public async Task<ChatGroup> CreateGroupAsync(string name, int? branchId, List<string> userIds)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var group = new ChatGroup { Name = name, BranchId = branchId };
            context.ChatGroups.Add(group);
            await context.SaveChangesAsync();

            foreach (var uid in userIds)
                context.ChatGroupUsers.Add(new ChatGroupUser { ChatGroupId = group.Id, UserId = uid });

            await context.SaveChangesAsync();
            return group;
        }

        public async Task UpdateGroupAsync(ChatGroup group, List<string> userIds)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.ChatGroups
                .Include(g => g.ChatGroupUsers)
                .FirstOrDefaultAsync(g => g.Id == group.Id);

            if (existing != null)
            {
                existing.Name = group.Name;
                existing.BranchId = group.BranchId;

                existing.ChatGroupUsers.Clear();
                foreach (var uid in userIds)
                    existing.ChatGroupUsers.Add(new ChatGroupUser { ChatGroupId = existing.Id, UserId = uid });

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

        // -----------------------------
        // Access check
        // -----------------------------
        public async Task<bool> IsParticipantAsync(string threadId, string userId)
        {
            if (int.TryParse(threadId, out var groupId))
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ChatGroupUsers
                    .AnyAsync(gu => gu.ChatGroupId == groupId && gu.UserId == userId);
            }
            else
            {
                // 1:1 chats – if it's the other user's Id, the current user is implicitly a participant.
                return true;
            }
        }
    }
}
