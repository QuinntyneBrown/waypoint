/*
 * Word Up User Guide — site chrome.
 *
 * THE PAGE REGISTRY (`PAGES`, below) IS THE SINGLE SOURCE OF TRUTH.
 * ------------------------------------------------------------------
 * Sidebar navigation, breadcrumbs, the previous/next pager and the search index
 * are all derived from it. Its array order *is* the reading order of the guide.
 *
 * Adding a page therefore means doing two things:
 *   1. add the HTML file at `<slug>.html`, and
 *   2. add its registry entry here, in the position it should be read.
 *
 * The smoke suite fails if those two ever disagree — in either direction. A
 * registry entry with no file on disk fails, and a file on disk with no registry
 * entry fails too. There is no third place to update.
 *
 * Everything here runs from `file://` with no build step, no bundler, no network
 * request and no inline script (a strict CSP forbids the last one). Links are
 * resolved relative to the current page's depth, derived from its slug, so the
 * same markup works at any nesting level.
 */
(function () {
  "use strict";

  /** Version of the vendored Word Up design system these pages are styled with. */
  var DESIGN_SYSTEM_VERSION = "wu-ui 0.0.1";

  /* ==========================================================================
     Page registry — order is reading order
     ========================================================================== */

  var PAGES = [
    {
      category: "Start here",
      slug: "index",
      title: "Word Up User Guide",
      icon: "home",
      description:
        "What Word Up does, who does what, how this guide is organised, and what this release does and does not do.",
      keywords: [
        "overview",
        "introduction",
        "start",
        "home",
        "roles",
        "lifecycle",
        "reading path",
        "release",
      ],
    },
    {
      category: "Start here",
      slug: "start-here/what-you-need",
      title: "What you need before you start",
      icon: "clipboard-check",
      description:
        "The addresses Word Up runs at, why there is no self-signup, and how the very first administrator account is created.",
      keywords: [
        "requirements",
        "prerequisites",
        "browser",
        "url",
        "port",
        "signup",
        "invitation",
        "seed",
        "wu db seed",
        "age",
        "eligibility",
      ],
    },
    {
      category: "Start here",
      slug: "start-here/first-sign-in",
      title: "Opening Word Up for the first time",
      icon: "log-in",
      description:
        "Accept an invitation, choose a password, complete the three onboarding steps, and clear the guardian consent gate.",
      keywords: [
        "sign in",
        "invitation",
        "accept",
        "password",
        "onboarding",
        "consent",
        "first run",
        "expired",
      ],
    },
    {
      category: "Start here",
      slug: "start-here/the-word-up-window",
      title: "A tour of the Word Up window",
      icon: "grid",
      description:
        "The name of every region of the application frame, from the offline banner down to the toast host.",
      keywords: [
        "tour",
        "anatomy",
        "layout",
        "top bar",
        "sidebar",
        "navigation",
        "bottom nav",
        "account menu",
        "toast",
        "regions",
      ],
    },
    {
      category: "Start here",
      slug: "start-here/end-to-end-walkthrough",
      title: "One complete walkthrough",
      icon: "compass",
      description:
        "A single run through the whole product: application, approval, invitation, lesson, submission, review and guardian view.",
      keywords: [
        "walkthrough",
        "end to end",
        "example",
        "complete",
        "apply",
        "approve",
        "invite",
        "submit",
        "grade",
      ],
    },
    {
      category: "Youth",
      slug: "youth/work-through-a-lesson",
      title: "Work through a lesson",
      icon: "book-open",
      description:
        "Find the next unlocked lesson, read it at a comfortable size, mark it complete, and write the reflection that follows.",
      keywords: [
        "youth",
        "lesson",
        "curriculum",
        "locked",
        "reading size",
        "bookmark",
        "complete",
        "reflection",
        "mood",
        "private",
      ],
    },
    {
      category: "Youth",
      slug: "youth/quizzes-and-assignments",
      title: "Take a quiz and submit an assignment",
      icon: "file-text",
      description:
        "How attempts, autosave and mentor approval work for quizzes, and how to submit, track and revise an assignment.",
      keywords: [
        "youth",
        "quiz",
        "attempt",
        "autosave",
        "pass mark",
        "assignment",
        "submission",
        "resubmit",
        "revision",
        "timeline",
      ],
    },
    {
      category: "Mentor",
      slug: "mentor/review-and-grade",
      title: "Review and grade submitted work",
      icon: "clipboard-check",
      description:
        "Work the review queue: score against a rubric, approve or request a revision, and approve quiz assessments.",
      keywords: [
        "mentor",
        "review",
        "grade",
        "rubric",
        "approve",
        "request revision",
        "draft",
        "quiz approval",
        "override",
        "conflict",
      ],
    },
    {
      category: "Mentor",
      slug: "mentor/roster-check-ins-and-flags",
      title: "Keep track of your youth",
      icon: "users",
      description:
        "Your roster and youth records, logging check-ins with the right privacy, and raising or resolving at-risk flags.",
      keywords: [
        "mentor",
        "roster",
        "my youth",
        "check-in",
        "prayer",
        "private",
        "flag",
        "at risk",
        "resolve",
        "competency",
      ],
    },
    {
      category: "Program administrator",
      slug: "administrator/applications-and-people",
      title: "Applications, invitations and people",
      icon: "user-plus",
      description:
        "Move an applicant from the public form to an enrolled account, then manage roles and the account lifecycle.",
      keywords: [
        "admin",
        "application",
        "approve",
        "decline",
        "consent",
        "invitation",
        "people",
        "role",
        "deactivate",
        "remove",
      ],
    },
    {
      category: "Program administrator",
      slug: "administrator/cohorts-and-curriculum",
      title: "Cohorts, curriculum and settings",
      icon: "layers",
      description:
        "Create cohorts and mentor teams, author and publish curriculum, and set the organisation policy and terms.",
      keywords: [
        "admin",
        "cohort",
        "track",
        "module",
        "lesson",
        "quiz",
        "assignment",
        "publish",
        "badge",
        "progression",
        "term",
        "settings",
      ],
    },
    {
      category: "Program administrator",
      slug: "administrator/attendance-and-reports",
      title: "Attendance, the progress board and reports",
      icon: "bar-chart",
      description:
        "Schedule sessions, capture attendance (including offline), and read the progress board, reports hub and roster import.",
      keywords: [
        "admin",
        "session",
        "calendar",
        "attendance",
        "offline",
        "sync",
        "conflict",
        "progress board",
        "report",
        "export",
        "import",
        "csv",
      ],
    },
    {
      category: "Guardian",
      slug: "guardian/consent-and-progress",
      title: "Give consent and follow progress",
      icon: "shield",
      description:
        "Sign participation consent, choose what media permission you grant, and see the progress shared with you.",
      keywords: [
        "guardian",
        "consent",
        "e-signature",
        "media",
        "photo",
        "withdraw",
        "progress",
        "child",
        "contact",
        "announcements",
      ],
    },
    {
      category: "Volunteer",
      slug: "volunteer/todays-session",
      title: "Run today's session",
      icon: "user-check",
      description:
        "See the session you are assigned to, mark the names-only roster present or absent, and check your schedule.",
      keywords: [
        "volunteer",
        "today",
        "session",
        "roster",
        "attendance",
        "present",
        "absent",
        "schedule",
      ],
    },
    {
      category: "Reference",
      slug: "reference/messages-and-error-codes",
      title: "Messages and error codes",
      icon: "alert-triangle",
      description:
        "Every response code and validation message Word Up can show you, what causes it, and what to do about it.",
      keywords: [
        "error",
        "validation",
        "message",
        "status code",
        "400",
        "401",
        "403",
        "404",
        "409",
        "410",
        "423",
        "429",
        "500",
        "503",
        "lockout",
        "rate limit",
      ],
    },
    {
      category: "Reference",
      slug: "reference/lifecycles-and-evidence",
      title: "Lifecycles and the evidence they leave",
      icon: "activity",
      description:
        "Every state a record can hold, the moves that are legal between them, and the audit evidence each move records.",
      keywords: [
        "lifecycle",
        "state",
        "status",
        "transition",
        "application",
        "invitation",
        "submission",
        "publish",
        "attendance",
        "consent",
        "audit",
      ],
    },
    {
      category: "Reference",
      slug: "reference/roles-and-permissions",
      title: "Roles and permissions",
      icon: "lock",
      description:
        "The five roles, the eight policies that gate the product, and the scoping rules that narrow access further.",
      keywords: [
        "role",
        "permission",
        "policy",
        "youth",
        "mentor",
        "program administrator",
        "guardian",
        "volunteer",
        "matrix",
        "scope",
      ],
    },
    {
      category: "Reference",
      slug: "reference/accessibility-and-display",
      title: "Accessibility and display preferences",
      icon: "eye",
      description:
        "Text size, contrast, motion, underlines and reading font, plus the keyboard and screen-reader behaviour that exists today.",
      keywords: [
        "accessibility",
        "display",
        "text size",
        "contrast",
        "reduce motion",
        "underline",
        "reading font",
        "keyboard",
        "screen reader",
        "focus",
        "wcag",
      ],
    },
    {
      category: "Advanced",
      slug: "advanced/privacy-consent-and-audit",
      title: "Privacy, consent and the audit trail",
      icon: "shield",
      description:
        "Data-rights requests and how they are fulfilled, the consent policy over time, and the append-only audit log.",
      keywords: [
        "privacy",
        "data rights",
        "access",
        "correction",
        "removal",
        "consent withdrawal",
        "retention",
        "audit",
        "integrity",
        "safeguarding",
      ],
    },
    {
      category: "Advanced",
      slug: "advanced/integration-contracts",
      title: "Integration contracts",
      icon: "code",
      description:
        "The wu operational command line, the HTTP surface, realtime events, and the headers every response carries.",
      keywords: [
        "cli",
        "wu",
        "api",
        "http",
        "endpoint",
        "signalr",
        "realtime",
        "hub",
        "notification",
        "correlation id",
        "headers",
        "maintenance",
      ],
    },
    {
      category: "Advanced",
      slug: "advanced/troubleshooting",
      title: "Troubleshooting by symptom",
      icon: "help-circle",
      description:
        "Start from what you are seeing on screen and work back to the cause and the fix.",
      keywords: [
        "troubleshooting",
        "problem",
        "cannot sign in",
        "locked",
        "blank icon",
        "stuck",
        "offline",
        "maintenance",
        "symptom",
        "fix",
      ],
    },
  ];

  /* ==========================================================================
     Inline SVG icon paths
     --------------------------------------------------------------------------
     Drawn inline so the guide never requests an icon font or an image, and never
     depends on an external sprite. (The product's own shell references a sprite
     that is not present in the build, which is why 26 of its icons render blank;
     this guide deliberately does not repeat that pattern.)
     ========================================================================== */

  var ICONS = {
    home: "M3 10.2 12 3l9 7.2V21H3z",
    "book-open":
      "M12 6.5S9.5 4.5 3 4.5V19c6.5 0 9 2 9 2s2.5-2 9-2V4.5c-6.5 0-9 2-9 2Zm0 0V21",
    "file-text": "M14 3H6v18h12V7zm0 0v4h4M9 13h6M9 17h6",
    "clipboard-check": "M9 4h6v3H9zM7 5H5v16h14V5h-2M9 14l2 2 4-4",
    users:
      "M16 20v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 10a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Zm13 10v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8",
    "user-plus":
      "M15 20v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M8.5 10a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7ZM19 8v6M22 11h-6",
    "user-check":
      "M15 20v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M8.5 10a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7ZM17 11.5l2 2 4-4",
    layers: "M12 3 3 8l9 5 9-5zM3 13l9 5 9-5M3 17.5l9 5 9-5",
    "bar-chart": "M7 20v-7M12 20V6M17 20v-4M3 21h18",
    shield: "M12 3 5 6v6c0 4.5 3 7.8 7 9 4-1.2 7-4.5 7-9V6z",
    lock: "M6 11h12v10H6zM9 11V8a3 3 0 0 1 6 0v3",
    "log-in": "M14 3h5v18h-5M10 8l4 4-4 4M3 12h11",
    grid: "M3 3h7v7H3zM14 3h7v7h-7zM3 14h7v7H3zM14 14h7v7h-7z",
    compass: "M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18ZM15.5 8.5l-2 5-5 2 2-5z",
    "alert-triangle": "M12 3 1.5 21h21zM12 10v5M12 18h.01",
    activity: "M3 12h4l3 8 4-16 3 8h4",
    eye: "M1.5 12S5 5.5 12 5.5 22.5 12 22.5 12 19 18.5 12 18.5 1.5 12 1.5 12Zm10.5 3a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z",
    code: "m9 7-6 5 6 5M15 7l6 5-6 5",
    "help-circle":
      "M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18ZM9.5 9.5a2.5 2.5 0 1 1 3.4 2.3c-.6.3-.9.8-.9 1.4v.6M12 17h.01",
    search: "M11 18a7 7 0 1 0 0-14 7 7 0 0 0 0 14Zm5-2 5 5",
    "chevron-right": "m9 5 7 7-7 7",
    "chevron-left": "m15 5-7 7 7 7",
    "arrow-left": "M20 12H4m6-6-6 6 6 6",
    "arrow-right": "M4 12h16m-6-6 6 6-6 6",
    moon: "M20 14.5A8.5 8.5 0 1 1 9.5 4a7 7 0 0 0 10.5 10.5Z",
    zap: "M13 3 4 14h7l-1 7 9-11h-7z",
    x: "m6 6 12 12M18 6 6 18",
    copy: "M9 9h11v11H9zM5 15H4V4h11v1",
    link: "M10 13a4 4 0 0 0 5.7 0l3-3A4 4 0 0 0 13 4.3l-1.6 1.6M14 11a4 4 0 0 0-5.7 0l-3 3A4 4 0 0 0 11 19.7l1.6-1.6",
    menu: "M4 7h16M4 12h16M4 17h16",
  };

  /* ==========================================================================
     Small helpers
     ========================================================================== */

  var root = document.documentElement;
  var STORAGE_THEME = "wu-ug.theme";
  var STORAGE_MOTION = "wu-ug.motion";
  var SVG_NS = "http://www.w3.org/2000/svg";

  function readStored(key) {
    try {
      return window.localStorage.getItem(key);
    } catch (error) {
      return null;
    }
  }

  function writeStored(key, value) {
    try {
      window.localStorage.setItem(key, value);
    } catch (error) {
      /* Private browsing and file:// origins may refuse storage. Preferences
         still apply for this page view; they simply are not remembered. */
    }
  }

  function el(tag, className, text) {
    var node = document.createElement(tag);
    if (className) node.className = className;
    if (text != null) node.textContent = text;
    return node;
  }

  /** Builds an inline SVG icon. Unknown names render nothing rather than a box. */
  function icon(name, className) {
    var svg = document.createElementNS(SVG_NS, "svg");
    svg.setAttribute("class", "wu-icon" + (className ? " " + className : ""));
    svg.setAttribute("viewBox", "0 0 24 24");
    svg.setAttribute("aria-hidden", "true");
    svg.setAttribute("focusable", "false");
    var d = ICONS[name];
    if (d) {
      var path = document.createElementNS(SVG_NS, "path");
      path.setAttribute("d", d);
      path.setAttribute("fill", "none");
      path.setAttribute("stroke", "currentColor");
      path.setAttribute("stroke-width", "1.7");
      path.setAttribute("stroke-linecap", "round");
      path.setAttribute("stroke-linejoin", "round");
      svg.appendChild(path);
    }
    return svg;
  }

  /* --------------------------------------------------------------------------
     Depth-relative link resolution.
     `index` sits at the root, `reference/roles-and-permissions` one level down.
     Every generated href is prefixed with the right number of `../` so the same
     chrome works from any folder and over file://.
     -------------------------------------------------------------------------- */

  var currentSlug = document.body.getAttribute("data-page") || "index";
  var depth = currentSlug.split("/").length - 1;
  var upToRoot = depth === 0 ? "" : new Array(depth + 1).join("../");

  function href(slug) {
    return upToRoot + slug + ".html";
  }

  function indexOfSlug(slug) {
    for (var i = 0; i < PAGES.length; i += 1) {
      if (PAGES[i].slug === slug) return i;
    }
    return -1;
  }

  var currentIndex = indexOfSlug(currentSlug);
  var currentPage = currentIndex === -1 ? null : PAGES[currentIndex];

  /** Categories in registry order, each with its pages. */
  function groupedPages() {
    var groups = [];
    var byName = {};
    for (var i = 0; i < PAGES.length; i += 1) {
      var page = PAGES[i];
      if (!byName[page.category]) {
        byName[page.category] = { category: page.category, pages: [] };
        groups.push(byName[page.category]);
      }
      byName[page.category].pages.push(page);
    }
    return groups;
  }

  /* ==========================================================================
     Display preferences — dark theme and reduced motion
     --------------------------------------------------------------------------
     The Word Up product has no light/dark theme; display preferences there are
     text size, contrast, motion, underlines and reading font, and they are saved
     on the server. These two toggles are guide chrome only. The dark theme is
     built by remapping existing --wu-* tokens to one another, so it introduces
     no new colour values of its own.
     ========================================================================== */

  var prefs = {
    theme: readStored(STORAGE_THEME) === "dark" ? "dark" : "light",
    motion: readStored(STORAGE_MOTION) === "reduced" ? "reduced" : "full",
  };

  function applyPrefs() {
    root.setAttribute("data-ug-theme", prefs.theme);
    root.setAttribute("data-ug-motion", prefs.motion);
    // Reuse the product's own reduced-motion hook from the vendored a11y.css.
    root.classList.toggle("wu-a11y-reduce-motion", prefs.motion === "reduced");
  }

  function describeToggle(button, kind) {
    if (kind === "theme") {
      var dark = prefs.theme === "dark";
      button.setAttribute("aria-pressed", dark ? "true" : "false");
      button.setAttribute(
        "aria-label",
        dark
          ? "Dark theme is on. Switch to the light theme."
          : "Dark theme is off. Switch to the dark theme.",
      );
    } else {
      var reduced = prefs.motion === "reduced";
      button.setAttribute("aria-pressed", reduced ? "true" : "false");
      button.setAttribute(
        "aria-label",
        reduced
          ? "Reduced motion is on. Allow animations and transitions."
          : "Reduced motion is off. Remove animations and transitions.",
      );
    }
  }

  function buildPrefToggle(kind, iconName, label) {
    var button = el("button", "ug-pref");
    button.type = "button";
    button.appendChild(icon(iconName));
    button.appendChild(el("span", "ug-pref__label", label));
    describeToggle(button, kind);
    button.addEventListener("click", function () {
      if (kind === "theme") {
        prefs.theme = prefs.theme === "dark" ? "light" : "dark";
        writeStored(STORAGE_THEME, prefs.theme);
      } else {
        prefs.motion = prefs.motion === "reduced" ? "full" : "reduced";
        writeStored(STORAGE_MOTION, prefs.motion);
      }
      applyPrefs();
      describeToggle(button, kind);
    });
    return button;
  }

  /* ==========================================================================
     Header
     ========================================================================== */

  /** The header's own search control, used as the fallback focus target. */
  var headerSearchButton = null;

  function buildHeader(host) {
    host.className = "ug-header";

    var brand = el("a", "ug-brand");
    brand.href = href("index");
    brand.appendChild(el("span", "ug-brand__mark", "W"));
    var brandText = el("span", "ug-brand__text");
    brandText.appendChild(el("span", "ug-brand__name", "Word Up"));
    brandText.appendChild(el("span", "ug-brand__sub", "User Guide"));
    brand.appendChild(brandText);
    host.appendChild(brand);

    host.appendChild(el("div", "ug-header__spacer"));

    var searchButton = el("button", "ug-searchbtn");
    searchButton.type = "button";
    searchButton.setAttribute(
      "aria-label",
      "Search the guide. Keyboard shortcut: Control K.",
    );
    searchButton.appendChild(icon("search"));
    searchButton.appendChild(el("span", "ug-searchbtn__label", "Search"));
    searchButton.appendChild(el("kbd", "ug-kbd", "Ctrl K"));
    searchButton.addEventListener("click", function () {
      openSearch(searchButton);
    });
    host.appendChild(searchButton);
    headerSearchButton = searchButton;

    host.appendChild(buildPrefToggle("theme", "moon", "Dark"));
    host.appendChild(buildPrefToggle("motion", "zap", "Motion"));
  }

  /* ==========================================================================
     Sidebar navigation
     ========================================================================== */

  function buildNav(host) {
    host.className = "ug-nav";
    host.setAttribute("aria-label", "Guide pages");

    var groups = groupedPages();
    for (var g = 0; g < groups.length; g += 1) {
      var group = groups[g];
      host.appendChild(el("p", "ug-nav__group", group.category));
      var list = el("ul", "ug-nav__list");
      for (var p = 0; p < group.pages.length; p += 1) {
        var page = group.pages[p];
        var item = el("li");
        var link = el("a", "ug-nav__item");
        link.href = href(page.slug);
        link.appendChild(icon(page.icon, "ug-nav__icon"));
        link.appendChild(el("span", null, page.title));
        if (page.slug === currentSlug) {
          link.classList.add("is-active");
          link.setAttribute("aria-current", "page");
        }
        item.appendChild(link);
        list.appendChild(item);
      }
      host.appendChild(list);
    }
  }

  /* ==========================================================================
     Breadcrumbs
     ========================================================================== */

  function buildBreadcrumbs(host) {
    if (!currentPage) return;
    host.className = "ug-crumbs";
    host.setAttribute("aria-label", "Breadcrumb");

    var list = el("ol", "ug-crumbs__list");

    var homeItem = el("li", "ug-crumbs__item");
    var homeLink = el("a", "ug-crumbs__link");
    homeLink.href = href("index");
    homeLink.textContent = "User Guide";
    homeItem.appendChild(homeLink);
    list.appendChild(homeItem);

    var catItem = el("li", "ug-crumbs__item");
    catItem.appendChild(el("span", "ug-crumbs__sep", "/"));
    catItem.appendChild(el("span", "ug-crumbs__text", currentPage.category));
    list.appendChild(catItem);

    var pageItem = el("li", "ug-crumbs__item");
    pageItem.appendChild(el("span", "ug-crumbs__sep", "/"));
    var here = el(
      "span",
      "ug-crumbs__text ug-crumbs__text--current",
      currentPage.title,
    );
    here.setAttribute("aria-current", "page");
    pageItem.appendChild(here);
    list.appendChild(pageItem);

    host.appendChild(list);
  }

  /* ==========================================================================
     On-page contents, plus a "#" anchor beside every identified <h2>
     ========================================================================== */

  function buildToc(host) {
    var sections = document.querySelectorAll("main section[id] > h2");
    if (!sections.length) {
      host.remove();
      return;
    }

    host.className = "ug-toc";
    host.setAttribute("aria-label", "On this page");
    host.appendChild(el("p", "ug-toc__head", "On this page"));

    var list = el("ul", "ug-toc__list");
    for (var i = 0; i < sections.length; i += 1) {
      var heading = sections[i];
      var section = heading.parentNode;

      var item = el("li", "ug-toc__item");
      var link = el("a", "ug-toc__link");
      link.href = "#" + section.id;
      link.textContent = heading.textContent;
      item.appendChild(link);
      list.appendChild(item);

      var anchor = el("a", "ug-anchor", "#");
      anchor.href = "#" + section.id;
      anchor.setAttribute(
        "aria-label",
        "Link to this section: " + heading.textContent,
      );
      heading.appendChild(anchor);
    }
    host.appendChild(list);
  }

  /* ==========================================================================
     Previous / next pager
     ========================================================================== */

  function buildPager(host) {
    if (currentIndex === -1) {
      host.remove();
      return;
    }

    host.className = "ug-pager";
    host.setAttribute("aria-label", "Previous and next page");

    var previous = currentIndex > 0 ? PAGES[currentIndex - 1] : null;
    var next = currentIndex < PAGES.length - 1 ? PAGES[currentIndex + 1] : null;

    if (previous) {
      var back = el("a", "ug-pager__link ug-pager__link--prev");
      back.href = href(previous.slug);
      back.rel = "prev";
      back.appendChild(icon("arrow-left"));
      var backText = el("span", "ug-pager__text");
      backText.appendChild(el("span", "ug-pager__kicker", "Previous"));
      backText.appendChild(el("span", "ug-pager__title", previous.title));
      back.appendChild(backText);
      host.appendChild(back);
    } else {
      host.appendChild(el("span", "ug-pager__spacer"));
    }

    if (next) {
      var forward = el("a", "ug-pager__link ug-pager__link--next");
      forward.href = href(next.slug);
      forward.rel = "next";
      var forwardText = el("span", "ug-pager__text");
      forwardText.appendChild(el("span", "ug-pager__kicker", "Next"));
      forwardText.appendChild(el("span", "ug-pager__title", next.title));
      forward.appendChild(forwardText);
      forward.appendChild(icon("arrow-right"));
      host.appendChild(forward);
    } else {
      host.appendChild(el("span", "ug-pager__spacer"));
    }
  }

  /* ==========================================================================
     Status bar
     ========================================================================== */

  function buildStatus(host) {
    host.className = "ug-status";

    var left = el("div", "ug-status__group");
    left.appendChild(el("span", "ug-status__label", "Word Up User Guide"));
    left.appendChild(el("span", "ug-status__dot"));
    left.appendChild(
      el(
        "span",
        "ug-status__value",
        PAGES.length + (PAGES.length === 1 ? " page" : " pages"),
      ),
    );
    host.appendChild(left);

    var right = el("div", "ug-status__group");
    right.appendChild(el("span", "ug-status__label", "Design system"));
    right.appendChild(
      el(
        "span",
        "ug-status__value ug-status__value--mono",
        DESIGN_SYSTEM_VERSION,
      ),
    );
    host.appendChild(right);
  }

  /* ==========================================================================
     Search dialog — Ctrl+K, focus trapped, Escape restores focus to the opener
     ========================================================================== */

  var search = {
    dialog: null,
    input: null,
    results: null,
    opener: null,
    open: false,
  };

  function matches(page, query) {
    if (!query) return true;
    var haystack = (
      page.title +
      " " +
      page.description +
      " " +
      page.keywords.join(" ")
    ).toLowerCase();
    var terms = query.toLowerCase().split(/\s+/);
    for (var i = 0; i < terms.length; i += 1) {
      if (terms[i] && haystack.indexOf(terms[i]) === -1) return false;
    }
    return true;
  }

  function renderResults(query) {
    search.results.textContent = "";
    var found = 0;
    for (var i = 0; i < PAGES.length; i += 1) {
      var page = PAGES[i];
      if (!matches(page, query)) continue;
      found += 1;

      var item = el("li", "ug-search__item");
      var link = el("a", "ug-search__link");
      link.href = href(page.slug);
      link.appendChild(icon(page.icon, "ug-search__icon"));
      var body = el("span", "ug-search__body");
      body.appendChild(el("span", "ug-search__kicker", page.category));
      body.appendChild(el("span", "ug-search__title", page.title));
      body.appendChild(el("span", "ug-search__desc", page.description));
      link.appendChild(body);
      item.appendChild(link);
      search.results.appendChild(item);
    }

    if (!found) {
      var empty = el("li", "ug-search__empty");
      empty.appendChild(
        el("span", "ug-search__title", "No pages match that search"),
      );
      empty.appendChild(
        el(
          "span",
          "ug-search__desc",
          "Try a shorter word, or a term used on the page you want.",
        ),
      );
      search.results.appendChild(empty);
    }
    search.count.textContent = found + (found === 1 ? " page" : " pages");
  }

  function tabbables() {
    var nodes = search.dialog.querySelectorAll(
      "a[href], button:not(:disabled), input:not(:disabled)",
    );
    var out = [];
    for (var i = 0; i < nodes.length; i += 1) {
      var node = nodes[i];
      if (node.offsetParent !== null || node === document.activeElement)
        out.push(node);
    }
    return out;
  }

  function buildSearch() {
    var overlay = el("div", "ug-search");
    overlay.hidden = true;

    var scrim = el("div", "ug-search__scrim");
    scrim.addEventListener("click", closeSearch);
    overlay.appendChild(scrim);

    var dialog = el("div", "ug-search__panel");
    dialog.setAttribute("role", "dialog");
    dialog.setAttribute("aria-modal", "true");
    dialog.setAttribute("aria-label", "Search the guide");

    var head = el("div", "ug-search__head");
    head.appendChild(icon("search", "ug-search__headicon"));

    var input = el("input", "ug-search__input");
    input.type = "search";
    input.setAttribute(
      "aria-label",
      "Search guide pages by title, description or keyword",
    );
    input.placeholder = "Search the guide…";
    input.addEventListener("input", function () {
      renderResults(input.value.trim());
    });
    head.appendChild(input);

    var close = el("button", "ug-search__close");
    close.type = "button";
    close.setAttribute("aria-label", "Close search");
    close.appendChild(icon("x"));
    close.addEventListener("click", closeSearch);
    head.appendChild(close);
    dialog.appendChild(head);

    var count = el("p", "ug-search__count");
    count.setAttribute("role", "status");
    dialog.appendChild(count);

    var results = el("ul", "ug-search__results");
    dialog.appendChild(results);

    overlay.appendChild(dialog);
    document.body.appendChild(overlay);

    search.overlay = overlay;
    search.dialog = dialog;
    search.input = input;
    search.results = results;
    search.count = count;

    dialog.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        event.preventDefault();
        closeSearch();
        return;
      }
      if (event.key !== "Tab") return;
      var focusable = tabbables();
      if (!focusable.length) return;
      var first = focusable[0];
      var last = focusable[focusable.length - 1];
      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault();
        first.focus();
      }
    });
  }

  function openSearch(opener) {
    if (search.open) return;
    // Escape must return focus to a real control. When the dialog is opened by
    // the keyboard shortcut with nothing focused, `document.activeElement` is
    // <body>, which cannot take focus — fall back to the header search button so
    // the keyboard user lands somewhere usable rather than at the top of the
    // document.
    var candidate = opener || document.activeElement;
    if (!candidate || candidate === document.body || candidate === root) {
      candidate = headerSearchButton;
    }
    search.opener = candidate;
    search.overlay.hidden = false;
    search.open = true;
    document.body.classList.add("ug-search-open");
    search.input.value = "";
    renderResults("");
    search.input.focus();
  }

  function closeSearch() {
    if (!search.open) return;
    search.overlay.hidden = true;
    search.open = false;
    document.body.classList.remove("ug-search-open");
    // Focus must return to whatever opened the dialog.
    if (search.opener && search.opener.isConnected) search.opener.focus();
    search.opener = null;
  }

  /* ==========================================================================
     Copy affordance for command and payload snippets
     ========================================================================== */

  function buildCopyButtons() {
    var blocks = document.querySelectorAll("[data-ug-copy]");
    for (var i = 0; i < blocks.length; i += 1) {
      (function (block) {
        var button = el("button", "ug-copy__btn");
        button.type = "button";
        button.appendChild(icon("copy"));
        button.appendChild(el("span", null, "Copy"));
        var target = block.querySelector("code, pre");
        button.setAttribute("aria-label", "Copy this snippet to the clipboard");
        button.addEventListener("click", function () {
          var text = (target || block).textContent;
          var done = function () {
            showToast("Copied to the clipboard");
          };
          if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(done, function () {
              showToast(
                "Could not copy. Select the text and copy it manually.",
              );
            });
          } else {
            showToast("Could not copy. Select the text and copy it manually.");
          }
        });
        block.appendChild(button);
      })(blocks[i]);
    }
  }

  var toastHost = null;

  function showToast(message) {
    if (!toastHost) {
      toastHost = el("div", "ug-toasts");
      toastHost.setAttribute("role", "status");
      toastHost.setAttribute("aria-live", "polite");
      document.body.appendChild(toastHost);
    }
    var toast = el("div", "ug-toast", message);
    toastHost.appendChild(toast);
    window.setTimeout(function () {
      if (toast.parentNode) toast.parentNode.removeChild(toast);
    }, 2600);
  }

  /* ==========================================================================
     Boot
     ========================================================================== */

  function inject(selector, build) {
    var host = document.querySelector(selector);
    if (host) build(host);
  }

  applyPrefs();
  root.setAttribute("data-ug-design-system", DESIGN_SYSTEM_VERSION);

  inject("[data-ug-header]", buildHeader);
  inject("[data-ug-nav]", buildNav);
  inject("[data-ug-breadcrumbs]", buildBreadcrumbs);
  inject("[data-ug-toc]", buildToc);
  inject("[data-ug-pager]", buildPager);
  inject("[data-ug-status]", buildStatus);

  var pageCount = document.querySelector("[data-ug-pagecount]");
  if (pageCount) pageCount.textContent = String(PAGES.length);

  buildSearch();
  buildCopyButtons();

  document.addEventListener("keydown", function (event) {
    if (
      (event.ctrlKey || event.metaKey) &&
      (event.key === "k" || event.key === "K")
    ) {
      event.preventDefault();
      if (search.open) closeSearch();
      else openSearch(document.activeElement);
    }
  });

  // Exposed for the smoke suite only; nothing on a page reads this.
  window.WordUpGuide = {
    pages: PAGES,
    designSystemVersion: DESIGN_SYSTEM_VERSION,
  };
})();
