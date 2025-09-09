using CMetalsWS.Data;
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

        public async Task<List<ApplicationUser>> SearchUsersAsync(string query, int skip = 0, int take = 30)
        {
            return await _userManager.Users
                .Where(u => u.UserName!.Contains(query))
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(take)
                .Select(u => new ApplicationUser
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Avatar = u.Avatar,
                    IsOnline = u.IsOnline
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
    }
}
