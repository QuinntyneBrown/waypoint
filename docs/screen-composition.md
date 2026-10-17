# Screen composition

Binding map from every screen in `docs/mocks/` to the components of `@waypoint/components` it shall be composed of, plus the UI that must be built locally in the application.

- **Sources.** The HTML mocks under `docs/mocks/<subsystem>/<role>/` and the design-system component pages in `docs/design-system/components/`. Component names below are design-system names. Each is implemented as an Angular component in `@waypoint/components` (DECISIONS #23); the selector and API are shown in that component's Storybook story.
- **Library** means a component in `@waypoint/components` (existing or scheduled in DECISIONS #23). **Local** means UI the mocks show that has no library component; it is built in the application feature library, not hand-written with library classes.
- **Excluded.** Mock-only chrome (`mock-bar`, `mock-note`), layout and spacing utilities (`w-stack`, `w-cluster`, `w-grid`, `w-container`, `w-page*`, `w-mb-*`, `w-mt-*`, `w-muted`, `w-eyebrow`, `w-text-*`), which stay shared CSS.
- **States.** Loading, empty, error and similar states live inside each screen mock. A state uses the Skeleton, Empty state or Alert component rather than a local equivalent.
- **Dialogs and notifications.** Dialogs are the Dialog component (with `wuAccessibleDialog` behaviour); toasts are the Toast component and `ToastService`. Screens that contain a dialog list Dialog below.
- **Generated.** Rows are derived from the CSS classes each mock uses, so they are inventory, not design: a row lists every component present in any state of that screen. Regenerate when mocks change.

## Component catalogue and screen counts

| Component | Screens using it | Status |
| --- | ---: | --- |
| Icon | 111 | in library |
| Button | 109 | in library |
| Shell | 101 | in library |
| Card | 90 | in library |
| Badge | 72 | in library |
| Avatar | 60 | in library |
| Form field | 60 | in library |
| Alert | 57 | in library |
| Empty state | 54 | in library |
| Dialog | 48 | in library |
| Icon button | 47 | in library |
| Select | 43 | in library |
| Text field | 43 | in library |
| Skeleton | 41 | in library |
| Textarea | 32 | in library |
| Table | 30 | in library |
| Pagination | — | in library |
| Progress | 30 | in library |
| Stat | 28 | in library |
| Divider | 25 | in library |
| List | 20 | in library |
| Scripture | 19 | in library |
| Button group | 18 | in library |
| Chip | — | in library |
| Checkbox | 18 | in library |
| Switch | 17 | in library |
| Link | — | in library |
| Toast | — | in library |
| Timeline | 15 | in library |
| Message thread | — | in library |
| Tabs | 11 | in library |
| Description list | — | in library |
| Radio group | 10 | in library |
| Footer | 8 | in library |
| Marketing nav | 8 | in library |
| Brand | — | in library |
| Skip link | — | in library |
| Quiz option | 3 | in library |

## Screens

### `application-intake/admin`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Applications · Waypoint (`applications.html`) | Alert, Avatar, Badge, Button, Button group, Card, Dialog, Empty state, Form field, Icon, Icon button, Select, Shell, Skeleton, Stat, Table, Textarea | `w-person` |

### `application-intake/public`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Application submitted · Waypoint (`application-submitted.html`) | Alert, Button, Card, Empty state, Footer, Icon, List, Marketing nav, Shell, Timeline | — |
| Apply / refer a youth · Waypoint (`apply.html`) | Alert, Button, Card, Checkbox, Chip, Divider, Footer, Form field, Icon, Marketing nav, Radio group, Select, Shell, Text field, Textarea | — |

### `authentication/admin`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Invitations · Waypoint (`invitations.html`) | Badge, Button, Card, Dialog, Empty state, Form field, Icon, Icon button, Select, Shell, Skeleton, Stat, Table, Tabs, Textarea | — |
| Users & roles · Waypoint (`users-roles.html`) | Alert, Avatar, Button, Card, Dialog, Divider, Empty state, Form field, Icon, Icon button, Select, Shell, Skeleton, Switch, Table, Text field | `w-avatarstack`, `w-person` |

### `authentication/auth`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Account locked · Waypoint (`account-locked.html`) | Alert, Button, Dialog, Icon | `w-authcard`, `w-authpage` |
| Forgot password · Waypoint (`forgot-password.html`) | Button, Form field, Icon, Shell, Text field | `w-authcard`, `w-authpage` |
| Accept your invite · Waypoint (`invite-accept.html`) | Alert, Avatar, Button, Checkbox, Form field, Icon, Shell, Text field | `w-authcard`, `w-authpage`, `w-person` |
| Sign in · Waypoint (`login.html`) | Button, Checkbox, Divider, Form field, Icon, Shell, Text field | `w-authcard`, `w-authpage` |
| Check your email · Waypoint (`reset-link-sent.html`) | Button, Dialog, Divider, Icon, Shell | `w-authcard`, `w-authpage` |
| Set a new password · Waypoint (`reset-password.html`) | Button, Form field, Icon, Shell, Text field | `w-authcard`, `w-authpage`, `w-inputgroup` |
| Password updated · Waypoint (`reset-success.html`) | Button, Dialog, Icon, Shell | `w-authcard`, `w-authpage` |
| Signed out · Waypoint (`signed-out.html`) | Alert, Button, Dialog, Icon, Shell | `w-authcard`, `w-authpage` |
| Welcome to Waypoint · Waypoint (`welcome-setup.html`) | Avatar, Button, Button group, Card, Form field, Icon, Select, Shell, Switch, Text field | `w-authcard`, `w-authpage`, `w-person` |

### `consent-and-safeguarding/admin`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Audit log · Waypoint (`audit-log.html`) | Avatar, Badge, Button, Card, Empty state, Form field, Icon, Select, Shell, Skeleton, Table, Text field | — |

### `consent-and-safeguarding/auth`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Guardian consent · Waypoint (`consent.html`) | Alert, Avatar, Button, Checkbox, Divider, Form field, Icon, Select, Shell, Text field | `w-authcard`, `w-authpage`, `w-person` |

### `curriculum-authoring/admin`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Assignment builder · Waypoint (`assignment-builder.html`) | Avatar, Badge, Button, Card, Checkbox, Divider, Form field, Icon, Icon button, Select, Shell, Switch, Table, Text field, Textarea | `w-person` |
| Badges · Waypoint (`badge-builder.html`) | Alert, Badge, Button, Card, Dialog, Empty state, Form field, Icon, Icon button, Radio group, Select, Shell, Skeleton, Stat, Text field, Textarea | `w-badgeitem` |
| Lesson editor · Waypoint (`lesson-editor.html`) | Alert, Avatar, Badge, Button, Button group, Card, Dialog, Divider, Form field, Icon, Icon button, List, Scripture, Shell, Text field, Textarea | `w-lessonstep`, `w-person` |
| Module editor · Waypoint (`module-editor.html`) | Alert, Badge, Button, Card, Dialog, Divider, Form field, Icon, Icon button, List, Scripture, Select, Shell, Switch, Text field, Textarea | `w-lessonstep` |
| Progression framework · Waypoint (`progression-editor.html`) | Avatar, Badge, Button, Card, Dialog, Divider, Form field, Icon, Icon button, List, Select, Shell, Text field, Timeline | `w-lessonstep`, `w-person` |
| Quiz builder · Waypoint (`quiz-builder.html`) | Badge, Button, Card, Dialog, Divider, Empty state, Form field, Icon, Icon button, List, Progress, Quiz option, Radio group, Select, Shell, Switch, Text field, Textarea | — |
| Curriculum builder · Waypoint (`tracks.html`) | Avatar, Badge, Button, Card, Chip, Dialog, Empty state, Form field, Icon, Icon button, List, Select, Shell, Skeleton, Stat, Switch, Text field, Textarea | `w-lessonstep` |

### `engagement/admin`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Announcements · Waypoint (`announcements.html`) | Alert, Badge, Button, Button group, Card, Dialog, Empty state, Form field, Icon, Icon button, Progress, Radio group, Select, Shell, Skeleton, Stat, Text field, Textarea | — |
| Attendance · Waypoint (`attendance-admin.html`) | Alert, Avatar, Badge, Button, Card, Dialog, Empty state, Form field, Icon, Icon button, Progress, Select, Shell, Skeleton, Stat, Table, Text field | — |
| Calendar · Waypoint (`calendar.html`) | Avatar, Badge, Button, Button group, Card, Chip, Dialog, Empty state, Form field, Icon, Icon button, Select, Shell, Skeleton, Stat, Switch, Text field | `w-avatarstack`, `w-person` |

### `engagement/mentor`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Attendance history · Waypoint (`attendance-history.html`) | Avatar, Badge, Button, Button group, Card, Empty state, Form field, Icon, Progress, Select, Shell, Skeleton, Stat, Table | — |
| Take attendance · Waypoint (`attendance.html`) | Alert, Avatar, Badge, Button, Button group, Card, Dialog, Empty state, Form field, Icon, Icon button, Select, Shell, Skeleton, Table, Textarea | `w-person` |
| Calendar · Waypoint (`calendar.html`) | Avatar, Badge, Button, Button group, Card, Checkbox, Dialog, Empty state, Form field, Icon, Icon button, Select, Shell, Skeleton, Text field | `w-avatarstack`, `w-person` |
| Messages · Waypoint (`messages.html`) | Alert, Avatar, Badge, Button, Card, Dialog, Empty state, Form field, Icon, Icon button, List, Shell, Skeleton, Text field, Textarea | `w-person` |
| Notifications · Waypoint (`notifications.html`) | Avatar, Badge, Button, Card, Chip, Empty state, Icon, List, Shell, Skeleton | — |

### `engagement/youth`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Announcements · Waypoint (`announcements.html`) | Avatar, Badge, Button, Card, Empty state, Icon, Shell, Skeleton | `ann-card`, `ann-dot`, `w-person` |
| Messages · Waypoint (`messages.html`) | Avatar, Badge, Button, Card, Empty state, Icon, Shell, Skeleton, Textarea | `msg-bubble`, `msg-compose`, `msg-daysep`, `msg-log`, `msg-thread`, `w-person` |
| Notifications · Waypoint (`notifications.html`) | Badge, Button, Card, Empty state, Icon, Shell, Skeleton | `notif` |
| Schedule · Waypoint (`schedule.html`) | Badge, Button, Button group, Card, Dialog, Empty state, Form field, Icon, Icon button, Radio group, Shell, Skeleton, Textarea | — |

### `guardian-portal/guardian`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| News · Waypoint (`announcements.html`) | Avatar, Badge, Button, Card, Empty state, Icon, Shell, Skeleton | `ann-card`, `ann-dot`, `w-person` |
| My youth · Waypoint (`child-progress.html`) | Avatar, Badge, Card, Empty state, Icon, List, Progress, Shell, Skeleton, Stat, Table, Timeline | `w-badgeitem`, `w-person` |
| Consent & profile · Waypoint (`consent.html`) | Alert, Badge, Button, Card, Checkbox, Dialog, Form field, Icon, Icon button, Select, Shell, Switch, Table, Text field | — |
| Guardian home · Waypoint (`home.html`) | Alert, Avatar, Badge, Button, Card, Icon, Progress, Scripture, Shell | `w-person` |

### `learning-experience/youth`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Assignment · Waypoint (`assignment.html`) | Alert, Badge, Button, Card, Icon, Scripture, Shell, Table | — |
| Badges · Waypoint (`badges.html`) | Badge, Button, Card, Empty state, Icon, Progress, Shell, Skeleton, Timeline | `w-badgeitem` |
| Coding lesson · Waypoint (`coding-lesson.html`) | Badge, Button, Card, Checkbox, Icon, Progress, Shell | `cl-code`, `cl-try`, `w-lessonstep` |
| Coding project · Waypoint (`coding-project.html`) | Alert, Avatar, Badge, Button, Card, Checkbox, Dialog, Empty state, Form field, Icon, Icon button, Shell, Skeleton, Textarea | `cp-gallery`, `cp-preview`, `cp-tile` |
| Curriculum · Waypoint (`curriculum.html`) | Badge, Button, Button group, Card, Empty state, Icon, Progress, Shell, Skeleton | `w-badgeitem` |
| Home · Waypoint (`home.html`) | Avatar, Badge, Button, Card, Dialog, Icon, Icon button, List, Progress, Radio group, Scripture, Shell | `w-badgeitem`, `w-person` |
| My Journey · Waypoint (`journey.html`) | Badge, Button, Card, Icon, List, Progress, Shell, Timeline | `w-badgeitem` |
| Bible lesson · Waypoint (`lesson-bible.html`) | Alert, Badge, Button, Card, Dialog, Icon, Icon button, Progress, Scripture, Shell, Skeleton | `lb-actions`, `lb-read`, `lb-sizectl`, `lb-topbar`, `lb-video` |
| Lesson overview · Waypoint (`lesson.html`) | Badge, Button, Card, Icon, List, Scripture, Shell | `w-lessonstep` |
| Module · Waypoint (`module.html`) | Badge, Button, Card, Icon, List, Progress, Shell | `w-lessonstep` |
| My progress · Waypoint (`progress.html`) | Badge, Card, Icon, List, Progress, Shell, Stat, Table, Tabs | — |
| Quiz intro · Waypoint (`quiz-intro.html`) | Alert, Button, Card, Icon, Shell | `qi-fact`, `qi-facts` |
| Quiz result · Waypoint (`quiz-result.html`) | Badge, Button, Card, Icon, Progress, Quiz option, Shell | `qr-actions`, `qr-hero`, `qr-review`, `w-badgeitem` |
| Quiz · Waypoint (`quiz.html`) | Alert, Button, Dialog, Icon, Icon button, Progress, Quiz option, Shell | `qz-actions`, `qz-topbar` |
| Reflection · Waypoint (`reflection.html`) | Alert, Button, Card, Form field, Icon, Shell, Switch, Textarea | `rf-actions`, `rf-mood` |
| Submission status · Waypoint (`submission-status.html`) | Alert, Avatar, Badge, Button, Card, Icon, Shell, Timeline | `w-person` |
| Submit work · Waypoint (`submit-work.html`) | Alert, Button, Checkbox, Dialog, Form field, Icon, Icon button, Shell, Textarea | `sw-drop` |

### `mentoring/mentor`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Assessment approvals · Waypoint (`assessment-approvals.html`) | Alert, Avatar, Badge, Button, Card, Empty state, Icon, Shell, Skeleton, Table, Text field | — |
| Assign work · Waypoint (`assign-work.html`) | Alert, Avatar, Badge, Button, Card, Checkbox, Dialog, Divider, Form field, Icon, Icon button, Select, Shell, Text field, Textarea | `w-avatarstack`, `w-person` |
| Check-ins · Waypoint (`check-in-history.html`) | Avatar, Badge, Button, Button group, Card, Chip, Dialog, Empty state, Form field, Icon, Icon button, Select, Shell, Skeleton, Switch, Textarea, Timeline | `w-person` |
| Log check-in · Waypoint (`check-in.html`) | Alert, Button, Button group, Card, Divider, Form field, Icon, Select, Shell, Switch, Text field, Textarea | — |
| Competency tracker · Waypoint (`competency.html`) | Alert, Avatar, Badge, Button, Button group, Card, Form field, Icon, Shell, Textarea | `comp-bar`, `comp-item`, `comp-note`, `comp-seg`, `w-person` |
| Curriculum · Waypoint (`curriculum.html`) | Alert, Badge, Button, Button group, Card, Empty state, Icon, Scripture, Shell, Skeleton, Stat, Timeline | `w-person` |
| Mentor dashboard · Waypoint (`dashboard.html`) | Avatar, Badge, Button, Card, Dialog, Form field, Icon, Icon button, Progress, Select, Shell, Stat, Textarea | `dash-date`, `dash-main`, `dash-review`, `dash-session`, `dash-youth`, `w-person` |
| At-risk · Waypoint (`flags.html`) | Alert, Avatar, Badge, Button, Button group, Card, Chip, Dialog, Empty state, Form field, Icon, Icon button, Progress, Select, Shell, Skeleton, Textarea | `w-person` |
| Grade submission · Waypoint (`grade-submission.html`) | Avatar, Badge, Button, Card, Checkbox, Dialog, Divider, Empty state, Form field, Icon, Icon button, Scripture, Shell, Text field, Textarea | `w-person` |
| My group · Waypoint (`group.html`) | Alert, Avatar, Badge, Button, Card, Chip, Divider, Empty state, Icon, Progress, Scripture, Shell, Skeleton, Stat, Table, Timeline | `w-person` |
| My youth · Waypoint (`my-youth.html`) | Avatar, Badge, Button, Card, Chip, Dialog, Empty state, Form field, Icon, Icon button, Progress, Select, Shell, Skeleton, Table, Text field, Textarea | — |
| Reviews · Waypoint (`reviews.html`) | Avatar, Badge, Button, Card, Empty state, Icon, Shell, Skeleton, Table, Tabs | — |
| Youth detail · Waypoint (`youth-detail.html`) | Alert, Avatar, Badge, Button, Card, Checkbox, Dialog, Form field, Icon, Icon button, Progress, Scripture, Select, Shell, Stat, Table, Tabs, Textarea, Timeline | `w-lessonstep`, `yd-activity`, `yd-comp`, `yd-head`, `yd-overview`, `yd-ringlabel`, `yd-rings` |

### `platform-services/admin`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Profile & settings · Waypoint (`profile.html`) | Avatar, Badge, Button, Card, Dialog, Form field, Icon, Icon button, List, Shell, Switch, Text field | `w-person` |

### `platform-services/mentor`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Profile & settings · Waypoint (`profile.html`) | Avatar, Badge, Button, Card, Dialog, Divider, Form field, Icon, Icon button, List, Shell, Switch, Text field | — |

### `platform-services/shared`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| About Waypoint · Waypoint (`about.html`) | Avatar, Badge, Card, Icon, Shell | `about-mark`, `legal-link`, `meta-row`, `w-person` |
| Help center · Waypoint (`help.html`) | Button, Card, Icon, Shell, Text field | `faq`, `help-cat`, `help-search` |
| Display & accessibility (dark theme) · Waypoint (`settings-display-dark.html`) | Alert, Button, Button group, Card, Divider, Form field, Icon, Scripture, Select, Shell, Switch, Tabs | `disp-preview`, `set-tabs` |
| Display & accessibility · Waypoint (`settings-display.html`) | Alert, Button, Button group, Card, Divider, Form field, Icon, Scripture, Select, Shell, Switch, Tabs | `disp-preview`, `set-tabs` |
| Notification settings · Waypoint (`settings-notifications.html`) | Button, Card, Form field, Shell, Switch, Tabs, Text field | `notif-col`, `notif-head`, `notif-row`, `set-tabs` |
| Profile settings · Waypoint (`settings-profile.html`) | Avatar, Button, Card, Dialog, Form field, Icon, Icon button, Select, Shell, Tabs, Text field | `set-avatar`, `set-tabs` |
| Contact support · Waypoint (`support.html`) | Alert, Button, Card, Form field, Icon, Select, Shell, Text field, Textarea | `up-drop` |

### `platform-services/states`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| No access · Waypoint (`403.html`) | Alert, Button, Empty state, Icon | `w-authpage` |
| Page not found · Waypoint (`404.html`) | Button, Empty state, Icon | `w-authpage` |
| Something went wrong · Waypoint (`500.html`) | Button, Empty state, Icon | `w-authpage` |
| Maintenance · Waypoint (`503-maintenance.html`) | Button, Empty state, Icon, Scripture | `w-authpage` |
| Account deactivated · Waypoint (`account-deactivated.html`) | Avatar, Button, Empty state, Icon | `w-authpage`, `w-person` |
| Consent required · Waypoint (`consent-required.html`) | Alert, Button, Dialog, Empty state, Form field, Icon, Icon button, Text field | `w-authpage` |
| Invite expired · Waypoint (`invite-expired.html`) | Avatar, Button, Dialog, Empty state, Form field, Icon, Icon button, Text field, Textarea | `w-authpage`, `w-person` |
| Not in a cohort yet · Waypoint (`no-cohort.html`) | Alert, Button, Empty state, Icon | `w-authpage` |
| Offline · Waypoint (`offline.html`) | Alert, Button, Empty state, Icon | `w-authpage` |
| Session expired · Waypoint (`session-expired.html`) | Alert, Button, Empty state, Icon | `w-authpage` |

### `platform-services/youth`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Help · Waypoint (`help.html`) | Alert, Button, Card, Icon, Shell | `faq` |
| Profile · Waypoint (`profile.html`) | Avatar, Badge, Button, Card, Dialog, Icon, Icon button, Scripture, Shell, Stat | `prof-hero`, `prof-link` |

### `program-administration/admin`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Cohort detail · Waypoint (`cohort-detail.html`) | Alert, Avatar, Badge, Button, Card, Checkbox, Dialog, Divider, Form field, Icon, Icon button, Progress, Select, Shell, Stat, Table, Tabs, Text field, Timeline | `w-person` |
| Progress board · Waypoint (`cohort-progress.html`) | Alert, Button, Card, Checkbox, Dialog, Empty state, Form field, Icon, Icon button, Radio group, Select, Shell, Skeleton, Stat, Table | — |
| Cohorts · Waypoint (`cohorts.html`) | Alert, Avatar, Badge, Button, Card, Chip, Dialog, Empty state, Form field, Icon, Icon button, Progress, Select, Shell, Skeleton, Stat, Text field | `w-person` |
| People · Waypoint (`people.html`) | Alert, Avatar, Badge, Button, Card, Chip, Dialog, Empty state, Form field, Icon, Icon button, Select, Shell, Skeleton, Table, Text field | — |
| Person detail · Waypoint (`person-detail.html`) | Alert, Avatar, Badge, Button, Card, Dialog, Divider, Form field, Icon, Icon button, List, Progress, Scripture, Select, Shell, Stat, Table, Tabs, Text field, Textarea, Timeline | `w-badgeitem`, `w-person` |
| Organization settings · Waypoint (`settings-org.html`) | Alert, Avatar, Badge, Button, Card, Form field, Icon, Shell, Switch, Table, Tabs, Text field, Textarea | `w-person` |

### `public-site/public`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| About · Waypoint (`about.html`) | Avatar, Badge, Button, Card, Footer, Icon, List, Marketing nav, Shell, Table, Timeline | `w-hero`, `w-person` |
| Contact · Waypoint (`contact.html`) | Button, Card, Divider, Empty state, Footer, Form field, Icon, Marketing nav, Select, Shell, Text field, Textarea | — |
| Get involved · Waypoint (`get-involved.html`) | Alert, Button, Card, Checkbox, Chip, Dialog, Divider, Footer, Form field, Icon, Icon button, Marketing nav, Select, Shell, Stat, Text field, Textarea | `w-hero` |
| Waypoint (`landing.html`) | Avatar, Button, Card, Footer, Icon, Marketing nav, Scripture, Shell, Stat | `w-hero`, `w-person` |
| Privacy & consent · Waypoint (`privacy.html`) | Alert, Button, Divider, Footer, Icon, Marketing nav, Shell | — |
| Programs · Waypoint (`programs.html`) | Avatar, Badge, Button, Card, Chip, Divider, Footer, Icon, List, Marketing nav, Scripture, Shell | `w-hero`, `w-person` |

### `reporting-and-data/admin`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Director dashboard · Waypoint (`dashboard.html`) | Alert, Avatar, Badge, Button, Card, Divider, Icon, Icon button, List, Progress, Shell, Stat, Timeline | `w-person` |
| Import from Google Sheets · Waypoint (`data-import.html`) | Alert, Avatar, Badge, Button, Card, Icon, Icon button, Select, Shell, Table | `w-person` |
| Attendance report · Waypoint (`report-attendance.html`) | Badge, Button, Card, Dialog, Empty state, Form field, Icon, Icon button, Progress, Radio group, Select, Shell, Skeleton, Stat, Table, Text field | — |
| Cohort report · Waypoint (`report-cohort.html`) | Avatar, Badge, Button, Card, Checkbox, Dialog, Empty state, Form field, Icon, Icon button, Progress, Radio group, Select, Shell, Skeleton, Stat, Table | — |
| Staff activity report · Waypoint (`report-staff.html`) | Alert, Avatar, Badge, Button, Card, Empty state, Form field, Icon, Progress, Select, Shell, Skeleton, Stat, Table | — |
| Individual youth report · Waypoint (`report-youth.html`) | Alert, Avatar, Badge, Button, Card, Empty state, Icon, Progress, Scripture, Shell, Skeleton, Stat, Table | `w-badgeitem`, `w-person` |
| Reports · Waypoint (`reports.html`) | Alert, Avatar, Badge, Button, Card, Checkbox, Dialog, Divider, Empty state, Form field, Icon, Icon button, Progress, Radio group, Select, Shell, Skeleton, Stat | `w-person` |

### `volunteer-portal/volunteer`

| Screen (mock) | Library | Local UI |
| --- | --- | --- |
| Take attendance · Waypoint (`attendance.html`) | Alert, Avatar, Badge, Button, Button group, Card, Empty state, Icon, Shell, Skeleton, Table | `w-person` |
| Volunteer today · Waypoint (`home.html`) | Alert, Avatar, Badge, Button, Card, Icon, Shell, Timeline | `w-person` |
| Schedule & profile · Waypoint (`schedule.html`) | Avatar, Badge, Button, Card, Empty state, Icon, Shell, Skeleton, Stat, Table | `w-person` |

## Local UI to build

Classes the mocks use that no library component covers, with the number of screens using each. Build once in the owning feature library and reuse; if one is needed in three or more screens, propose it for the library through DECISIONS #23 change control instead of copying it.

| Local UI | Screens |
| --- | ---: |
| `w-person` | 45 |
| `w-authpage` | 20 |
| `w-authcard` | 10 |
| `w-badgeitem` | 9 |
| `w-lessonstep` | 8 |
| `set-tabs` | 4 |
| `w-avatarstack` | 4 |
| `w-hero` | 4 |
| `ann-card` | 2 |
| `ann-dot` | 2 |
| `disp-preview` | 2 |
| `faq` | 2 |
| `about-mark` | 1 |
| `cl-code` | 1 |
| `cl-try` | 1 |
| `comp-bar` | 1 |
| `comp-item` | 1 |
| `comp-note` | 1 |
| `comp-seg` | 1 |
| `cp-gallery` | 1 |
| `cp-preview` | 1 |
| `cp-tile` | 1 |
| `dash-date` | 1 |
| `dash-main` | 1 |
| `dash-review` | 1 |
| `dash-session` | 1 |
| `dash-youth` | 1 |
| `help-cat` | 1 |
| `help-search` | 1 |
| `lb-actions` | 1 |
| `lb-read` | 1 |
| `lb-sizectl` | 1 |
| `lb-topbar` | 1 |
| `lb-video` | 1 |
| `legal-link` | 1 |
| `meta-row` | 1 |
| `msg-bubble` | 1 |
| `msg-compose` | 1 |
| `msg-daysep` | 1 |
| `msg-log` | 1 |
| `msg-thread` | 1 |
| `notif` | 1 |
| `notif-col` | 1 |
| `notif-head` | 1 |
| `notif-row` | 1 |
| `prof-hero` | 1 |
| `prof-link` | 1 |
| `qi-fact` | 1 |
| `qi-facts` | 1 |
| `qr-actions` | 1 |
| `qr-hero` | 1 |
| `qr-review` | 1 |
| `qz-actions` | 1 |
| `qz-topbar` | 1 |
| `rf-actions` | 1 |
| `rf-mood` | 1 |
| `set-avatar` | 1 |
| `sw-drop` | 1 |
| `up-drop` | 1 |
| `w-inputgroup` | 1 |
| `yd-activity` | 1 |
| `yd-comp` | 1 |
| `yd-head` | 1 |
| `yd-overview` | 1 |
| `yd-ringlabel` | 1 |
| `yd-rings` | 1 |
