// window-scoped helpers for chat components
window.cmetalsChat = {
  scrollToBottom: function (el) {
    if (!el) return;
    requestAnimationFrame(()=>{ el.scrollTop = el.scrollHeight; });
  },
  isNearTop: function (el, threshold) {
    if (!el) return false;
    return el.scrollTop <= (threshold ?? 24);
  },
  isNearBottom: function (el, threshold) {
    if (!el) return false;
    const t = threshold ?? 48;
    return (el.scrollHeight - el.scrollTop - el.clientHeight) <= t;
  },
  preserveScroll: function (el, prevHeight) {
    if (!el) return;
    el.scrollTop = el.scrollHeight - (prevHeight - el.scrollTop);
  }
};
