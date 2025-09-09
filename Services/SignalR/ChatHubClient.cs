using CMetalsWS.Data.Chat;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CMetalsWS.Services.SignalR
{
    public class ChatHubClient : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly SemaphoreSlim _connectGate = new(1, 1);
        private bool _isDisposed;

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
                .WithUrl(navManager.ToAbsoluteUri("/chathub"))
                .WithAutomaticReconnect(new[]
                {
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                })
                .Build();

            RegisterHubEventHandlers();

            // Optional: light self-heal
            _hubConnection.Closed += async _ =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                try { await EnsureConnectedAsync(); } catch { /* swallow */ }
            };
        }

        public HubConnectionState State => _hubConnection.State;
        public bool IsConnected => State == HubConnectionState.Connected;

        // Call this at startup or before any hub action.
        public async Task EnsureConnectedAsync()
        {
            if (_isDisposed) return;
            if (IsConnected) return;

            await _connectGate.WaitAsync();
            try
            {
                if (!_isDisposed && !IsConnected)
                {
                    try { await _hubConnection.StartAsync(); }
                    catch
                    {
                        // Keep silent; SafeInvoke() will no-op if still disconnected.
                    }
                }
            }
            finally { _connectGate.Release(); }
        }

        // Back-compat
        public Task ConnectAsync() => EnsureConnectedAsync();

        private void RegisterHubEventHandlers()
        {
            _hubConnection.On<MessageDto>("ReceiveMessage", m => MessageReceived?.Invoke(m) ?? Task.CompletedTask);
            _hubConnection.On<MessageDto>("ReactionAdded", m => ReactionAdded?.Invoke(m) ?? Task.CompletedTask);
            _hubConnection.On<MessageDto>("ReactionRemoved", m => ReactionRemoved?.Invoke(m) ?? Task.CompletedTask);
            _hubConnection.On<TypingDto>("UserTyping", t => UserTyping?.Invoke(t) ?? Task.CompletedTask);
            _hubConnection.On<object>("ThreadRead", o => ThreadRead?.Invoke(o) ?? Task.CompletedTask);
            _hubConnection.On<PresenceDto>("PresenceChanged", p => PresenceChanged?.Invoke(p) ?? Task.CompletedTask);
            _hubConnection.On<ThreadSummary>("ThreadUpdated", s => ThreadUpdated?.Invoke(s) ?? Task.CompletedTask);
            _hubConnection.On<MessageDto>("MessageUpdated", m => MessageUpdated?.Invoke(m) ?? Task.CompletedTask);
            _hubConnection.On("ThreadsUpdated", () => ThreadsUpdated?.Invoke() ?? Task.CompletedTask);
            _hubConnection.On<MessageDto>("MessagePinned", m => MessagePinned?.Invoke(m) ?? Task.CompletedTask);
            _hubConnection.On<int>("MessageDeleted", id => MessageDeleted?.Invoke(id) ?? Task.CompletedTask);
        }

        // Centralized safe invoker for hub methods
        private async Task SafeInvoke(string methodName, params object?[] args)
        {
            await EnsureConnectedAsync();
            if (!IsConnected) return;
            await _hubConnection.InvokeAsync(methodName, args);
        }

        // ---- Public API (all guarded) ----
        public Task SendMessageAsync(string threadId, string content)
            => SafeInvoke("SendMessage", threadId, content);

        public Task AddReactionAsync(int messageId, string emoji)
            => SafeInvoke("AddReaction", messageId, emoji);

        public Task RemoveReactionAsync(int messageId, string emoji)
            => SafeInvoke("RemoveReaction", messageId, emoji);

        public Task UpdateMessageAsync(int messageId, string newContent)
            => SafeInvoke("UpdateMessage", messageId, newContent);

        public Task TypingAsync(string threadId, bool isTyping)
            => SafeInvoke("Typing", threadId, isTyping);

        public Task MarkReadAsync(string threadId)
            => SafeInvoke("MarkRead", threadId);

        public Task JoinThreadAsync(string threadId)
            => SafeInvoke("JoinThread", threadId);

        public Task LeaveThreadAsync(string threadId)
        {
            // During dispose/navigation we don't want to throw when disconnected
            if (!IsConnected) return Task.CompletedTask;
            return _hubConnection.InvokeAsync("LeaveThread", threadId);
        }

        public Task UpdatePresenceAsync(string status)
            => SafeInvoke("UpdatePresence", status);

        public Task PinThreadAsync(string threadId, bool isPinned)
            => SafeInvoke("PinThread", threadId, isPinned);

        public Task PinMessageAsync(int messageId, bool isPinned)
            => SafeInvoke("PinMessage", messageId, isPinned);

        public Task DeleteMessageAsync(int messageId)
            => SafeInvoke("DeleteMessage", messageId);

        // ---- Disposal ----
        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                _hubConnection.Remove("ReceiveMessage");
                _hubConnection.Remove("ReactionAdded");
                _hubConnection.Remove("ReactionRemoved");
                _hubConnection.Remove("UserTyping");
                _hubConnection.Remove("ThreadRead");
                _hubConnection.Remove("PresenceChanged");
                _hubConnection.Remove("ThreadUpdated");
                _hubConnection.Remove("ThreadsUpdated");
                _hubConnection.Remove("MessageUpdated");
                _hubConnection.Remove("MessagePinned");
                _hubConnection.Remove("MessageDeleted");

                if (State != HubConnectionState.Disconnected)
                {
                    try { await _hubConnection.StopAsync(); } catch { /* ignore */ }
                }
                await _hubConnection.DisposeAsync();
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
