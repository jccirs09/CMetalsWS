using CMetalsWS.Data.Chat;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CMetalsWS.Services.SignalR
{
    public class ChatHubClient : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly SemaphoreSlim _connectGate = new(1, 1);
        private bool _isDisposed;
        private readonly Func<Exception?, Task> _closedHandler;

        public event Func<MessageDto, Task>? MessageReceived;
        public event Func<MessageDto, Task>? ReactionAdded;
        public event Func<MessageDto, Task>? ReactionRemoved;
        public event Func<TypingDto, Task>? UserTyping;
        public event Func<object, Task>? ThreadRead;
        public event Func<PresenceDto, Task>? PresenceChanged;
        public event Func<ThreadSummary, Task>? ThreadUpdated;
        public event Func<MessageDto, Task>? MessageUpdated;
        public event Func<Task>? ThreadsUpdated;
        public event Func<MessageDto, Task>? MessagePinned;
        public event Func<int, Task>? MessageDeleted;

        public ChatHubClient(NavigationManager navManager, IHttpContextAccessor httpContextAccessor)
        {
            var uri = new Uri(navManager.BaseUri);
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(navManager.ToAbsoluteUri("/chathub"), options =>
                {
                    if (httpContextAccessor.HttpContext != null)
                    {
                        foreach (var cookie in httpContextAccessor.HttpContext.Request.Cookies)
                        {
                            options.Cookies.Add(new Cookie(cookie.Key, cookie.Value) { Domain = uri.Host });
                        }
                    }
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                .Build();

            RegisterHubEventHandlers();

            _closedHandler = async (error) =>
            {
                if (_isDisposed) return;
                if (error != null) Console.WriteLine($"SignalR connection closed: {error}");
                await Task.Delay(5000);
                if (!_isDisposed) await EnsureConnectedAsync();
            };
            _hubConnection.Closed += _closedHandler;
        }

        public HubConnectionState State => _hubConnection.State;
        public bool IsConnected => State == HubConnectionState.Connected;

        public async Task EnsureConnectedAsync()
        {
            if (_isDisposed || IsConnected) return;
            await _connectGate.WaitAsync();
            try
            {
                if (_isDisposed || IsConnected) return;
                await _hubConnection.StartAsync();
            }
            finally
            {
                _connectGate.Release();
            }
        }

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

        private async Task InvokeApi(Func<Task> hubAction)
        {
            if (_isDisposed) return;
            try
            {
                await EnsureConnectedAsync();
                if (!IsConnected) return;
                await hubAction();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invoking hub method: {ex}");
                // In a real app, you might use a proper logging framework
                // or a UI notification service to show a user-friendly error.
            }
        }

        // ---- Public API ----
        public Task SendMessageAsync(string threadId, string content) => InvokeApi(() => _hubConnection.InvokeAsync("SendMessage", threadId, content));
        public Task AddReactionAsync(int messageId, string emoji) => InvokeApi(() => _hubConnection.InvokeAsync("AddReaction", messageId, emoji));
        public Task RemoveReactionAsync(int messageId, string emoji) => InvokeApi(() => _hubConnection.InvokeAsync("RemoveReaction", messageId, emoji));
        public Task UpdateMessageAsync(int messageId, string newContent) => InvokeApi(() => _hubConnection.InvokeAsync("UpdateMessage", messageId, newContent));
        public Task MarkReadAsync(string threadId) => InvokeApi(() => _hubConnection.InvokeAsync("MarkRead", threadId));
        public Task JoinThreadAsync(string threadId) => InvokeApi(() => _hubConnection.InvokeAsync("JoinThread", threadId));
        public Task LeaveThreadAsync(string threadId) => InvokeApi(() => _hubConnection.InvokeAsync("LeaveThread", threadId));
        public Task UpdatePresenceAsync(string status) => InvokeApi(() => _hubConnection.InvokeAsync("UpdatePresence", status));
        public Task PinThreadAsync(string threadId, bool isPinned) => InvokeApi(() => _hubConnection.InvokeAsync("PinThread", threadId, isPinned));
        public Task PinMessageAsync(int messageId, bool isPinned) => InvokeApi(() => _hubConnection.InvokeAsync("PinMessage", messageId, isPinned));
        public Task DeleteMessageAsync(int messageId) => InvokeApi(() => _hubConnection.InvokeAsync("DeleteMessage", messageId));

        // Typing is fire-and-forget, so it uses SendAsync
        public Task TypingAsync(string threadId, bool isTyping) => InvokeApi(() => _hubConnection.SendAsync("Typing", threadId, isTyping));

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            if (_hubConnection != null)
            {
                _hubConnection.Closed -= _closedHandler;
                try { await _hubConnection.StopAsync(); }
                catch (Exception ex) { Console.WriteLine($"Error stopping hub connection: {ex}"); }
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
