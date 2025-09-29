using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace CMetalsWS.Components.FigmaDesign
{
    public partial class MessagingCoordinator
    {
        [Parameter]
        public bool IsMessagingOpen { get; set; }

        [Parameter]
        public EventCallback<bool> IsMessagingOpenChanged { get; set; }

        private string? _activeConversationId;
        private List<string> _openConversationIds = new();

        private void OpenConversation(string conversationId)
        {
            if (!_openConversationIds.Contains(conversationId))
            {
                if (_openConversationIds.Count >= 3)
                {
                    _openConversationIds.RemoveAt(0);
                }
                _openConversationIds.Add(conversationId);
            }
            _activeConversationId = conversationId;
            StateHasChanged();
        }

        private void CloseConversation(string conversationId)
        {
            _openConversationIds.Remove(conversationId);
            if (_activeConversationId == conversationId)
            {
                _activeConversationId = _openConversationIds.LastOrDefault();
            }
            StateHasChanged();
        }

        private void ToggleMessagingSystem(bool isOpen)
        {
            IsMessagingOpen = isOpen;
            IsMessagingOpenChanged.InvokeAsync(isOpen);
        }
    }
}