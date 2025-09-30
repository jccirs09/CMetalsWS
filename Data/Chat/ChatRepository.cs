using CMetalsWS.Data.Chat;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMetalsWS.Data.Chat
{
    public sealed class UserBasics
    {
        public string Id { get; init; } = "";
        public string? UserName { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Avatar { get; init; }
    }
    public class ChatRepository : IChatRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatRepository(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<ApplicationUser> userManager)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
        }

        private static bool IsGroupThread(string threadId, out int groupId)
        {
            groupId = 0;
            if (threadId != null && threadId.StartsWith("g:"))
            {
                return int.TryParse(threadId.Substring(2), out groupId);
            }
            return false;
        }
        public async Task<UserBasics?> GetUserBasicsAsync(string id)
        {
            await using var c = await _contextFactory.CreateDbContextAsync();
            return await c.Users.AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserBasics
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Avatar = u.Avatar
                })
                .FirstOrDefaultAsync();
        }
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
                ThreadId = message.ChatGroupId.HasValue ? $"g:{message.ChatGroupId.Value}" : (message.SenderId == currentUserId ? message.RecipientId : message.SenderId),
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

        public async Task<MessageDto?> AddReactionAsync(int messageId, string emoji, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var message = await context.ChatMessages.AsSplitQuery().Include(m => m.Sender).Include(m => m.Reactions).Include(m => m.SeenBy).FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null) return null;
            var existing = await context.MessageReactions.FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);
            if (existing == null)
            {
                context.MessageReactions.Add(new MessageReaction { MessageId = messageId, UserId = userId, Emoji = emoji });
                await context.SaveChangesAsync();
                await context.Entry(message).Collection(m => m.Reactions).LoadAsync();
            }
            var reactionUsers = await LookupUserNamesAsync((message.Reactions ?? []).Select(r => r.UserId));
            return ToMessageDto(message, userId, reactionUsers);
        }

        public async Task<MessageDto> CreateMessageAsync(string threadId, string senderId, string content)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (string.IsNullOrWhiteSpace(threadId)) throw new InvalidOperationException("ThreadId is required.");
            if (string.IsNullOrWhiteSpace(senderId)) throw new InvalidOperationException("SenderId is required.");
            if (string.IsNullOrWhiteSpace(content)) content = string.Empty;
            var message = new ChatMessage { SenderId = senderId, Content = content, Timestamp = DateTime.UtcNow };
            if (IsGroupThread(threadId, out var groupId))
            {
                var exists = await context.ChatGroups.AsNoTracking().AnyAsync(g => g.Id == groupId);
                if (!exists) throw new InvalidOperationException($"Chat group '{groupId}' was not found.");
                message.ChatGroupId = groupId;
            }
            else
            {
                var recipientExists = await context.Users.AsNoTracking().AnyAsync(u => u.Id == threadId);
                if (!recipientExists) throw new InvalidOperationException($"Recipient user '{threadId}' was not found.");
                message.RecipientId = threadId;
            }
            context.ChatMessages.Add(message);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException dbx)
            {
                throw new InvalidOperationException($"Failed to save message: {dbx.GetBaseException().Message}", dbx);
            }
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
            var message = await context.ChatMessages.AsNoTracking().AsSplitQuery().Include(m => m.Sender).Include(m => m.Reactions).Include(m => m.SeenBy).FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null) return null;
            var reactionUsers = await LookupUserNamesAsync((message.Reactions ?? []).Select(r => r.UserId));
            return ToMessageDto(message, currentUserId, reactionUsers);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(string threadId, string currentUserId, DateTime? before = null, int take = 50)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.ChatMessages.AsNoTracking().AsSplitQuery().Include(m => m.Sender).Include(m => m.Reactions).Include(m => m.SeenBy).AsQueryable();
            if (IsGroupThread(threadId, out var groupId))
            {
                query = query.Where(m => m.ChatGroupId == groupId);
            }
            else
            {
                query = query.Where(m => (m.SenderId == currentUserId && m.RecipientId == threadId) || (m.SenderId == threadId && m.RecipientId == currentUserId));
            }
            if (before.HasValue) query = query.Where(m => m.Timestamp < before.Value);
            var messages = await query.OrderByDescending(m => m.Timestamp).Take(take).ToListAsync();
            messages.Reverse();
            var reactionUserIds = messages.SelectMany(m => (m.Reactions ?? Enumerable.Empty<MessageReaction>()).Select(r => r.UserId)).Where(id => id != null).Cast<string>().Distinct().ToList();
            var reactionUsers = await LookupUserNamesAsync(reactionUserIds);
            return messages.Select(m => ToMessageDto(m, currentUserId, reactionUsers));
        }

        public async Task<IEnumerable<ApplicationUser>> GetThreadParticipantsAsync(string threadId, string currentUserId)
        {
            if (IsGroupThread(threadId, out var groupId))
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ChatGroupUsers.AsNoTracking().Where(gu => gu.ChatGroupId == groupId).Include(gu => gu.User).Select(gu => gu.User!).ToListAsync();
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

        public async Task<IEnumerable<ThreadSummary>> GetThreadSummariesAsync(string userId, string? searchQuery = null)
        {
            var (items, _) = await GetThreadSummariesAsync(userId, searchQuery, 0, 1000); // Call the paged version
            return items;
        }

        public async Task MarkThreadAsReadAsync(string threadId, string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;
            IQueryable<ChatMessage> q;
            if (IsGroupThread(threadId, out var groupId))
                q = context.ChatMessages.Where(m => m.ChatGroupId == groupId && m.SenderId != userId);
            else
                q = context.ChatMessages.Where(m => m.SenderId == threadId && m.RecipientId == userId);
            var unseenIds = await q.Where(m => !m.SeenBy.Any(s => s.UserId == userId)).Select(m => m.Id).ToListAsync();
            if (unseenIds.Count > 0)
            {
                var rows = unseenIds.Select(id => new MessageSeen { MessageId = id, UserId = userId, Timestamp = now });
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
                var current = await context.ChatMessages.Where(m => m.ChatGroupId == gid && m.IsPinned).ToListAsync();
                foreach (var m in current) m.IsPinned = false;
            }
            else
            {
                var otherId = message.RecipientId ?? message.SenderId!;
                var current = await context.ChatMessages.Where(m => (m.SenderId == otherId || m.RecipientId == otherId) && m.IsPinned).ToListAsync();
                foreach (var m in current) m.IsPinned = false;
            }
            message.IsPinned = isPinned;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PinThreadAsync(string threadId, string userId, bool isPinned)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existing = await context.PinnedThreads.FirstOrDefaultAsync(p => p.ThreadId == threadId && p.UserId == userId);
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
            var message = await context.ChatMessages.Include(m => m.Sender).Include(m => m.Reactions).Include(m => m.SeenBy).FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null) return null;
            var toRemove = await context.MessageReactions.FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId && r.Emoji == emoji);
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
            var message = await context.ChatMessages.Include(m => m.Sender).Include(m => m.Reactions).Include(m => m.SeenBy).FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null || message.SenderId != currentUserId) return null;
            message.Content = newContent;
            message.EditedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            var reactionUsers = await LookupUserNamesAsync((message.Reactions ?? []).Select(r => r.UserId));
            return ToMessageDto(message, currentUserId, reactionUsers);
        }

        public async Task<IEnumerable<ChatGroup>> GetAllGroupsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ChatGroups.AsNoTracking().Include(g => g.Branch).ToListAsync();
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
            var existing = await context.ChatGroups.Include(g => g.ChatGroupUsers).FirstOrDefaultAsync(g => g.Id == group.Id);
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

        public async Task<bool> IsParticipantAsync(string threadId, string userId)
        {
            if (IsGroupThread(threadId, out var groupId))
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ChatGroupUsers.AnyAsync(gu => gu.ChatGroupId == groupId && gu.UserId == userId);
            }
            else
            {
                return true;
            }
        }

        public async Task<(IReadOnlyList<ThreadSummary> Items, int Total)> GetThreadSummariesAsync(
            string userId, string? searchQuery, int skip, int take, CancellationToken ct = default)
        {
            if (take <= 0) take = 25;
            if (skip < 0) skip = 0;
            ct.ThrowIfCancellationRequested();

            await using var db = await _contextFactory.CreateDbContextAsync();

            // 1. Get Pinned Threads
            var pinnedThreadIds = await db.PinnedThreads
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => p.ThreadId)
                .ToHashSetAsync(ct);

            // 2. Get Group Summaries
            var myGroupIds = await db.ChatGroupUsers
                .AsNoTracking()
                .Where(gu => gu.UserId == userId)
                .Select(gu => gu.ChatGroupId)
                .ToListAsync(ct);

            var groupSummaries = new List<ThreadSummary>();
            if (myGroupIds.Any())
            {
                var groupDetails = await db.ChatGroups
                    .AsNoTracking()
                    .Where(g => myGroupIds.Contains(g.Id))
                    .Select(g => new { g.Id, g.Name })
                    .ToListAsync(ct);

                foreach (var group in groupDetails)
                {
                    var lastMessage = await db.ChatMessages
                        .AsNoTracking()
                        .Where(m => m.ChatGroupId == group.Id)
                        .OrderByDescending(m => m.Timestamp)
                        .FirstOrDefaultAsync(ct);

                    var unreadCount = await db.ChatMessages
                        .AsNoTracking()
                        .CountAsync(m => m.ChatGroupId == group.Id && m.SenderId != userId && !m.SeenBy.Any(s => s.UserId == userId), ct);

                    groupSummaries.Add(new ThreadSummary
                    {
                        Id = $"g:{group.Id}",
                        Title = group.Name,
                        LastMessagePreview = lastMessage?.Content,
                        UnreadCount = unreadCount,
                        LastActivityAt = lastMessage?.Timestamp ?? DateTime.MinValue,
                        IsPinned = pinnedThreadIds.Contains($"g:{group.Id}")
                    });
                }
            }

            // 3. Get DM Summaries
            var dmPeers = await db.ChatMessages
                .AsNoTracking()
                .Where(m => m.SenderId == userId || m.RecipientId == userId)
                .Select(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
                .Distinct()
                .ToListAsync(ct);

            var dmUserInfos = await db.Users
                .AsNoTracking()
                .Where(u => dmPeers.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.FirstName, u.LastName, u.Avatar })
                .ToDictionaryAsync(u => u.Id, u => u, ct);

            var dmSummaries = new List<ThreadSummary>();
            foreach (var peerId in dmPeers)
            {
                if (peerId == null) continue;

                var lastMessage = await db.ChatMessages
                    .AsNoTracking()
                    .Where(m => (m.SenderId == userId && m.RecipientId == peerId) || (m.SenderId == peerId && m.RecipientId == userId))
                    .OrderByDescending(m => m.Timestamp)
                    .FirstOrDefaultAsync(ct);

                var unreadCount = await db.ChatMessages
                    .AsNoTracking()
                    .CountAsync(m => m.SenderId == peerId && m.RecipientId == userId && !m.SeenBy.Any(s => s.UserId == userId), ct);

                dmUserInfos.TryGetValue(peerId, out var userInfo);

                dmSummaries.Add(new ThreadSummary
                {
                    Id = peerId,
                    Title = userInfo?.UserName,
                    FirstName = userInfo?.FirstName,
                    LastName = userInfo?.LastName,
                    AvatarUrl = userInfo?.Avatar,
                    LastMessagePreview = lastMessage?.Content,
                    UnreadCount = unreadCount,
                    LastActivityAt = lastMessage?.Timestamp ?? DateTime.MinValue,
                    IsPinned = pinnedThreadIds.Contains(peerId)
                });
            }

            // 4. Combine and Filter
            var allSummaries = groupSummaries.Concat(dmSummaries).ToList();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim();
                allSummaries = allSummaries.Where(s =>
                    (s.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.FirstName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.LastName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (s.LastMessagePreview?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            // 5. Sort and Page
            var total = allSummaries.Count;
            var page = allSummaries
                .OrderByDescending(s => s.IsPinned)
                .ThenByDescending(s => s.LastActivityAt)
                .Skip(skip)
                .Take(take)
                .ToList();

            return (page, total);
        }
    }
}