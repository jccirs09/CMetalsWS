using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CMetalsWS.Components.FigmaDesign
{
    public partial class DockedConversation
    {
        [Parameter]
        public string? ConversationId { get; set; }

        [Parameter]
        public EventCallback<string> OnClose { get; set; }

        private bool _isMinimized = false;

        private void ToggleMinimize()
        {
            _isMinimized = !_isMinimized;
        }

        private Task Close()
        {
            return OnClose.InvokeAsync(ConversationId);
        }
    }
}