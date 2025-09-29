using CMetalsWS.Data.Chat;
using CMetalsWS.Services.SignalR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CMetalsWS.Components.FigmaDesign
{
    public partial class ConversationView : IAsyncDisposable
    {
        [Inject] private IChatRepository ChatRepository { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] private ChatHubClient ChatHubClient { get; set; } = default!;

        [Parameter]
        public string? ConversationId { get; set; }

        private string? _userId;
        private List<MessageDto> _messages = new();
        private string? _newMessage;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            _userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ChatHubClient.MessageReceived += OnMessageReceived;
            await ChatHubClient.ConnectAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(ConversationId) && !string.IsNullOrEmpty(_userId))
            {
                await ChatHubClient.JoinThreadAsync(ConversationId);
                var messages = await ChatRepository.GetMessagesAsync(ConversationId, _userId);
                _messages = new List<MessageDto>(messages);
                StateHasChanged();
            }
        }

        private Task OnMessageReceived(MessageDto message)
        {
            if (message.ThreadId == ConversationId)
            {
                _messages.Add(message);
                StateHasChanged();
            }
            return Task.CompletedTask;
        }

        private async Task SendMessageAsync()
        {
            if (!string.IsNullOrEmpty(ConversationId) && !string.IsNullOrWhiteSpace(_newMessage))
            {
                await ChatHubClient.SendMessageAsync(ConversationId, _newMessage);
                _newMessage = string.Empty;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (ChatHubClient != null)
            {
                ChatHubClient.MessageReceived -= OnMessageReceived;
                if (!string.IsNullOrEmpty(ConversationId))
                {
                    await ChatHubClient.LeaveThreadAsync(ConversationId);
                }
            }
        }
    }
}