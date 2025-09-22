using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
using CMetalsWS.Services;
using CMetalsWS.Services.SignalR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;


namespace CMetalsWS.Components.Layout
{
    public partial class MainLayout : LayoutComponentBase, IDisposable
    {
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        //[Inject] private ChatStateService ChatState { get; set; } = default!;
        //[Inject] IChatRepository ChatRepository { get; set; } = default!;
        //[Inject]ChatHubClient ChatHubClient { get; set; } = default!;

        private bool _isDarkMode = false;
        private bool _drawerOpen = true;
        private string? _userId;
        private int _unreadTotal;


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

        //protected override void OnInitialized()
        //{
        //    ChatState.OnChange += StateHasChanged;
        //}

        //private void OnHeaderThreadSelected(CMetalsWS.Data.Chat.ThreadSummary t)
        //{
        //    // Off page -> open dock (or navigate if already on messages page, handled by service)
        //    //ChatState.HandleThreadClick(t);
        //}
        protected override async Task OnInitializedAsync()
        {
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            _userId = auth.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? auth.User.FindFirst("sub")?.Value
                     ?? auth.User.FindFirst("oid")?.Value
                     ?? auth.User.FindFirst("uid")?.Value;

            //ChatHubClient.ThreadsUpdated += OnThreadsChanged;
            //ChatHubClient.InboxNewMessage += OnInboxNewMessage;
            //await ChatHubClient.ConnectAsync();

            //await RecalcUnread();
        }

        //private async Task RecalcUnread()
        //{
        //    if (string.IsNullOrEmpty(_userId)) { _unreadTotal = 0; return; }
        //    var threads = await ChatRepository.GetThreadSummariesAsync(_userId);
        //    _unreadTotal = threads.Sum(t => t.UnreadCount);
        //}

        //private async Task OnThreadsChanged()
        //{
        //    await RecalcUnread();
        //    await InvokeAsync(StateHasChanged);
        //}

        //private async Task OnInboxNewMessage(MessageDto _)
        //{
        //    await RecalcUnread(); // fast recompute on new message
        //    await InvokeAsync(StateHasChanged);
        //}

        private void OpenNotifications()
        {
            // optional: open a drawer or navigate to /chat
        }

        public void Dispose()
        {
            //ChatHubClient.ThreadsUpdated -= OnThreadsChanged;
            //ChatHubClient.InboxNewMessage -= OnInboxNewMessage;
        }
        
    }
}
