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

            _hubConnection.Closed += async (error) =>
            {
                if (error != null)
                {
                    Console.WriteLine($"SignalR connection closed due to an error: {error}");
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
                await EnsureConnectedAsync();
            };
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

                Console.WriteLine("Attempting to connect to SignalR hub...");
                await _hubConnection.StartAsync();
                Console.WriteLine("SignalR connection established.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to SignalR hub: {ex}");
                // Re-throw the exception so the caller knows the connection failed.
                throw;
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

        private async Task SafeInvoke(string methodName, params object?[] args)
        {
            try
            {
                await EnsureConnectedAsync();
                await _hubConnection.InvokeAsync(methodName, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invoking hub method '{methodName}': {ex}");
                // Depending on the app's needs, you might want to show a notification to the user.
            }
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
                _hubConnection.Closed -= null; // Unsubscribe from all Closed events
                if (State != HubConnectionState.Disconnected)
                {
                    await _hubConnection.StopAsync();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error during ChatHubClient disposal: {ex}");
            }
            finally
            {
                await _hubConnection.DisposeAsync();
                GC.SuppressFinalize(this);
            }
        }
    }
}
