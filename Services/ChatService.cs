using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;

namespace CMetalsWS.Services
{
    public class ChatService : IChatService, IMessageStore
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatService(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<ApplicationUser> userManager)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
        }

        // IChatService implementations
        public async Task<List<ApplicationUser>> GetUsersAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users.ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserDetailsAsync(string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<ChatMessage>> GetConversationAsync(string currentUserId, string contactId)
        {
            using var context = _contextFactory.CreateDbContext();
            var messages = await context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == currentUserId && m.RecipientId == contactId) || (m.SenderId == contactId && m.RecipientId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            return messages;
        }

        public async Task<List<ChatMessage>> GetGroupConversationAsync(int groupId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChatGroupId == groupId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetRecentConversationsAsync(string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            var userMessages = context.ChatMessages
                .Where(m => m.SenderId == userId || m.RecipientId == userId);

            var latestMessages = await userMessages
                .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
                .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
                .ToListAsync();

            var userGroupIds = await context.ChatGroupUsers
                .Where(gu => gu.UserId == userId)
                .Select(gu => gu.ChatGroupId)
                .ToListAsync();

            var latestGroupMessages = await context.ChatMessages
                .Where(m => m.ChatGroupId.HasValue && userGroupIds.Contains(m.ChatGroupId.Value))
                .GroupBy(m => m.ChatGroupId)
                .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
                .ToListAsync();

            var recentConversations = latestMessages.Concat(latestGroupMessages)
                .OrderByDescending(m => m.Timestamp)
                .ToList();

            return recentConversations;
        }

        public async Task<ChatGroup?> GetGroupAsync(int groupId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatGroups.FindAsync(groupId);
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            await SaveOutgoingAsync(message);
        }

        // IMessageStore implementations
        async Task<IReadOnlyList<ChatMessage>> IMessageStore.LoadUserThreadAsync(string me, string other, int take)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == me && m.RecipientId == other) || (m.SenderId == other && m.RecipientId == me))
                .OrderByDescending(m => m.Timestamp)
                .Take(take)
                .ToListAsync();
        }

        async Task<IReadOnlyList<ChatMessage>> IMessageStore.LoadUserThreadBeforeAsync(string me, string other, DateTime before, int take)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => ((m.SenderId == me && m.RecipientId == other) || (m.SenderId == other && m.RecipientId == me)) && m.Timestamp < before)
                .OrderByDescending(m => m.Timestamp)
                .Take(take)
                .ToListAsync();
        }

        async Task<IReadOnlyList<ChatMessage>> IMessageStore.LoadGroupThreadAsync(int groupId, int take)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChatGroupId == groupId)
                .OrderByDescending(m => m.Timestamp)
                .Take(take)
                .ToListAsync();
        }

        async Task<IReadOnlyList<ChatMessage>> IMessageStore.LoadGroupThreadBeforeAsync(int groupId, DateTime before, int take)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChatGroupId == groupId && m.Timestamp < before)
                .OrderByDescending(m => m.Timestamp)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> SaveOutgoingAsync(ChatMessage msg)
        {
            using var context = _contextFactory.CreateDbContext();
            context.ChatMessages.Add(msg);
            await context.SaveChangesAsync();
            return msg.Id;
        }

        public Task MarkDeliveredAsync(int tempId, int finalId)
        {
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(int tempId)
        {
            return Task.CompletedTask;
        }
    }
}
