// using CMetalsWS.Components.Chat;
using CMetalsWS.Data;
using CMetalsWS.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Collections.Generic;
using System.Linq;

namespace CMetalsWS.Components.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        // [Inject] private IChatService ChatService { get; set; } = default!;

        private bool _isDarkMode = false;
        private bool _drawerOpen = true;
        private bool _isChatThreadsPanelOpen = false;
        // private List<ChatThread> _openChatThreads = new();

        private readonly MudTheme _customTheme = new()
        {
            PaletteLight = new PaletteLight
            {
                // brand colors
                Primary = "#2E8B57",          // SeaGreen
                Secondary = "#FFA500",        // Orange

                // backgrounds and surfaces
                Background = Colors.Gray.Lighten5,
                Surface = Colors.Gray.Lighten4,
                DrawerBackground = Colors.Gray.Lighten5,
                DrawerText = Colors.Gray.Darken4,
                DrawerIcon = Colors.Gray.Darken3,

                // app bar
                AppbarBackground = "#2E8B57",
                AppbarText = Colors.Shades.White
            },
            PaletteDark = new PaletteDark
            {
                Black = "#27272f",
                Background = "#32333d",
                BackgroundGray = "#27272f",   // was BackgroundGrey
                Surface = "#373740",
                TextPrimary = "#ffffffb3",
                TextSecondary = "rgba(255,255,255,0.50)"
            }
        };

        private void ToggleChatThreadsPanel()
        {
            _isChatThreadsPanelOpen = !_isChatThreadsPanelOpen;
        }

        // private void OpenChatWindow(ApplicationUser user)
        // {
        //     var existingThread = _openChatThreads.FirstOrDefault(t => t.User?.Id == user.Id);
        //     if (existingThread == null)
        //     {
        //         _openChatThreads.Add(new ChatThread { User = user });
        //     }
        // }

        // private void OpenChatWindow(ChatGroup group)
        // {
        //     var existingThread = _openChatThreads.FirstOrDefault(t => t.Group?.Id == group.Id);
        //     if (existingThread == null)
        //     {
        //         _openChatThreads.Add(new ChatThread { Group = group });
        //     }
        // }

        // private void CloseChatWindow(ChatThread thread)
        // {
        //     _openChatThreads.Remove(thread);
        // }

        // private void MinimizeChatWindow(ChatThread thread)
        // {
        //     thread.IsMinimized = !thread.IsMinimized;
        // }

        // private async void OpenChatWindowFromConversation(ChatMessage message)
        // {
        //     if (message.ChatGroupId.HasValue)
        //     {
        //         var group = await ChatService.GetGroupAsync(message.ChatGroupId.Value);
        //         if (group != null)
        //         {
        //             OpenChatWindow(group);
        //         }
        //     }
        //     else
        //     {
        //         var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        //         var currentUser = authState.User;
        //         var currentUserId = currentUser.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        //         var user = message.SenderId == currentUserId ? message.Recipient : message.Sender;
        //         OpenChatWindow(user);
        //     }
        //     _isChatThreadsPanelOpen = false;
        // }

        private async void Logout()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated ?? false)
            {
                NavigationManager.NavigateTo("Account/Logout", true);
            }
        }
    }

    // public class ChatThread
    // {
    //     public ApplicationUser? User { get; set; }
    //     public ChatGroup? Group { get; set; }
    //     public bool IsMinimized { get; set; }
    // }
}
