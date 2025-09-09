using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CMetalsWS.Services
{
    public class ChatStateService : IDisposable
    {
        private readonly NavigationManager _navManager;
        private readonly List<string> _openDocks = new();

        // Public properties
        public bool IsChatPageOpen { get; private set; }
        public bool IsThreadPanelOpen { get; private set; }
        public string? ActiveThreadId { get; private set; }
        public IReadOnlyList<string> OpenDocks => _openDocks.AsReadOnly();
        public int MaxDocks { get; set; } = 3;

        public event Action? OnChange;

        public ChatStateService(NavigationManager navManager)
        {
            _navManager = navManager;
            // Subscribe to location changes
            _navManager.LocationChanged += OnLocationChanged;
            // Set initial state based on the current URL
            UpdateChatPageState(_navManager.Uri);
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            UpdateChatPageState(e.Location);
        }

        private void UpdateChatPageState(string location)
        {
            var uri = new Uri(location, UriKind.Absolute);
            if (uri.AbsolutePath.StartsWith("/messages"))
            {
                var match = Regex.Match(uri.AbsolutePath, @"^/messages/([^/]+)");
                var threadId = match.Success ? match.Groups[1].Value : null;
                GoToChatPage(threadId);
            }
            else
            {
                LeaveChatPage();
            }
        }

        public void GoToChatPage(string? threadId)
        {
            if (IsChatPageOpen && ActiveThreadId == threadId) return;

            IsChatPageOpen = true;
            ActiveThreadId = threadId;
            _openDocks.Clear();
            IsThreadPanelOpen = false;
            NotifyStateChanged();
        }

        public void LeaveChatPage()
        {
            if (!IsChatPageOpen) return;

            IsChatPageOpen = false;
            ActiveThreadId = null;
            NotifyStateChanged();
        }

        public void ActivateThread(string id)
        {
            if (ActiveThreadId == id) return;
            ActiveThreadId = id;
            NotifyStateChanged();
        }

        public void OpenDock(string id)
        {
            if (_openDocks.Contains(id))
            {
                return;
            }

            if (_openDocks.Count >= MaxDocks)
            {
                _openDocks.RemoveAt(0);
            }

            _openDocks.Add(id);
            IsThreadPanelOpen = false;
            NotifyStateChanged();
        }

        public void CloseDock(string id)
        {
            if (_openDocks.Remove(id))
            {
                NotifyStateChanged();
            }
        }

        public void ToggleThreadPanel()
        {
            IsThreadPanelOpen = !IsThreadPanelOpen;
            NotifyStateChanged();
        }

        public void HandleThreadClick(Data.Chat.ThreadSummary thread)
        {
            if (thread.Id == null) return;

            if (IsChatPageOpen)
            {
                _navManager.NavigateTo($"/messages/{thread.Id}");
            }
            else
            {
                OpenDock(thread.Id);
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void Dispose()
        {
            _navManager.LocationChanged -= OnLocationChanged;
        }
    }
}
