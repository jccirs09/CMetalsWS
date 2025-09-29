using CMetalsWS.Data;
using CMetalsWS.Data.Chat;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CMetalsWS.Components.FigmaDesign
{
    public partial class ConversationList
    {
        [Inject] private IChatRepository ChatRepository { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        [Parameter]
        public EventCallback<string> OnConversationSelected { get; set; }

        private string? _userId;
        private IEnumerable<ThreadSummary> _threads = new List<ThreadSummary>();
        private IEnumerable<UserBasics> _contacts = new List<UserBasics>();
        private IEnumerable<ChatGroup> _groups = new List<ChatGroup>();

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            _userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(_userId))
            {
                _threads = await ChatRepository.GetThreadSummariesAsync(_userId);
                _contacts = await ChatRepository.GetContactsAsync(_userId);
                _groups = await ChatRepository.GetAllGroupsAsync();
            }
        }

        private Task SelectConversation(string conversationId)
        {
            return OnConversationSelected.InvokeAsync(conversationId);
        }
    }
}