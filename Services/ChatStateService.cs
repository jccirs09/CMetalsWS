using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMetalsWS.Services
{
    public class ChatStateService : IDisposable
    {
        private readonly NavigationManager _navManager;

        public bool IsChatPageOpen { get; private set; }
        public bool IsThreadPanelOpen { get; private set; }
        public string? ActiveThreadId { get; private set; }
        public List<string> OpenDocks { get; } = new List<string>();
        public int MaxDocks { get; set; } = 3;

        public event Action? OnChange;

        public ChatStateService(NavigationManager navManager)
        {
            _navManager = navManager;
            _navManager.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            if (_navManager.Uri.Contains("/messages"))
            {
                if (!IsChatPageOpen) GoToChatPage();
            }
            else
            {
                if (IsChatPageOpen) LeaveChatPage();
            }
        }

        public void ToggleThreadPanel()
        {
            IsThreadPanelOpen = !IsThreadPanelOpen;
            NotifyStateChanged();
        }

        public void OpenDock(string threadId)
        {
            if (OpenDocks.Contains(threadId))
            {
                // Already open, maybe focus it
                return;
            }

            if (OpenDocks.Count >= MaxDocks)
            {
                // Close the least recently used dock (the first one in the list)
                OpenDocks.RemoveAt(0);
            }

            OpenDocks.Add(threadId);
            IsThreadPanelOpen = false; // Close panel when a dock opens
            NotifyStateChanged();
        }

        public void CloseDock(string threadId)
        {
            OpenDocks.Remove(threadId);
            NotifyStateChanged();
        }

        public void ActivateThread(string? threadId)
        {
            ActiveThreadId = threadId;
            NotifyStateChanged();
        }

        public void GoToChatPage(string? threadId = null)
        {
            IsChatPageOpen = true;
            ActiveThreadId = threadId;
            OpenDocks.Clear();
            IsThreadPanelOpen = false;
            NotifyStateChanged();
        }

        public void LeaveChatPage()
        {
            IsChatPageOpen = false;
            ActiveThreadId = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void Dispose()
        {
            _navManager.LocationChanged -= OnLocationChanged;
        }
    }
}
