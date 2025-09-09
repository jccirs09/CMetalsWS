window.cmetalsChat = {
    scrollToBottom: (elementId) => {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollTop = element.scrollHeight;
        }
    },
    playSound: (sound) => {
        const audio = new Audio(`sounds/${sound}.mp3`);
        audio.play();
    },
    isDocumentFocused: () => {
        return document.hasFocus();
    },
    requestNotificationPermission: async () => {
        if (!("Notification" in window)) {
            return "unsupported";
        }
        return await Notification.requestPermission();
    },
    showNotification: (title, body) => {
        if (Notification.permission === "granted") {
            new Notification(title, { body: body });
        }
    },
    getCursorPosition: (elementId) => {
        const element = document.getElementById(elementId);
        if (element) {
            return element.selectionStart;
        }
        return -1;
    },
    getScrollHeight: (elementId) => {
        const element = document.getElementById(elementId);
        if (element) {
            return element.scrollHeight;
        }
        return 0;
    },
    setScrollTop: (elementId, position) => {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollTop = position;
        }
    },
    addScrollListener: (elementId, dotNetHelper) => {
        const element = document.getElementById(elementId);
        if (element) {
            element.addEventListener('scroll', () => {
                if (element.scrollTop === 0) {
                    dotNetHelper.invokeMethodAsync('OnScrollToTop');
                }
            });
        }
    }
};
