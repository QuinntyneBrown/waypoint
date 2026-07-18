import { Routes } from '@angular/router';
import { authGuard, guestGuard } from '@wu/auth';
import { consentGuard } from './guards/consent.guard';
import { unsavedChangesGuard } from './guards/unsaved-changes.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'auth/sign-in' },
  {
    path: 'consent/respond',
    title: 'Guardian consent · Word Up',
    loadComponent: () =>
      import('./states/requested-guardian-consent/requested-guardian-consent').then(
        (module) => module.RequestedGuardianConsentPage,
      ),
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: 'sign-in',
        title: 'Sign in · Word Up',
        loadComponent: () => import('./auth/sign-in/sign-in').then((m) => m.SignIn),
      },
      {
        path: 'forgot-password',
        title: 'Reset password · Word Up',
        loadComponent: () =>
          import('./auth/forgot-password/forgot-password').then((m) => m.ForgotPassword),
      },
      {
        path: 'reset-link-sent',
        title: 'Check your email · Word Up',
        loadComponent: () =>
          import('./auth/reset-link-sent/reset-link-sent').then((m) => m.ResetLinkSent),
      },
      {
        path: 'reset-password',
        title: 'Set a new password · Word Up',
        loadComponent: () =>
          import('./auth/reset-password/reset-password').then((m) => m.ResetPassword),
      },
      {
        path: 'reset-link-invalid',
        title: 'Reset link · Word Up',
        loadComponent: () =>
          import('./auth/reset-link-invalid/reset-link-invalid').then((m) => m.ResetLinkInvalid),
      },
      {
        path: 'reset-success',
        title: 'Password updated · Word Up',
        loadComponent: () =>
          import('./auth/reset-success/reset-success').then((m) => m.ResetSuccess),
      },
      {
        path: 'account-locked',
        title: 'Account locked · Word Up',
        loadComponent: () =>
          import('./states/account-locked/account-locked').then((m) => m.AccountLocked),
      },
      {
        path: 'account-deactivated',
        title: 'Account inactive · Word Up',
        loadComponent: () =>
          import('./states/account-deactivated/account-deactivated').then(
            (m) => m.AccountDeactivated,
          ),
      },
    ],
  },
  {
    path: 'onboarding',
    title: 'Set up your profile · Word Up',
    canActivate: [authGuard],
    loadComponent: () => import('./onboarding/onboarding').then((m) => m.Onboarding),
  },
  {
    path: 'youth',
    canActivate: [authGuard, consentGuard],
    children: [
      {
        path: 'home',
        title: 'Home · Word Up',
        loadComponent: () => import('./features/youth/home/youth-home').then((m) => m.YouthHome),
      },
      {
        path: 'curriculum',
        title: 'Curriculum · Word Up',
        loadComponent: () =>
          import('./features/youth/curriculum/youth-curriculum').then(
            (module) => module.YouthCurriculumPage,
          ),
      },
      {
        path: 'journey',
        title: 'My journey · Word Up',
        loadComponent: () =>
          import('./features/youth/journey/youth-journey').then(
            (module) => module.YouthJourneyPage,
          ),
      },
      {
        path: 'badges',
        title: 'Badges · Word Up',
        loadComponent: () =>
          import('./features/youth/badges/youth-badges').then((module) => module.YouthBadgesPage),
      },
      {
        path: 'progress',
        title: 'My progress · Word Up',
        loadComponent: () =>
          import('./features/youth/progress/youth-progress').then(
            (module) => module.YouthProgressPage,
          ),
      },
      {
        path: 'schedule',
        title: 'My schedule · Word Up',
        loadComponent: () =>
          import('./features/youth/schedule/youth-schedule').then(
            (module) => module.YouthSchedulePage,
          ),
      },
      {
        path: 'messages',
        title: 'Messages · Word Up',
        loadComponent: () =>
          import('./features/youth/messages/youth-messages').then(
            (module) => module.YouthMessagesPage,
          ),
      },
      {
        path: 'coding/:id',
        title: 'Coding lesson · Word Up',
        loadComponent: () =>
          import('./features/youth/coding/youth-coding').then((module) => module.YouthCodingPage),
      },
      {
        path: 'lessons/:id/reflection',
        title: 'Reflection · Word Up',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/youth/reflection/youth-reflection').then(
            (module) => module.YouthReflectionPage,
          ),
      },
      {
        path: 'lessons/:id',
        title: 'Lesson · Word Up',
        loadComponent: () =>
          import('./features/youth/lesson/youth-lesson').then((module) => module.YouthLessonPage),
      },
      {
        path: 'quizzes/:id/attempt',
        title: 'Quiz attempt · Word Up',
        loadComponent: () =>
          import('./features/youth/quiz-attempt/youth-quiz-attempt').then(
            (module) => module.YouthQuizAttemptPage,
          ),
      },
      {
        path: 'quizzes/:id',
        title: 'Quiz · Word Up',
        loadComponent: () =>
          import('./features/youth/quiz-intro/youth-quiz-intro').then(
            (module) => module.YouthQuizIntroPage,
          ),
      },
      {
        path: 'quiz-attempts/:id/result',
        title: 'Quiz result · Word Up',
        loadComponent: () =>
          import('./features/youth/quiz-result/youth-quiz-result').then(
            (module) => module.YouthQuizResultPage,
          ),
      },
      {
        path: 'assignments/:id',
        title: 'Assignment · Word Up',
        loadComponent: () =>
          import('./features/youth/assignment/youth-assignment').then(
            (module) => module.YouthAssignmentPage,
          ),
      },
      {
        path: 'submissions/:id/resubmit',
        title: 'Revise assignment · Word Up',
        loadComponent: () =>
          import('./features/youth/resubmit/youth-resubmit').then(
            (module) => module.YouthResubmitPage,
          ),
      },
      {
        path: 'submissions/:id',
        title: 'Submission status · Word Up',
        loadComponent: () =>
          import('./features/youth/submission-status/youth-submission-status').then(
            (module) => module.YouthSubmissionStatusPage,
          ),
      },
      {
        path: 'announcements',
        title: 'Announcements · Word Up',
        loadComponent: () =>
          import('./features/youth/announcements/youth-announcements').then(
            (m) => m.YouthAnnouncements,
          ),
      },
      {
        path: 'notifications',
        title: 'Notifications · Word Up',
        loadComponent: () =>
          import('./features/youth/notifications/youth-notifications').then(
            (m) => m.YouthNotifications,
          ),
      },
    ],
  },
  {
    path: 'mentor',
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        title: 'Mentor dashboard · Word Up',
        loadComponent: () =>
          import('./features/mentor/dashboard/mentor-dashboard').then(
            (module) => module.MentorDashboardPage,
          ),
      },
      {
        path: 'my-youth',
        title: 'My youth · Word Up',
        loadComponent: () =>
          import('./features/mentor/roster/mentor-roster').then(
            (module) => module.MentorRosterPage,
          ),
      },
      {
        path: 'check-ins',
        title: 'Check-ins · Word Up',
        loadComponent: () =>
          import('./features/mentor/check-ins/mentor-check-ins').then(
            (module) => module.MentorCheckInsPage,
          ),
      },
      {
        path: 'flags',
        title: 'At-risk youth · Word Up',
        loadComponent: () =>
          import('./features/mentor/flags/mentor-flags').then((module) => module.MentorFlagsPage),
      },
      {
        path: 'youth/:id',
        title: 'Youth detail · Word Up',
        loadComponent: () =>
          import('./features/mentor/youth-detail/mentor-youth-detail').then(
            (module) => module.MentorYouthDetailPage,
          ),
      },
      {
        path: 'assign-work',
        title: 'Assign work · Word Up',
        loadComponent: () =>
          import('./features/mentor/assign-work/mentor-assign-work').then(
            (module) => module.MentorAssignWorkPage,
          ),
      },
      {
        path: 'calendar',
        title: 'Calendar · Word Up',
        loadComponent: () =>
          import('./features/mentor/calendar/mentor-calendar').then(
            (module) => module.MentorCalendarPage,
          ),
      },
      {
        path: 'attendance',
        title: 'Attendance · Word Up',
        loadComponent: () =>
          import('./features/mentor/attendance-history/mentor-attendance-history').then(
            (module) => module.MentorAttendanceHistoryPage,
          ),
      },
      {
        path: 'attendance/:sessionId',
        title: 'Take attendance · Word Up',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/mentor/attendance-capture/mentor-attendance-capture').then(
            (module) => module.MentorAttendanceCapturePage,
          ),
      },
      {
        path: 'youth/:id/competencies',
        title: 'Competency tracker · Word Up',
        loadComponent: () =>
          import('./features/mentor/competencies/mentor-competencies').then(
            (module) => module.MentorCompetenciesPage,
          ),
      },
      {
        path: 'reviews',
        title: 'Reviews · Word Up',
        loadComponent: () =>
          import('./features/mentor/reviews/mentor-reviews').then(
            (module) => module.MentorReviewsPage,
          ),
      },
      {
        path: 'reviews/submissions/:id',
        title: 'Review submission · Word Up',
        loadComponent: () =>
          import('./features/mentor/submission-review/mentor-submission-review').then(
            (module) => module.MentorSubmissionReviewPage,
          ),
      },
      {
        path: 'reviews/quizzes/:id',
        title: 'Review quiz assessment · Word Up',
        loadComponent: () =>
          import('./features/mentor/quiz-review/mentor-quiz-review').then(
            (module) => module.MentorQuizReviewPage,
          ),
      },
      {
        path: 'messages',
        title: 'Messages · Word Up',
        loadComponent: () =>
          import('./features/mentor/messages/mentor-messages').then(
            (module) => module.MentorMessagesPage,
          ),
      },
      {
        path: 'notifications',
        title: 'Notifications · Word Up',
        loadComponent: () =>
          import('./features/youth/notifications/youth-notifications').then(
            (module) => module.YouthNotifications,
          ),
      },
    ],
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    children: [
      {
        path: 'people',
        title: 'People · Word Up',
        loadComponent: () =>
          import('./features/admin/people/people-directory').then((m) => m.PeopleDirectory),
      },
      {
        path: 'people/:id',
        title: 'Person record · Word Up',
        loadComponent: () =>
          import('./features/admin/people/person-detail').then((module) => module.PersonDetailPage),
      },
      {
        path: 'applications',
        title: 'Applications · Word Up',
        loadComponent: () =>
          import('./features/admin/applications/applications-list').then((m) => m.ApplicationsList),
      },
      {
        path: 'cohorts',
        title: 'Cohorts · Word Up',
        loadComponent: () =>
          import('./features/admin/cohorts/cohorts-list').then((module) => module.CohortsListPage),
      },
      {
        path: 'cohorts/:id',
        title: 'Cohort detail · Word Up',
        loadComponent: () =>
          import('./features/admin/cohorts/cohort-detail').then(
            (module) => module.CohortDetailPage,
          ),
      },
    ],
  },
  {
    path: 'volunteer',
    canActivate: [authGuard],
    children: [
      {
        path: 'home',
        title: 'Today · Word Up',
        loadComponent: () =>
          import('./features/volunteer/today/volunteer-today').then((m) => m.VolunteerToday),
      },
      {
        path: 'schedule',
        title: 'Schedule · Word Up',
        loadComponent: () =>
          import('./features/volunteer/schedule/volunteer-schedule').then(
            (m) => m.VolunteerSchedule,
          ),
      },
      {
        path: 'attendance/:sessionId',
        title: 'Take attendance · Word Up',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/volunteer/attendance/volunteer-attendance').then(
            (module) => module.VolunteerAttendance,
          ),
      },
    ],
  },
  {
    path: 'guardian',
    canActivate: [authGuard],
    children: [
      {
        path: 'home',
        title: 'Guardian home · Word Up',
        loadComponent: () =>
          import('./features/guardian/home/guardian-home').then((module) => module.GuardianHome),
      },
      {
        path: 'progress',
        title: 'Child progress · Word Up',
        loadComponent: () =>
          import('./features/guardian/progress/guardian-progress').then(
            (module) => module.GuardianProgress,
          ),
      },
      {
        path: 'announcements',
        title: 'Guardian news · Word Up',
        loadComponent: () =>
          import('./features/guardian/announcements/guardian-announcements').then(
            (module) => module.GuardianAnnouncements,
          ),
      },
      {
        path: 'consent',
        title: 'Consent and contact · Word Up',
        loadComponent: () =>
          import('./features/guardian/consent/guardian-consent').then(
            (module) => module.GuardianConsent,
          ),
      },
    ],
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'profile' },
      {
        path: 'profile',
        title: 'Profile · Word Up',
        loadComponent: () =>
          import('./settings/profile/settings-profile').then((m) => m.SettingsProfile),
      },
      {
        path: 'notifications',
        title: 'Notification settings · Word Up',
        loadComponent: () =>
          import('./settings/notifications/settings-notifications').then(
            (m) => m.SettingsNotifications,
          ),
      },
      {
        path: 'display',
        title: 'Display & accessibility · Word Up',
        loadComponent: () =>
          import('./settings/display/settings-display').then((m) => m.SettingsDisplay),
      },
    ],
  },
  {
    path: 'support',
    title: 'Contact support · Word Up',
    canActivate: [authGuard],
    loadComponent: () => import('./support/support').then((m) => m.Support),
  },
  {
    path: 'consent-required',
    title: 'Consent needed · Word Up',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./states/consent-required/consent-required').then((m) => m.ConsentRequired),
  },
  {
    path: 'session-expired',
    title: 'Session expired · Word Up',
    loadComponent: () =>
      import('./states/session-expired/session-expired').then((m) => m.SessionExpired),
  },
  {
    path: 'forbidden',
    title: 'No access · Word Up',
    loadComponent: () => import('./states/forbidden/forbidden').then((m) => m.Forbidden),
  },
  {
    path: 'server-error',
    title: 'Something went wrong · Word Up',
    loadComponent: () => import('./states/server-error/server-error').then((m) => m.ServerError),
  },
  {
    path: 'maintenance',
    title: 'Maintenance · Word Up',
    loadComponent: () => import('./states/maintenance/maintenance').then((m) => m.Maintenance),
  },
  {
    path: 'offline',
    title: 'Offline · Word Up',
    loadComponent: () => import('./states/offline/offline-state').then((m) => m.OfflineState),
  },
  {
    path: '**',
    title: 'Not found · Word Up',
    loadComponent: () => import('./states/not-found/not-found').then((m) => m.NotFound),
  },
];
