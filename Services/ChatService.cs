using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class ChatService : IChatService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ChatService(UserManager<ApplicationUser> userManager, IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _userManager = userManager;
            _contextFactory = contextFactory;
        }

        public async Task<Dictionary<string, List<ApplicationUser>>> SearchUsersAsync(string query, int? branchId = null, int skip = 0, int take = 30)
        {
            var usersQuery = _userManager.Users
                .Include(u => u.Branch)
                .Where(u => u.UserName!.Contains(query));

            if (branchId.HasValue)
                usersQuery = usersQuery.Where(u => u.BranchId == branchId);

            var users = await usersQuery
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(take)
                .Select(u => new ApplicationUser
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Avatar = u.Avatar,
                    IsOnline = u.IsOnline,
                    BranchId = u.BranchId,
                    Branch = u.Branch == null ? null : new Branch { Id = u.Branch.Id, Name = u.Branch.Name }
                }).ToListAsync();

            return users
                .GroupBy(u => u.Branch?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task<List<ApplicationUser>> GetBranchUsersAsync(int branchId, int skip = 0, int take = 100)
        {
            return await _userManager.Users
                .Where(u => u.BranchId == branchId)
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(take)
                .Select(u => new ApplicationUser
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Avatar = u.Avatar,
                    IsOnline = u.IsOnline,
                    BranchId = u.BranchId
                }).ToListAsync();
        }

        public async Task<List<ChatGroup>> SearchGroupsAsync(string query, int skip = 0, int take = 30)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ChatGroups
                .Where(g => g.Name!.Contains(query))
                .OrderBy(g => g.Name)
                .Skip(skip)
                .Take(take)
                .Select(g => new ChatGroup { Id = g.Id, Name = g.Name })
                .ToListAsync();
        }

        public async Task<ThreadSummary> GetOrCreateThreadAsync(string currentUserId, string? otherUserId = null, int? groupId = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            if (groupId.HasValue)
            {
                var group = await context.ChatGroups
                    .Include(g => g.ChatGroupUsers)
                    .FirstOrDefaultAsync(g => g.Id == groupId.Value);
                if (group == null) throw new KeyNotFoundException("Group not found");
                return new ThreadSummary
                {
                    Id = group.Id.ToString(),
                    Title = group.Name,
                    Participants = group.ChatGroupUsers.Select(gu => gu.UserId).ToList()
                };
            }

            if (!string.IsNullOrEmpty(otherUserId))
            {
                var otherUser = await _userManager.FindByIdAsync(otherUserId);
                if (otherUser == null) throw new KeyNotFoundException("User not found");

                // check if any messages exist between users (thread exists implicitly)
                _ = await context.ChatMessages.AnyAsync(m =>
                    (m.SenderId == currentUserId && m.RecipientId == otherUserId) ||
                    (m.SenderId == otherUserId && m.RecipientId == currentUserId));

                return new ThreadSummary
                {
                    Id = otherUser.Id,
                    Title = otherUser.UserName,
                    AvatarUrl = otherUser.Avatar,
                    Participants = new List<string> { currentUserId, otherUser.Id }
                };
            }

            throw new ArgumentException("Either otherUserId or groupId must be provided");
        }
    }
}
