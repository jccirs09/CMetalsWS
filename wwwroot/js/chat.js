// window-scoped helpers for chat components
window.cmetalsChat = {
  scrollToBottom: function (el) {
    if (!el) return;
    el.scrollTop = el.scrollHeight;
  },
  isNearTop: function (el, threshold) {
    if (!el) return false;
    return el.scrollTop <= (threshold ?? 32);
  },
  isNearBottom: function (el, threshold) {
    if (!el) return false;
    const t = threshold ?? 48;
    return (el.scrollHeight - el.scrollTop - el.clientHeight) <= t;
  }
};
