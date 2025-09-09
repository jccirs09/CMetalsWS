using CMetalsWS.Data;
using System;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IChatClient : IAsyncDisposable
    {
        Task ConnectAsync();
        Task JoinGroupAsync(int groupId);
        Task LeaveGroupAsync(int groupId);

        Task SendToUserAsync(string userId, string content);
        Task SendToGroupAsync(int groupId, string content);
        Task SetTypingAsync(string userId, bool isTyping);
        Task AckReadUserAsync(string userId, int lastMessageId);
        Task AckReadGroupAsync(int groupId, int lastMessageId);

        event Func<ChatMessage, Task>? MessageReceived;
        event Func<string, bool, Task>? TypingChanged;   // (userId, isTyping)
        event Func<string, bool, Task>? PresenceChanged; // (userId, online)
        event Func<string, int, Task>? ReadReceipt;      // (userId, messageId)

    }
}
