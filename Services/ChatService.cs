using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CMetalsWS.Services
{
    public class ChatService : IChatService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatService(IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<ApplicationUser> userManager)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
        }

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
            return await context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == currentUserId && m.RecipientId == contactId) || (m.SenderId == contactId && m.RecipientId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            using var context = _contextFactory.CreateDbContext();
            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();
        }

        public async Task<List<ChatGroup>> GetUserGroupsAsync(string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatGroups
                .Where(g => g.ChatGroupUsers.Any(gu => gu.UserId == userId))
                .ToListAsync();
        }

        public async Task<List<ChatGroup>> GetAllGroupsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatGroups.Include(g => g.Branch).ToListAsync();
        }

        public async Task<ChatGroup> CreateGroupAsync(string name, int? branchId, List<string> userIds)
        {
            using var context = _contextFactory.CreateDbContext();
            var group = new ChatGroup
            {
                Name = name,
                BranchId = branchId
            };

            context.ChatGroups.Add(group);
            await context.SaveChangesAsync();

            foreach (var userId in userIds)
            {
                var chatGroupUser = new ChatGroupUser
                {
                    ChatGroupId = group.Id,
                    UserId = userId
                };
                context.ChatGroupUsers.Add(chatGroupUser);
            }
            await context.SaveChangesAsync();

            return group;
        }

        public async Task UpdateGroupAsync(ChatGroup group, List<string> userIds)
        {
            using var context = _contextFactory.CreateDbContext();
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
            using var context = _contextFactory.CreateDbContext();
            var group = await context.ChatGroups.FindAsync(groupId);
            if (group != null)
            {
                context.ChatGroups.Remove(group);
                await context.SaveChangesAsync();
            }
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

        public async Task<List<ChatMessage>> GetConversationBeforeAsync(string currentUserId, string contactId, DateTime before)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => ((m.SenderId == currentUserId && m.RecipientId == contactId) || (m.SenderId == contactId && m.RecipientId == currentUserId)) && m.Timestamp < before)
                .OrderByDescending(m => m.Timestamp)
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetGroupConversationBeforeAsync(int groupId, DateTime before)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChatGroupId == groupId && m.Timestamp < before)
                .OrderByDescending(m => m.Timestamp)
                .Take(20)
                .ToListAsync();
        }
    }
}
