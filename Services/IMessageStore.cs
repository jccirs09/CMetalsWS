using CMetalsWS.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMetalsWS.Services
{
    public interface IMessageStore
    {
        Task<IReadOnlyList<ChatMessage>> LoadUserThreadAsync(string me, string other, int take = 50);
        Task<IReadOnlyList<ChatMessage>> LoadUserThreadBeforeAsync(string me, string other, DateTime before, int take = 50);

        Task<IReadOnlyList<ChatMessage>> LoadGroupThreadAsync(int groupId, int take = 50);
        Task<IReadOnlyList<ChatMessage>> LoadGroupThreadBeforeAsync(int groupId, DateTime before, int take = 50);

        Task<int> SaveOutgoingAsync(ChatMessage msg);   // returns db id
        Task MarkDeliveredAsync(int tempId, int finalId);
        Task MarkFailedAsync(int tempId);
    }
}
