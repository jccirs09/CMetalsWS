using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
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

        private bool _isDarkMode = false;
        private bool _drawerOpen = true;
        private bool _isChatThreadsPanelOpen;
        private MudIconButton? _messagesBtnRef;

        private readonly List<ChatThreadRef> _openChatThreads = new();

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

        private async void Logout()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated ?? false)
            {
                NavigationManager.NavigateTo("Account/Logout", true);
            }
        }

        private void ToggleChatThreadsPanel() => _isChatThreadsPanelOpen = !_isChatThreadsPanelOpen;

        private void OpenChatWindowFromConversation(ThreadSummary summary)
        {
            if (_openChatThreads.Any(t => t.Id == summary.Id)) return;
            _openChatThreads.Add(new ChatThreadRef { Id = summary.Id, Title = summary.Title });
        }

        private void CloseChatWindow(ChatThreadRef thread) => _openChatThreads.Remove(thread);

        private void MinimizeChatWindow(ChatThreadRef thread) => thread.IsMinimized = !thread.IsMinimized;
    }

    public class ChatThreadRef
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public ApplicationUser? User { get; set; }
        public ChatGroup? Group { get; set; }
        public bool IsMinimized { get; set; }
    }
}
