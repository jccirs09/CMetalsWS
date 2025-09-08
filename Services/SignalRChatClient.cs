using CMetalsWS.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public class SignalRChatClient : IChatClient
    {
        private HubConnection _hubConnection;
        private readonly NavigationManager _navigationManager;

        public event Func<ChatMessage, Task>? MessageReceived;
        public event Func<string, bool, Task>? TypingChanged;
        public event Func<string, bool, Task>? PresenceChanged;
        public event Func<string, int, Task>? ReadReceipt;

        public SignalRChatClient(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/hubs/chat"))
                .WithAutomaticReconnect()
                .Build();

            RegisterHubEventHandlers();
        }

        private void RegisterHubEventHandlers()
        {
            _hubConnection.On<ChatMessage>("ReceiveMessage", async (message) =>
            {
                if (MessageReceived != null)
                    await MessageReceived.Invoke(message);
            });

            _hubConnection.On<string, bool>("ReceiveTypingState", async (userId, isTyping) =>
            {
                if (TypingChanged != null)
                    await TypingChanged.Invoke(userId, isTyping);
            });

            _hubConnection.On<string, int>("ReceiveReadReceipt", async (userId, messageId) =>
            {
                if (ReadReceipt != null)
                    await ReadReceipt.Invoke(userId, messageId);
            });

            // TODO: Implement presence handlers if needed
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
        }

        public Task JoinGroupAsync(int groupId)
            => _hubConnection.SendAsync("JoinGroup", groupId);

        public Task LeaveGroupAsync(int groupId)
            => _hubConnection.SendAsync("LeaveGroup", groupId);

        public Task SendToUserAsync(string userId, string content)
            => _hubConnection.SendAsync("SendMessageToUser", userId, content);

        public Task SendToGroupAsync(int groupId, string content)
            => _hubConnection.SendAsync("SendMessageToGroup", groupId, content);

        public Task SetTypingAsync(string userId, bool isTyping)
            => _hubConnection.SendAsync("SetTypingStateForUser", userId, isTyping);

        public Task AckReadUserAsync(string userId, int lastMessageId)
            => _hubConnection.SendAsync("AckReadUser", userId, lastMessageId);

        public Task AckReadGroupAsync(int groupId, int lastMessageId)
            => _hubConnection.SendAsync("AckReadGroup", groupId, lastMessageId);

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
