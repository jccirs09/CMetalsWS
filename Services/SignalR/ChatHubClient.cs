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
            catch(Exception ex)
            {
                Console.WriteLine($"Error starting SignalR connection: {ex}");
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
            _hubConnection.On<MessageDto>("InboxNewMessage", m => InboxNewMessage?.Invoke(m) ?? Task.CompletedTask);
        }

        private async Task InvokeApi(Func<HubConnection, Task> action, string methodName)
        {
            if (_isDisposed) return;
            await EnsureConnectedAsync();
            if (!IsConnected)
            {
                Console.WriteLine($"Cannot invoke '{methodName}', connection not active.");
                return;
            }

            try
            {
                await action(_hubConnection);
            }
            catch (Exception ex)
            {
                // This is often called during component disposal, so we just log instead of throwing.
                Console.WriteLine($"Ignoring error during {methodName}: {ex.Message}");
            }
        }
        public event Func<MessageDto, Task>? InboxNewMessage;

        public Task SendMessageAsync(string threadId, string content) => InvokeApi(hub => hub.InvokeAsync("SendMessage", threadId, content), nameof(SendMessageAsync));
        public Task AddReactionAsync(int messageId, string emoji) => InvokeApi(hub => hub.InvokeAsync("AddReaction", messageId, emoji), nameof(AddReactionAsync));
        public Task RemoveReactionAsync(int messageId, string emoji) => InvokeApi(hub => hub.InvokeAsync("RemoveReaction", messageId, emoji), nameof(RemoveReactionAsync));
        public Task UpdateMessageAsync(int messageId, string newContent) => InvokeApi(hub => hub.InvokeAsync("UpdateMessage", messageId, newContent), nameof(UpdateMessageAsync));
        public Task MarkReadAsync(string threadId) => InvokeApi(hub => hub.InvokeAsync("MarkRead", threadId), nameof(MarkReadAsync));
        public Task JoinThreadAsync(string threadId) => InvokeApi(hub => hub.InvokeAsync("JoinThread", threadId), nameof(JoinThreadAsync));
        public Task LeaveThreadAsync(string threadId) => InvokeApi(hub => hub.InvokeAsync("LeaveThread", threadId), nameof(LeaveThreadAsync));
        public Task UpdatePresenceAsync(string status) => InvokeApi(hub => hub.InvokeAsync("UpdatePresence", status), nameof(UpdatePresenceAsync));
        public Task PinThreadAsync(string threadId, bool isPinned) => InvokeApi(hub => hub.InvokeAsync("PinThread", threadId, isPinned), nameof(PinThreadAsync));
        public Task PinMessageAsync(int messageId, bool isPinned) => InvokeApi(hub => hub.InvokeAsync("PinMessage", messageId, isPinned), nameof(PinMessageAsync));
        public Task DeleteMessageAsync(int messageId) => InvokeApi(hub => hub.InvokeAsync("DeleteMessage", messageId), nameof(DeleteMessageAsync));
        public Task TypingAsync(string threadId, bool isTyping) => InvokeApi(hub => hub.SendAsync("Typing", threadId, isTyping), nameof(TypingAsync));

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
