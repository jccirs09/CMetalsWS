using CMetalsWS.Data.Chat;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services.SignalR
{
    public class ChatHubClient : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private bool _isDisposed;
        private bool _started;

        public event Func<MessageDto, Task>? MessageReceived;
        public event Func<MessageDto, Task>? ReactionAdded;
        public event Func<MessageDto, Task>? ReactionRemoved;
        public event Func<TypingDto, Task>? UserTyping;
        public event Func<object, Task>? ThreadRead; // Using object for anonymous type
        public event Func<PresenceDto, Task>? PresenceChanged;
        public event Func<ThreadSummary, Task>? ThreadUpdated;
        public event Func<MessageDto, Task>? MessageUpdated;
        public event Func<Task>? ThreadsUpdated;
        public event Func<MessageDto, Task>? MessagePinned;
        public event Func<int, Task>? MessageDeleted;

        public ChatHubClient(NavigationManager navManager)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(navManager.ToAbsoluteUri("/hubs/chat"))
                .WithAutomaticReconnect()
                .Build();

            RegisterHubEventHandlers();
        }

        public async Task ConnectAsync()
        {
            if (_started)
                return;
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
                _started = true;
            }
        }

        private void RegisterHubEventHandlers()
        {
            _hubConnection.On<MessageDto>("ReceiveMessage", (message) => MessageReceived?.Invoke(message) ?? Task.CompletedTask);
            _hubConnection.On<MessageDto>("ReactionAdded", (message) => ReactionAdded?.Invoke(message) ?? Task.CompletedTask);
            _hubConnection.On<MessageDto>("ReactionRemoved", (message) => ReactionRemoved?.Invoke(message) ?? Task.CompletedTask);
            _hubConnection.On<TypingDto>("UserTyping", (typingInfo) => UserTyping?.Invoke(typingInfo) ?? Task.CompletedTask);
            _hubConnection.On<object>("ThreadRead", (readInfo) => ThreadRead?.Invoke(readInfo) ?? Task.CompletedTask);
            _hubConnection.On<PresenceDto>("PresenceChanged", (presence) => PresenceChanged?.Invoke(presence) ?? Task.CompletedTask);
            _hubConnection.On<ThreadSummary>("ThreadUpdated", (summary) => ThreadUpdated?.Invoke(summary) ?? Task.CompletedTask);
            _hubConnection.On<MessageDto>("MessageUpdated", (message) => MessageUpdated?.Invoke(message) ?? Task.CompletedTask);
            _hubConnection.On("ThreadsUpdated", () => ThreadsUpdated?.Invoke() ?? Task.CompletedTask);
            _hubConnection.On<MessageDto>("MessagePinned", (message) => MessagePinned?.Invoke(message) ?? Task.CompletedTask);
            _hubConnection.On<int>("MessageDeleted", (messageId) => MessageDeleted?.Invoke(messageId) ?? Task.CompletedTask);
        }

        public async Task SendMessageAsync(string threadId, string content) => await _hubConnection.InvokeAsync("SendMessage", threadId, content);
        public async Task AddReactionAsync(int messageId, string emoji) => await _hubConnection.InvokeAsync("AddReaction", messageId, emoji);
        public async Task RemoveReactionAsync(int messageId, string emoji) => await _hubConnection.InvokeAsync("RemoveReaction", messageId, emoji);
        public async Task UpdateMessageAsync(int messageId, string newContent) => await _hubConnection.InvokeAsync("UpdateMessage", messageId, newContent);
        public async Task TypingAsync(string threadId, bool isTyping) => await _hubConnection.InvokeAsync("Typing", threadId, isTyping);
        public async Task MarkReadAsync(string threadId) => await _hubConnection.InvokeAsync("MarkRead", threadId);
        public async Task JoinThreadAsync(string threadId) => await _hubConnection.InvokeAsync("JoinThread", threadId);
        public async Task LeaveThreadAsync(string threadId) => await _hubConnection.InvokeAsync("LeaveThread", threadId);
        public async Task UpdatePresenceAsync(string status) => await _hubConnection.InvokeAsync("UpdatePresence", status);
        public async Task PinThreadAsync(string threadId, bool isPinned) => await _hubConnection.InvokeAsync("PinThread", threadId, isPinned);
        public async Task PinMessageAsync(int messageId, bool isPinned) => await _hubConnection.InvokeAsync("PinMessage", messageId, isPinned);
        public async Task DeleteMessageAsync(int messageId) => await _hubConnection.InvokeAsync("DeleteMessage", messageId);

        public async ValueTask DisposeAsync()
        {
            if (!_isDisposed)
            {
                if (_hubConnection != null)
                {
                    _hubConnection.Remove("ReceiveMessage");
                    _hubConnection.Remove("ReactionAdded");
                    _hubConnection.Remove("ReactionRemoved");
                    _hubConnection.Remove("UserTyping");
                    _hubConnection.Remove("ThreadRead");
                    _hubConnection.Remove("PresenceChanged");
                    _hubConnection.Remove("ThreadUpdated");
                    await _hubConnection.DisposeAsync();
                }
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
