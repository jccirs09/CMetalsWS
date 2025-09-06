using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace CMetalsWS.Components.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private bool _isDarkMode = false;
        private bool _drawerOpen = true;
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
    }
}
