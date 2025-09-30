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
                Primary = "#0d9488", // teal-600 from figma
                Secondary = "#f3f3f5", // input-background from figma
                Background = "#f8fafc", // A very light gray, similar to sidebar color in figma
                AppbarBackground = "#ffffff",
                AppbarText = "#030213", // secondary-foreground from figma
                DrawerBackground = "#ffffff",
                DrawerText = "#030213",
                Surface = "#ffffff",
                TextPrimary = "#030213",
                TextSecondary = "#717182", // muted-foreground from figma
                ActionDefault = "#717182",
                ActionDisabled = "#cbced4", // switch-background from figma
                LinesDefault = "rgba(0, 0, 0, 0.1)" // border from figma
            },
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "0.625rem" // radius from figma
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