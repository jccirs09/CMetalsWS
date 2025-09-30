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
        [Inject] private ChatStateService ChatState { get; set; } = default!;
        [Inject] private IChatRepository ChatRepository { get; set; } = default!;
        [Inject] private ChatHubClient ChatHubClient { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;

        private bool _isDarkMode = false;
        private bool _drawerOpen = true;
        private string? _userId;

        private readonly MudTheme _customTheme = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#2E8B57",
                Secondary = "#FFA500",
                Background = Colors.Gray.Lighten5,
                Surface = Colors.Gray.Lighten4,
                DrawerBackground = Colors.Gray.Lighten5,
                DrawerText = Colors.Gray.Darken4,
                DrawerIcon = Colors.Gray.Darken3,
                AppbarBackground = "#2E8B57",
                AppbarText = Colors.Shades.White
            },
            PaletteDark = new PaletteDark
            {
                Black = "#27272f",
                Background = "#32333d",
                BackgroundGray = "#27272f",
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

        protected override async Task OnInitializedAsync()
        {
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            _userId = auth.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ChatState.OnChange += StateHasChanged;
            NotificationService.Changed += OnNotificationsChanged;

            if (!string.IsNullOrEmpty(_userId))
            {
                await NotificationService.InitializeAsync();
                await ChatHubClient.ConnectAsync();
            }
        }

        private void OnNotificationsChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        private void OnHeaderThreadSelected(ThreadSummary t)
        {
            ChatState.HandleThreadClick(t);
        }

        public void Dispose()
        {
            ChatState.OnChange -= StateHasChanged;
            NotificationService.Changed -= OnNotificationsChanged;
        }
    }
}