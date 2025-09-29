using Microsoft.AspNetCore.Components;

namespace CMetalsWS.Components.FigmaDesign
{
    public partial class MobileMessaging
    {
        private string? _activeConversationId;

        private void ShowConversation(string conversationId)
        {
            _activeConversationId = conversationId;
            StateHasChanged();
        }

        private void ShowConversationList()
        {
            _activeConversationId = null;
            StateHasChanged();
        }
    }
}