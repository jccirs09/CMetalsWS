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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<ApplicationUser>> GetUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<ApplicationUser> GetUserDetailsAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<List<ChatMessage>> GetConversationAsync(string currentUserId, string contactId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == currentUserId && m.RecipientId == contactId) || (m.SenderId == contactId && m.RecipientId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatGroup>> GetUserGroupsAsync(string userId)
        {
            return await _context.ChatGroups
                .Where(g => g.ChatGroupUsers.Any(gu => gu.UserId == userId))
                .ToListAsync();
        }

        public async Task<List<ChatGroup>> GetAllGroupsAsync()
        {
            return await _context.ChatGroups.Include(g => g.Branch).ToListAsync();
        }

        public async Task<ChatGroup> CreateGroupAsync(string name, int? branchId, List<string> userIds)
        {
            var group = new ChatGroup
            {
                Name = name,
                BranchId = branchId
            };

            _context.ChatGroups.Add(group);
            await _context.SaveChangesAsync();

            foreach (var userId in userIds)
            {
                var chatGroupUser = new ChatGroupUser
                {
                    ChatGroupId = group.Id,
                    UserId = userId
                };
                _context.ChatGroupUsers.Add(chatGroupUser);
            }
            await _context.SaveChangesAsync();

            return group;
        }

        public async Task UpdateGroupAsync(ChatGroup group, List<string> userIds)
        {
            var existingGroup = await _context.ChatGroups
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

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            var group = await _context.ChatGroups.FindAsync(groupId);
            if (group != null)
            {
                _context.ChatGroups.Remove(group);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ChatMessage>> GetGroupConversationAsync(int groupId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ChatGroupId == groupId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }
}
