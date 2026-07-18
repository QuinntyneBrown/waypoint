/* ============================================================
   Word Up — shared runtime (classic script, window.WordUp)
   No fetch, no modules, no external requests. Works on file://.
   Public API:
     WordUp.base, WordUp.href(path)
     WordUp.mountShell({role, active, track})
     WordUp.init()                        (auto-run on DOMContentLoaded)
     WordUp.openModal(id) / closeModal(id)
     WordUp.toast(msg, {type,title,duration})
     WordUp.setOffline(bool) / switchTrack('bible'|'coding')
     WordUp.data
   ============================================================ */
(function () {
  "use strict";

  // ---- base path (derived from this script's own src) ----
  var me = document.currentScript;
  var base = "./";
  if (me && me.src) {
    base = me.src.replace(/assets\/js\/wordup\.js.*$/, "");
  }

  var WordUp = window.WordUp = {
    base: base,
    href: function (p) { return base + String(p).replace(/^\/+/, ""); }
  };

  /* ---------------- Icon sprite ---------------- */
  var ICONS = {
    "home": '<path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>',
    "compass": '<circle cx="12" cy="12" r="10"/><polygon points="16.24 7.76 14.12 14.12 7.76 16.24 9.88 9.88 16.24 7.76"/>',
    "book-open": '<path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"/><path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"/>',
    "book": '<path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/>',
    "users": '<path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>',
    "user": '<path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/>',
    "user-check": '<path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="8.5" cy="7" r="4"/><polyline points="17 11 19 13 23 9"/>',
    "user-plus": '<path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="8.5" cy="7" r="4"/><line x1="20" y1="8" x2="20" y2="14"/><line x1="23" y1="11" x2="17" y2="11"/>',
    "calendar": '<rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>',
    "bell": '<path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/>',
    "search": '<circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>',
    "settings": '<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/>',
    "check": '<polyline points="20 6 9 17 4 12"/>',
    "check-circle": '<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>',
    "x": '<line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>',
    "x-circle": '<circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>',
    "plus": '<line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>',
    "minus": '<line x1="5" y1="12" x2="19" y2="12"/>',
    "edit": '<path d="M12 20h9"/><path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4z"/>',
    "trash": '<polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/>',
    "upload": '<path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/>',
    "download": '<path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/>',
    "filter": '<polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3"/>',
    "sliders": '<line x1="4" y1="21" x2="4" y2="14"/><line x1="4" y1="10" x2="4" y2="3"/><line x1="12" y1="21" x2="12" y2="12"/><line x1="12" y1="8" x2="12" y2="3"/><line x1="20" y1="21" x2="20" y2="16"/><line x1="20" y1="12" x2="20" y2="3"/><line x1="1" y1="14" x2="7" y2="14"/><line x1="9" y1="8" x2="15" y2="8"/><line x1="17" y1="16" x2="23" y2="16"/>',
    "menu": '<line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="18" x2="21" y2="18"/>',
    "log-out": '<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/>',
    "log-in": '<path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"/><polyline points="10 17 15 12 10 7"/><line x1="15" y1="12" x2="3" y2="12"/>',
    "award": '<circle cx="12" cy="8" r="7"/><polyline points="8.21 13.89 7 23 12 20 17 23 15.79 13.88"/>',
    "bar-chart": '<line x1="18" y1="20" x2="18" y2="10"/><line x1="12" y1="20" x2="12" y2="4"/><line x1="6" y1="20" x2="6" y2="14"/>',
    "pie-chart": '<path d="M21.21 15.89A10 10 0 1 1 8 2.83"/><path d="M22 12A10 10 0 0 0 12 2v10z"/>',
    "trending-up": '<polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/><polyline points="17 6 23 6 23 12"/>',
    "trending-down": '<polyline points="23 18 13.5 8.5 8.5 13.5 1 6"/><polyline points="17 18 23 18 23 12"/>',
    "clipboard": '<path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/><rect x="8" y="2" width="8" height="4" rx="1"/>',
    "clipboard-check": '<path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/><rect x="8" y="2" width="8" height="4" rx="1"/><path d="M9 14l2 2 4-4"/>',
    "message-square": '<path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>',
    "send": '<line x1="22" y1="2" x2="11" y2="13"/><polygon points="22 2 15 22 11 13 2 9 22 2"/>',
    "chevron-right": '<polyline points="9 18 15 12 9 6"/>',
    "chevron-left": '<polyline points="15 18 9 12 15 6"/>',
    "chevron-down": '<polyline points="6 9 12 15 18 9"/>',
    "chevron-up": '<polyline points="18 15 12 9 6 15"/>',
    "chevrons-right": '<polyline points="13 17 18 12 13 7"/><polyline points="6 17 11 12 6 7"/>',
    "arrow-left": '<line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/>',
    "arrow-right": '<line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/>',
    "external-link": '<path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><polyline points="15 3 21 3 21 9"/><line x1="10" y1="14" x2="21" y2="3"/>',
    "wifi-off": '<line x1="1" y1="1" x2="23" y2="23"/><path d="M16.72 11.06A10.94 10.94 0 0 1 19 12.55"/><path d="M5 12.55a10.94 10.94 0 0 1 5.17-2.39"/><path d="M10.71 5.05A16 16 0 0 1 22.58 9"/><path d="M1.42 9a15.91 15.91 0 0 1 4.7-2.88"/><path d="M8.53 16.11a6 6 0 0 1 6.95 0"/><line x1="12" y1="20" x2="12.01" y2="20"/>',
    "refresh": '<polyline points="23 4 23 10 17 10"/><polyline points="1 20 1 14 7 14"/><path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"/>',
    "alert-triangle": '<path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>',
    "alert-circle": '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>',
    "info": '<circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>',
    "help-circle": '<circle cx="12" cy="12" r="10"/><path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3"/><line x1="12" y1="17" x2="12.01" y2="17"/>',
    "lock": '<rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>',
    "mail": '<path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>',
    "eye": '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/>',
    "eye-off": '<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/>',
    "star": '<polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>',
    "flag": '<path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z"/><line x1="4" y1="22" x2="4" y2="15"/>',
    "clock": '<circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>',
    "map-pin": '<path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/>',
    "play": '<polygon points="5 3 19 12 5 21 5 3"/>',
    "play-circle": '<circle cx="12" cy="12" r="10"/><polygon points="10 8 16 12 10 16 10 8"/>',
    "file-text": '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/>',
    "folder": '<path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/>',
    "grid": '<rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/>',
    "target": '<circle cx="12" cy="12" r="10"/><circle cx="12" cy="12" r="6"/><circle cx="12" cy="12" r="2"/>',
    "heart": '<path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/>',
    "shield": '<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>',
    "code": '<polyline points="16 18 22 12 16 6"/><polyline points="8 6 2 12 8 18"/>',
    "zap": '<polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/>',
    "camera": '<path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/>',
    "paperclip": '<path d="M21.44 11.05l-9.19 9.19a6 6 0 0 1-8.49-8.49l9.19-9.19a4 4 0 0 1 5.66 5.66l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"/>',
    "thumbs-up": '<path d="M14 9V5a3 3 0 0 0-3-3l-4 9v11h11.28a2 2 0 0 0 2-1.7l1.38-9a2 2 0 0 0-2-2.3zM7 22H4a2 2 0 0 1-2-2v-7a2 2 0 0 1 2-2h3"/>',
    "more-vertical": '<circle cx="12" cy="12" r="1"/><circle cx="12" cy="5" r="1"/><circle cx="12" cy="19" r="1"/>',
    "more-horizontal": '<circle cx="12" cy="12" r="1"/><circle cx="19" cy="12" r="1"/><circle cx="5" cy="12" r="1"/>',
    "sparkles": '<path d="M12 3l1.8 4.7L18.5 9.5l-4.7 1.8L12 16l-1.8-4.7L5.5 9.5l4.7-1.8z"/><path d="M19 14l.7 1.9 1.9.7-1.9.7-.7 1.9-.7-1.9-1.9-.7 1.9-.7z"/>',
    "graduation-cap": '<path d="M22 10L12 5 2 10l10 5 10-5z"/><path d="M6 12v5c0 1 3 3 6 3s6-2 6-3v-5"/>',
    "church": '<path d="M12 2v6"/><path d="M9 5h6"/><path d="M12 8l7 4v10H5V12z"/><path d="M9 22v-5a3 3 0 0 1 6 0v5"/>',
    "phone": '<path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.13.96.36 1.9.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.91.34 1.85.57 2.81.7A2 2 0 0 1 22 16.92z"/>',
    "printer": '<polyline points="6 9 6 2 18 2 18 9"/><path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2"/><rect x="6" y="14" width="12" height="8"/>',
    "copy": '<rect x="9" y="9" width="13" height="13" rx="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/>',
    "link": '<path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/>',
    "circle": '<circle cx="12" cy="12" r="10"/>',
    "activity": '<polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/>',
    "list": '<line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/>',
    "bookmark": '<path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"/>',
    "key": '<path d="M21 2l-2 2m-7.61 7.61a5.5 5.5 0 1 1-7.778 7.778 5.5 5.5 0 0 1 7.777-7.777zm0 0L15.5 7.5m0 0l3 3L22 7l-3-3"/>',
    "video": '<polygon points="23 7 16 12 23 17 23 7"/><rect x="1" y="5" width="15" height="14" rx="2"/>',
    "headphones": '<path d="M3 18v-6a9 9 0 0 1 18 0v6"/><path d="M21 19a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3zM3 19a2 2 0 0 0 2 2h1a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H3z"/>',
    "layers": '<polygon points="12 2 2 7 12 12 22 7 12 2"/><polyline points="2 17 12 22 22 17"/><polyline points="2 12 12 17 22 12"/>',
    "columns": '<path d="M12 3h7a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-7m0-18H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h7m0-18v18"/>',
    "table": '<rect x="3" y="3" width="18" height="18" rx="2"/><line x1="3" y1="9" x2="21" y2="9"/><line x1="3" y1="15" x2="21" y2="15"/><line x1="12" y1="3" x2="12" y2="21"/>',
    "mic": '<path d="M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 6 0V4a3 3 0 0 0-3-3z"/><path d="M19 10v2a7 7 0 0 1-14 0v-2"/><line x1="12" y1="19" x2="12" y2="23"/><line x1="8" y1="23" x2="16" y2="23"/>',
    "sun": '<circle cx="12" cy="12" r="5"/><line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/><line x1="4.2" y1="4.2" x2="5.6" y2="5.6"/><line x1="18.4" y1="18.4" x2="19.8" y2="19.8"/><line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/><line x1="4.2" y1="19.8" x2="5.6" y2="18.4"/><line x1="18.4" y1="5.6" x2="19.8" y2="4.2"/>',
    "smile": '<circle cx="12" cy="12" r="10"/><path d="M8 14s1.5 2 4 2 4-2 4-2"/><line x1="9" y1="9" x2="9.01" y2="9"/><line x1="15" y1="9" x2="15.01" y2="9"/>'
  };

  function buildSprite() {
    if (document.getElementById("wu-sprite")) return;
    var parts = ['<svg id="wu-sprite" width="0" height="0" style="position:absolute" aria-hidden="true" focusable="false">'];
    for (var k in ICONS) {
      if (ICONS.hasOwnProperty(k)) {
        parts.push('<symbol id="i-' + k + '" viewBox="0 0 24 24">' + ICONS[k] + '</symbol>');
      }
    }
    parts.push("</svg>");
    var d = document.createElement("div");
    d.innerHTML = parts.join("");
    document.body.insertBefore(d.firstChild, document.body.firstChild);
  }
  function icon(name, cls) {
    return '<svg class="wu-icon ' + (cls || "") + '" aria-hidden="true"><use href="#i-' + name + '"></use></svg>';
  }
  WordUp.icon = icon;

  /* ---------------- Shared mock dataset ---------------- */
  WordUp.data = {
    org: { name: "Word Up", tagline: "Raising up young leaders who love Jesus", city: "Toronto" },
    tracks: [
      { id: "bible", name: "Bible & Leadership", icon: "book-open" },
      { id: "coding", name: "Coding for Kingdom", icon: "code" }
    ],
    // Youth / trainees
    youth: [
      { name: "Jayden Alcantara", cohort: "Rexdale Fall '25", mentor: "Marcus Bello", progress: 82, status: "on-track", age: 15, last: "2h ago" },
      { name: "Amara Okafor", cohort: "Rexdale Fall '25", mentor: "Marcus Bello", progress: 74, status: "on-track", age: 16, last: "Yesterday" },
      { name: "Devon Grant", cohort: "Rexdale Fall '25", mentor: "Marcus Bello", progress: 38, status: "at-risk", age: 14, last: "6 days ago" },
      { name: "Priya Sharma", cohort: "Rexdale Fall '25", mentor: "Grace Adeyemi", progress: 91, status: "on-track", age: 17, last: "1h ago" },
      { name: "Malik Johnson", cohort: "Rexdale Fall '25", mentor: "Grace Adeyemi", progress: 55, status: "on-track", age: 15, last: "3d ago" },
      { name: "Sofia Nguyen", cohort: "Warden Winter '26", mentor: "Grace Adeyemi", progress: 12, status: "new", age: 13, last: "Just joined" },
      { name: "Elijah Brown", cohort: "Warden Winter '26", mentor: "Danielle Osei", progress: 47, status: "on-track", age: 16, last: "5h ago" },
      { name: "Fatima Ali", cohort: "Warden Winter '26", mentor: "Danielle Osei", progress: 63, status: "on-track", age: 15, last: "Yesterday" },
      { name: "Noah Williams", cohort: "Warden Winter '26", mentor: "Danielle Osei", progress: 29, status: "at-risk", age: 14, last: "8 days ago" },
      { name: "Zara Mensah", cohort: "Warden Winter '26", mentor: "Marcus Bello", progress: 70, status: "on-track", age: 16, last: "4h ago" },
      { name: "Tyrone Clarke", cohort: "St. Clair Spring '26", mentor: "Grace Adeyemi", progress: 5, status: "new", age: 13, last: "Just joined" },
      { name: "Leila Haddad", cohort: "St. Clair Spring '26", mentor: "Danielle Osei", progress: 44, status: "on-track", age: 15, last: "2d ago" }
    ],
    mentors: [
      { name: "Marcus Bello", role: "Lead Mentor", youth: 4 },
      { name: "Grace Adeyemi", role: "Mentor", youth: 4 },
      { name: "Danielle Osei", role: "Mentor", youth: 4 },
      { name: "Shawn Peters", role: "Program Director", youth: 0 }
    ],
    cohorts: [
      { name: "Rexdale Fall '25", term: "Sep–Dec 2025", mentor: "Marcus Bello", size: 5, progress: 68 },
      { name: "Warden Winter '26", term: "Jan–Apr 2026", mentor: "Danielle Osei", size: 5, progress: 41 },
      { name: "St. Clair Spring '26", term: "Apr–Jul 2026", mentor: "Grace Adeyemi", size: 2, progress: 24 }
    ],
    modules: [
      { name: "Who Is Jesus?", lessons: 4, track: "bible" },
      { name: "Identity in Christ", lessons: 5, track: "bible" },
      { name: "Servant Leadership", lessons: 4, track: "bible" },
      { name: "Leading a Small Group", lessons: 6, track: "bible" },
      { name: "Sharing Your Story", lessons: 3, track: "bible" },
      { name: "Intro to Scratch", lessons: 5, track: "coding" },
      { name: "Web Basics: HTML & CSS", lessons: 6, track: "coding" }
    ],
    badges: [
      { name: "First Steps", icon: "star", desc: "Completed your first lesson" },
      { name: "Word Warrior", icon: "book-open", desc: "Finished the book of Mark" },
      { name: "Faithful Five", icon: "zap", desc: "5-day study streak" },
      { name: "Servant Heart", icon: "heart", desc: "Led a group activity" },
      { name: "Storyteller", icon: "mic", desc: "Shared your testimony" },
      { name: "Code Starter", icon: "code", desc: "Built your first program" }
    ],
    announcements: [
      { title: "Fall retreat sign-ups open", by: "Shawn Peters", when: "2 days ago" },
      { title: "New module: Servant Leadership", by: "Grace Adeyemi", when: "5 days ago" }
    ]
  };

  /* ---------------- Navigation config per role ---------------- */
  var NAV = {
    youth: {
      brandHref: "learning-experience/youth/home.html",
      botnav: ["home", "journey", "curriculum", "messages", "profile"],
      items: [
        { key: "home", label: "Home", icon: "home", path: "learning-experience/youth/home.html" },
        { key: "journey", label: "My Journey", icon: "target", path: "learning-experience/youth/journey.html" },
        { key: "curriculum", label: "Curriculum", icon: "book-open", path: "learning-experience/youth/curriculum.html" },
        { key: "badges", label: "Badges", icon: "award", path: "learning-experience/youth/badges.html" },
        { key: "schedule", label: "Schedule", icon: "calendar", path: "engagement/youth/schedule.html" },
        { key: "messages", label: "Messages", icon: "message-square", path: "engagement/youth/messages.html", badge: "2" },
        { key: "announcements", label: "Announcements", icon: "bell", path: "engagement/youth/announcements.html" },
        { key: "profile", label: "Profile", icon: "user", path: "platform-services/youth/profile.html" }
      ]
    },
    mentor: {
      brandHref: "mentoring/mentor/dashboard.html",
      items: [
        { key: "dashboard", label: "Dashboard", icon: "grid", path: "mentoring/mentor/dashboard.html" },
        { key: "my-youth", label: "My Youth", icon: "users", path: "mentoring/mentor/my-youth.html" },
        { key: "check-ins", label: "Check-ins", icon: "clipboard-check", path: "mentoring/mentor/check-in-history.html" },
        { key: "reviews", label: "Reviews", icon: "file-text", path: "mentoring/mentor/reviews.html", badge: "5" },
        { key: "attendance", label: "Attendance", icon: "user-check", path: "engagement/mentor/attendance.html" },
        { key: "flags", label: "At-risk", icon: "flag", path: "mentoring/mentor/flags.html", badge: "2" },
        { group: "Program" },
        { key: "curriculum", label: "Curriculum", icon: "book-open", path: "mentoring/mentor/curriculum.html" },
        { key: "calendar", label: "Calendar", icon: "calendar", path: "engagement/mentor/calendar.html" },
        { key: "messages", label: "Messages", icon: "message-square", path: "engagement/mentor/messages.html" }
      ]
    },
    admin: {
      brandHref: "reporting-and-data/admin/dashboard.html",
      items: [
        { key: "dashboard", label: "Dashboard", icon: "grid", path: "reporting-and-data/admin/dashboard.html" },
        { group: "People" },
        { key: "people", label: "People", icon: "users", path: "program-administration/admin/people.html" },
        { key: "cohorts", label: "Cohorts", icon: "layers", path: "program-administration/admin/cohorts.html" },
        { key: "applications", label: "Applications", icon: "user-plus", path: "application-intake/admin/applications.html", badge: "3" },
        { key: "invitations", label: "Invitations", icon: "mail", path: "authentication/admin/invitations.html" },
        { key: "users-roles", label: "Users & Roles", icon: "shield", path: "authentication/admin/users-roles.html" },
        { group: "Progress" },
        { key: "cohort-progress", label: "Progress Board", icon: "table", path: "program-administration/admin/cohort-progress.html" },
        { key: "reports", label: "Reports", icon: "bar-chart", path: "reporting-and-data/admin/reports.html" },
        { key: "attendance", label: "Attendance", icon: "user-check", path: "engagement/admin/attendance-admin.html" },
        { group: "Curriculum" },
        { key: "tracks", label: "Curriculum", icon: "book-open", path: "curriculum-authoring/admin/tracks.html" },
        { key: "badges", label: "Badges", icon: "award", path: "curriculum-authoring/admin/badge-builder.html" },
        { key: "progression", label: "Progression", icon: "target", path: "curriculum-authoring/admin/progression-editor.html" },
        { group: "Program" },
        { key: "calendar", label: "Calendar", icon: "calendar", path: "engagement/admin/calendar.html" },
        { key: "announcements", label: "Announcements", icon: "bell", path: "engagement/admin/announcements.html" },
        { group: "System" },
        { key: "settings", label: "Settings", icon: "settings", path: "program-administration/admin/settings-org.html" },
        { key: "audit", label: "Audit Log", icon: "activity", path: "consent-and-safeguarding/admin/audit-log.html" }
      ]
    },
    guardian: {
      brandHref: "guardian-portal/guardian/home.html",
      botnav: ["home", "progress", "announcements", "consent"],
      items: [
        { key: "home", label: "Home", icon: "home", path: "guardian-portal/guardian/home.html" },
        { key: "progress", label: "My Youth", icon: "trending-up", path: "guardian-portal/guardian/child-progress.html" },
        { key: "announcements", label: "News", icon: "bell", path: "guardian-portal/guardian/announcements.html" },
        { key: "consent", label: "Consent", icon: "shield", path: "guardian-portal/guardian/consent.html" }
      ]
    },
    volunteer: {
      brandHref: "volunteer-portal/volunteer/home.html",
      botnav: ["home", "attendance", "schedule"],
      items: [
        { key: "home", label: "Today", icon: "home", path: "volunteer-portal/volunteer/home.html" },
        { key: "attendance", label: "Attendance", icon: "user-check", path: "volunteer-portal/volunteer/attendance.html" },
        { key: "schedule", label: "Schedule", icon: "calendar", path: "volunteer-portal/volunteer/schedule.html" }
      ]
    }
  };

  var ROLE_LABEL = { youth: "Youth Leader", mentor: "Mentor", admin: "Director", guardian: "Guardian", volunteer: "Volunteer" };
  var ROLE_USER = {
    youth: "Jayden Alcantara", mentor: "Marcus Bello", admin: "Shawn Peters",
    guardian: "Ruth Alcantara", volunteer: "Chris Tan"
  };

  /* ---------------- Shell ---------------- */
  WordUp.mountShell = function (opts) {
    opts = opts || {};
    var role = opts.role || "youth";
    var active = opts.active || "";
    var track = opts.track || "bible";
    var cfg = NAV[role];
    if (!cfg) return;
    var shell = document.body;
    shell.classList.add("wu-shell", "wu-has-sidenav");
    var hasBot = cfg.botnav && cfg.botnav.length;
    if (hasBot) shell.classList.add("wu-has-botnav");

    // sidenav items
    var sideItems = cfg.items.map(function (it) {
      if (it.group) return '<div class="wu-sidenav__group">' + it.group + "</div>";
      var badge = it.badge ? '<span class="wu-sidenav__badge">' + it.badge + "</span>" : "";
      return '<a class="wu-sidenav__item' + (it.key === active ? " is-active" : "") +
        '" href="' + WordUp.href(it.path) + '">' + icon(it.icon) + "<span>" + it.label + "</span>" + badge + "</a>";
    }).join("");

    // botnav
    var botItems = "";
    if (hasBot) {
      botItems = cfg.botnav.map(function (key) {
        var it = findItem(cfg, key); if (!it) return "";
        return '<a class="wu-botnav__item' + (key === active ? " is-active" : "") +
          '" href="' + WordUp.href(it.path) + '">' + icon(it.icon) + "<span>" + it.label + "</span></a>";
      }).join("");
    }

    var trackName = trackLabel(track);
    var showTrack = (role === "youth" || role === "mentor" || role === "admin");

    var topbar =
      '<header class="wu-topbar">' +
        '<button class="wu-iconbtn wu-topbar__burger" data-nav-toggle aria-label="Menu">' + icon("menu") + "</button>" +
        '<a class="wu-brand" href="' + WordUp.href(cfg.brandHref) + '"><span class="wu-brand__mark">W</span><span class="wu-brand__name">Word Up</span></a>' +
        '<div class="wu-topbar__actions">' +
          (showTrack ? '<button class="wu-trackswitch" data-menu-toggle="wu-trackmenu">' + icon(track === "coding" ? "code" : "book-open") + '<span class="wu-trackswitch__label" data-track-label>' + trackName + "</span>" + icon("chevron-down") + "</button>" : "") +
          '<button class="wu-iconbtn" data-menu-toggle="wu-notifmenu" aria-label="Notifications">' + icon("bell") + '<span class="wu-dot"></span></button>' +
          '<button class="wu-iconbtn" data-menu-toggle="wu-accountmenu" aria-label="Account"><span class="wu-avatar wu-avatar--sm" data-name="' + ROLE_USER[role] + '"></span></button>' +
        "</div>" +
      "</header>";

    var offline =
      '<div class="wu-offline" data-offline-banner>' + icon("wifi-off") +
      '<span>You’re offline — changes will save when you reconnect.</span></div>';

    var scrim = '<div class="wu-navscrim" data-nav-toggle></div>';
    var sidenav = '<nav class="wu-sidenav" aria-label="Main">' + sideItems +
      '<div class="wu-sidenav__foot"><a class="wu-sidenav__item" href="' + WordUp.href("authentication/auth/signed-out.html") + '">' + icon("log-out") + "<span>Sign out</span></a></div></nav>";
    var botnav = hasBot ? '<nav class="wu-botnav" aria-label="Primary">' + botItems + "</nav>" : "";

    // menus (popovers)
    var trackMenu = showTrack ?
      '<div class="wu-menu wu-hide" id="wu-trackmenu" style="top:52px;right:auto">' +
        '<div class="wu-menu__item" data-track-switch="bible">' + icon("book-open") + "Bible &amp; Leadership</div>" +
        '<div class="wu-menu__item" data-track-switch="coding">' + icon("code") + "Coding for Kingdom</div>" +
      "</div>" : "";
    var notifMenu =
      '<div class="wu-menu wu-hide" id="wu-notifmenu" style="top:52px;right:56px;min-width:280px">' +
        '<div class="wu-menu__item"><span>' + icon("check-circle") + "</span><span><b>Quiz graded</b><br><span class=\"wu-muted wu-text-sm\">You scored 8/10 on “Who Is Jesus?”</span></span></div>" +
        '<div class="wu-menu__item"><span>' + icon("award") + '</span><span><b>New badge earned</b><br><span class="wu-muted wu-text-sm">Faithful Five — 5-day streak</span></span></div>' +
        '<div class="wu-menu__sep"></div>' +
        '<a class="wu-menu__item" href="' + WordUp.href(role === "mentor" ? "engagement/mentor/notifications.html" : "engagement/youth/notifications.html") + '">' + icon("bell") + "View all notifications</a>" +
      "</div>";
    var acctMenu =
      '<div class="wu-menu wu-hide" id="wu-accountmenu" style="top:52px;right:12px">' +
        '<div class="wu-menu__item" style="pointer-events:none"><span class="wu-avatar wu-avatar--sm" data-name="' + ROLE_USER[role] + '"></span><span><b>' + ROLE_USER[role] + "</b><br><span class=\"wu-muted wu-text-sm\">" + ROLE_LABEL[role] + "</span></span></div>" +
        '<div class="wu-menu__sep"></div>' +
        '<a class="wu-menu__item" href="' + WordUp.href("platform-services/shared/settings-profile.html") + '">' + icon("user") + "Profile &amp; settings</a>" +
        '<a class="wu-menu__item" href="' + WordUp.href("platform-services/shared/help.html") + '">' + icon("help-circle") + "Help</a>" +
        '<a class="wu-menu__item" href="' + WordUp.href("authentication/auth/switch-user.html") + '">' + icon("refresh") + "Switch user</a>" +
        '<div class="wu-menu__sep"></div>' +
        '<a class="wu-menu__item wu-menu__item--danger" href="' + WordUp.href("authentication/auth/signed-out.html") + '">' + icon("log-out") + "Sign out</a>" +
      "</div>";

    document.body.insertAdjacentHTML("afterbegin",
      offline + topbar + scrim + sidenav + botnav + trackMenu + notifMenu + acctMenu);
  };

  function findItem(cfg, key) {
    for (var i = 0; i < cfg.items.length; i++) { if (cfg.items[i].key === key) return cfg.items[i]; }
    return null;
  }
  function trackLabel(id) {
    var t = WordUp.data.tracks.filter(function (x) { return x.id === id; })[0];
    return t ? t.name : "Bible & Leadership";
  }

  /* ---------------- Avatars ---------------- */
  var AV_BG = ["#c6fb50", "#8adfff", "#ffd700", "#ffb300", "#bae386", "#e4e0d8", "#f05228", "#1d8fb9"];
  function initials(name) {
    var p = (name || "?").trim().split(/\s+/);
    return ((p[0] || "")[0] || "") + ((p[1] || "")[0] || "");
  }
  function hashIdx(s, n) { var h = 0; for (var i = 0; i < s.length; i++) h = (h * 31 + s.charCodeAt(i)) % 100000; return h % n; }
  function paintAvatars(root) {
    (root || document).querySelectorAll(".wu-avatar[data-name]").forEach(function (el) {
      if (el.getAttribute("data-painted")) return;
      var name = el.getAttribute("data-name");
      if (!el.textContent.trim()) el.textContent = initials(name).toUpperCase();
      var bg = AV_BG[hashIdx(name || "x", AV_BG.length)];
      el.style.background = bg;
      // keep ink text for readability on light accents
      el.style.color = "#16160c";
      el.setAttribute("data-painted", "1");
    });
  }
  WordUp.paintAvatars = paintAvatars;

  /* ---------------- Modals ---------------- */
  WordUp.openModal = function (id) {
    var m = document.getElementById(id);
    if (m) { m.classList.add("is-open"); document.body.style.overflow = "hidden"; }
  };
  WordUp.closeModal = function (id) {
    var m = id ? document.getElementById(id) : document.querySelector(".wu-modal.is-open");
    if (m) m.classList.remove("is-open");
    if (!document.querySelector(".wu-modal.is-open")) document.body.style.overflow = "";
  };

  /* ---------------- Toasts ---------------- */
  function toastHost() {
    var h = document.querySelector(".wu-toasts");
    if (!h) { h = document.createElement("div"); h.className = "wu-toasts"; document.body.appendChild(h); }
    return h;
  }
  WordUp.toast = function (msg, opts) {
    opts = opts || {};
    var type = opts.type || "default";
    var ic = { success: "check-circle", error: "x-circle", warning: "alert-triangle", info: "info", "default": "info" }[type] || "info";
    var el = document.createElement("div");
    el.className = "wu-toast wu-toast--" + type;
    el.innerHTML = '<span class="wu-toast__icon">' + icon(ic) + "</span><div class=\"wu-toast__body\">" +
      (opts.title ? '<div class="wu-toast__title">' + opts.title + "</div>" : "") + (msg || "") +
      '</div><button class="wu-toast__close" aria-label="Dismiss">' + icon("x") + "</button>";
    toastHost().appendChild(el);
    var kill = function () { el.style.opacity = "0"; setTimeout(function () { el.remove(); }, 200); };
    el.querySelector(".wu-toast__close").addEventListener("click", kill);
    if (opts.duration !== 0) setTimeout(kill, opts.duration || 3600);
  };

  /* ---------------- Offline / track ---------------- */
  WordUp.setOffline = function (on) {
    var b = document.querySelector("[data-offline-banner]");
    document.body.classList.toggle("is-offline", !!on);
    if (b) b.classList.toggle("is-visible", !!on);
  };
  WordUp.switchTrack = function (id) {
    document.body.setAttribute("data-track", id);
    var lbl = document.querySelector("[data-track-label]");
    if (lbl) lbl.textContent = trackLabel(id);
    WordUp.toast("Switched to " + trackLabel(id), { type: "info" });
    closeMenus();
  };

  /* ---------------- Menus ---------------- */
  function closeMenus() {
    document.querySelectorAll(".wu-menu").forEach(function (m) { m.classList.add("wu-hide"); });
  }
  function toggleMenu(id) {
    var m = document.getElementById(id);
    if (!m) return;
    var wasHidden = m.classList.contains("wu-hide");
    closeMenus();
    if (wasHidden) m.classList.remove("wu-hide");
  }

  /* ---------------- Tabs ---------------- */
  function wireTabs(scope) {
    (scope || document).querySelectorAll(".wu-tabs").forEach(function (tabs) {
      if (tabs.getAttribute("data-wired")) return;
      tabs.setAttribute("data-wired", "1");
      tabs.addEventListener("click", function (e) {
        var t = e.target.closest("[data-tab]"); if (!t) return;
        var name = t.getAttribute("data-tab");
        tabs.querySelectorAll("[data-tab]").forEach(function (x) { x.classList.toggle("is-active", x === t); });
        var container = tabs.closest("[data-tabs-root]") || document;
        container.querySelectorAll("[data-tabpanel]").forEach(function (p) {
          p.classList.toggle("is-active", p.getAttribute("data-tabpanel") === name);
        });
      });
    });
  }
  WordUp.wireTabs = wireTabs;

  /* ---------------- URL state ---------------- */
  function applyUrlState() {
    var params = new URLSearchParams(window.location.search);
    var track = params.get("track");
    if (track) { document.body.setAttribute("data-track", track); var l = document.querySelector("[data-track-label]"); if (l) l.textContent = trackLabel(track); }
    var state = params.get("state");
    if (document.querySelector("[data-when-state]")) {
      var st = state || "default";
      document.querySelectorAll("[data-when-state]").forEach(function (el) {
        var list = el.getAttribute("data-when-state").split(/\s+/);
        el.classList.toggle("wu-hide", list.indexOf(st) < 0);
      });
    }
    if (state === "offline") WordUp.setOffline(true);
    var open = params.get("open");
    if (open) setTimeout(function () { WordUp.openModal(open); }, 60);
  }

  /* ---------------- Global click wiring ---------------- */
  function wireGlobal() {
    document.addEventListener("click", function (e) {
      var t;
      if ((t = e.target.closest("[data-open-modal]"))) { e.preventDefault(); WordUp.openModal(t.getAttribute("data-open-modal")); return; }
      if ((t = e.target.closest("[data-close-modal]"))) { e.preventDefault(); WordUp.closeModal(t.getAttribute("data-close-modal") || null); return; }
      if ((t = e.target.closest(".wu-modal__scrim"))) { WordUp.closeModal(); return; }
      if ((t = e.target.closest("[data-toast]"))) {
        e.preventDefault();
        WordUp.toast(t.getAttribute("data-toast"), { type: t.getAttribute("data-toast-type") || "success", title: t.getAttribute("data-toast-title") || "" });
        var goto = t.getAttribute("data-then");
        if (goto) setTimeout(function () { window.location.href = WordUp.href(goto); }, 700);
        return;
      }
      if ((t = e.target.closest("[data-track-switch]"))) { e.preventDefault(); WordUp.switchTrack(t.getAttribute("data-track-switch")); return; }
      if ((t = e.target.closest("[data-nav-toggle]"))) { e.preventDefault(); document.body.classList.toggle("is-nav-open"); return; }
      if ((t = e.target.closest("[data-toggle-offline]"))) { e.preventDefault(); WordUp.setOffline(!document.body.classList.contains("is-offline")); return; }
      if ((t = e.target.closest("[data-menu-toggle]"))) { e.preventDefault(); e.stopPropagation(); toggleMenu(t.getAttribute("data-menu-toggle")); return; }
      // click outside menus closes them
      if (!e.target.closest(".wu-menu")) closeMenus();
    });
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") { WordUp.closeModal(); closeMenus(); document.body.classList.remove("is-nav-open"); }
    });
  }

  /* ---------------- init ---------------- */
  var didInit = false;
  WordUp.init = function () {
    if (didInit) return; didInit = true;
    buildSprite();
    var body = document.body;
    if (body.classList.contains("wu-shell") || body.hasAttribute("data-role")) {
      body.classList.add("wu-shell");
      WordUp.mountShell({ role: body.getAttribute("data-role"), active: body.getAttribute("data-active"), track: body.getAttribute("data-track") || "bible" });
    }
    paintAvatars();
    wireTabs();
    wireGlobal();
    applyUrlState();
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", WordUp.init);
  } else {
    WordUp.init();
  }
})();
