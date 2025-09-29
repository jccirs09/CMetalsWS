using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CMetalsWS.Components.FigmaDesign
{
    public partial class MessagingSystem
    {
        [Parameter]
        public bool IsVisible { get; set; }

        [Parameter]
        public EventCallback<bool> IsVisibleChanged { get; set; }

        [Parameter]
        public EventCallback<string> OnOpenConversation { get; set; }

        private string? _selectedConversationId;

        private void SelectConversation(string conversationId)
        {
            _selectedConversationId = conversationId;
            StateHasChanged();
        }

        private Task Close()
        {
            IsVisible = false;
            return IsVisibleChanged.InvokeAsync(IsVisible);
        }
    }
}