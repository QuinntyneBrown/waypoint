import { Routes } from '@angular/router';
import { authGuard, guestGuard } from '@waypoint/auth';
import { consentGuard, unsavedChangesGuard } from '@waypoint/core';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'auth/sign-in' },
  {
    path: 'invite',
    title: 'Accept invitation · Waypoint',
    loadComponent: () => import('@waypoint/auth/pages').then((module) => module.InviteAcceptPage),
  },
  {
    path: 'consent/respond',
    title: 'Guardian consent · Waypoint',
    loadComponent: () =>
      import('@waypoint/account').then((module) => module.RequestedGuardianConsentPage),
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: 'invite',
        title: 'Accept invitation · Waypoint',
        loadComponent: () =>
          import('@waypoint/auth/pages').then((module) => module.InviteAcceptPage),
      },
      {
        path: 'sign-in',
        title: 'Sign in · Waypoint',
        loadComponent: () => import('@waypoint/auth/pages').then((m) => m.SignIn),
      },
      {
        path: 'forgot-password',
        title: 'Reset password · Waypoint',
        loadComponent: () => import('@waypoint/auth/pages').then((m) => m.ForgotPassword),
      },
      {
        path: 'reset-link-sent',
        title: 'Check your email · Waypoint',
        loadComponent: () => import('@waypoint/auth/pages').then((m) => m.ResetLinkSent),
      },
      {
        path: 'reset-password',
        title: 'Set a new password · Waypoint',
        loadComponent: () => import('@waypoint/auth/pages').then((m) => m.ResetPassword),
      },
      {
        path: 'reset-link-invalid',
        title: 'Reset link · Waypoint',
        loadComponent: () => import('@waypoint/auth/pages').then((m) => m.ResetLinkInvalid),
      },
      {
        path: 'reset-success',
        title: 'Password updated · Waypoint',
        loadComponent: () => import('@waypoint/auth/pages').then((m) => m.ResetSuccess),
      },
      {
        path: 'account-locked',
        title: 'Account locked · Waypoint',
        loadComponent: () => import('@waypoint/account').then((m) => m.AccountLocked),
      },
      {
        path: 'account-deactivated',
        title: 'Account inactive · Waypoint',
        loadComponent: () => import('@waypoint/account').then((m) => m.AccountDeactivated),
      },
    ],
  },
  {
    path: 'onboarding',
    title: 'Set up your profile · Waypoint',
    canActivate: [authGuard],
    loadComponent: () => import('@waypoint/account').then((m) => m.Onboarding),
  },
  {
    path: 'youth',
    canActivate: [authGuard, consentGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'home',
        title: 'Home · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((m) => m.YouthHome),
      },
      {
        path: 'curriculum',
        title: 'Curriculum · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthCurriculumPage),
      },
      {
        path: 'journey',
        title: 'My journey · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthJourneyPage),
      },
      {
        path: 'badges',
        title: 'Badges · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthBadgesPage),
      },
      {
        path: 'progress',
        title: 'My progress · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthProgressPage),
      },
      {
        path: 'schedule',
        title: 'My schedule · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthSchedulePage),
      },
      {
        path: 'messages',
        title: 'Messages · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthMessagesPage),
      },
      {
        path: 'coding/:id',
        title: 'Coding lesson · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthCodingPage),
      },
      {
        path: 'lessons/:id/reflection',
        title: 'Reflection · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthReflectionPage),
      },
      {
        path: 'lessons/:id',
        title: 'Lesson · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthLessonPage),
      },
      {
        path: 'quizzes/:id/attempt',
        title: 'Quiz attempt · Waypoint',
        loadComponent: () =>
          import('@waypoint/youth').then((module) => module.YouthQuizAttemptPage),
      },
      {
        path: 'quizzes/:id',
        title: 'Quiz · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthQuizIntroPage),
      },
      {
        path: 'quiz-attempts/:id/result',
        title: 'Quiz result · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthQuizResultPage),
      },
      {
        path: 'assignments/:id',
        title: 'Assignment · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthAssignmentPage),
      },
      {
        path: 'submissions/:id/resubmit',
        title: 'Revise assignment · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthResubmitPage),
      },
      {
        path: 'submissions/:id',
        title: 'Submission status · Waypoint',
        loadComponent: () =>
          import('@waypoint/youth').then((module) => module.YouthSubmissionStatusPage),
      },
      {
        path: 'announcements',
        title: 'Announcements · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((m) => m.YouthAnnouncements),
      },
      {
        path: 'notifications',
        title: 'Notifications · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((m) => m.YouthNotifications),
      },
    ],
  },
  {
    path: 'mentor',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'home', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Mentor dashboard · Waypoint',
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorDashboardPage),
      },
      {
        path: 'my-youth',
        title: 'My youth · Waypoint',
        loadComponent: () => import('@waypoint/mentor').then((module) => module.MentorRosterPage),
      },
      {
        path: 'invitations',
        title: 'Invitations · Waypoint',
        loadComponent: () => import('@waypoint/account').then((module) => module.InvitationsPage),
      },
      {
        path: 'check-ins',
        title: 'Check-ins · Waypoint',
        loadComponent: () => import('@waypoint/mentor').then((module) => module.MentorCheckInsPage),
      },
      {
        path: 'flags',
        title: 'At-risk youth · Waypoint',
        loadComponent: () => import('@waypoint/mentor').then((module) => module.MentorFlagsPage),
      },
      {
        path: 'youth/:id',
        title: 'Youth detail · Waypoint',
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorYouthDetailPage),
      },
      {
        path: 'assign-work',
        title: 'Assign work · Waypoint',
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorAssignWorkPage),
      },
      {
        path: 'calendar',
        title: 'Calendar · Waypoint',
        loadComponent: () => import('@waypoint/mentor').then((module) => module.MentorCalendarPage),
      },
      {
        path: 'attendance',
        title: 'Attendance · Waypoint',
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorAttendanceHistoryPage),
      },
      {
        path: 'attendance/:sessionId',
        title: 'Take attendance · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorAttendanceCapturePage),
      },
      {
        path: 'youth/:id/competencies',
        title: 'Competency tracker · Waypoint',
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorCompetenciesPage),
      },
      {
        path: 'reviews',
        title: 'Reviews · Waypoint',
        loadComponent: () => import('@waypoint/mentor').then((module) => module.MentorReviewsPage),
      },
      {
        path: 'reviews/submissions/:id',
        title: 'Review submission · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorSubmissionReviewPage),
      },
      {
        path: 'reviews/quizzes/:id',
        title: 'Review quiz assessment · Waypoint',
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorQuizReviewPage),
      },
      {
        path: 'messages',
        title: 'Messages · Waypoint',
        loadComponent: () => import('@waypoint/mentor').then((module) => module.MentorMessagesPage),
      },
      {
        path: 'notifications',
        title: 'Notifications · Waypoint',
        loadComponent: () => import('@waypoint/youth').then((module) => module.YouthNotifications),
      },
    ],
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'home', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Director dashboard · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.AdminDashboardPage),
      },
      {
        path: 'people',
        title: 'People · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((m) => m.PeopleDirectory),
      },
      {
        path: 'people/:id',
        title: 'Person record · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.PersonDetailPage),
      },
      {
        path: 'applications',
        title: 'Applications · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((m) => m.ApplicationsList),
      },
      {
        path: 'invitations',
        title: 'Invitations · Waypoint',
        loadComponent: () => import('@waypoint/account').then((module) => module.InvitationsPage),
      },
      {
        path: 'users-roles',
        title: 'Users & roles · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.UsersRolesPage),
      },
      {
        path: 'cohorts',
        title: 'Cohorts · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.CohortsListPage),
      },
      {
        path: 'cohorts/:id',
        title: 'Cohort detail · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.CohortDetailPage),
      },
      {
        path: 'cohort-progress',
        title: 'Progress board · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.ProgressBoardPage),
      },
      {
        path: 'reports',
        title: 'Reports hub · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.AdminReportsPage),
      },
      {
        path: 'import',
        title: 'Import roster · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.ImportRosterPage),
      },
      {
        path: 'audit',
        title: 'Audit log · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.AdminAuditLogPage),
      },
      {
        path: 'privacy',
        title: 'Privacy requests · Waypoint',
        loadComponent: () => import('@waypoint/admin').then((module) => module.AdminPrivacyPage),
      },
      { path: 'notifications', pathMatch: 'full', redirectTo: 'announcements' },
      {
        path: 'mentor-assignments',
        title: 'Mentor assignments · Waypoint',
        loadComponent: () =>
          import('@waypoint/admin').then((module) => module.MentorAssignmentsPage),
      },
      {
        path: 'tracks',
        title: 'Curriculum builder · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('@waypoint/admin').then((module) => module.CurriculumBuilderPage),
      },
      {
        path: 'badges',
        title: 'Badge builder · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () => import('@waypoint/admin').then((module) => module.BadgeBuilderPage),
      },
      {
        path: 'progression',
        title: 'Progression framework · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('@waypoint/admin').then((module) => module.ProgressionBuilderPage),
      },
      {
        path: 'calendar',
        title: 'Program calendar · Waypoint',
        loadComponent: () => import('@waypoint/mentor').then((module) => module.MentorCalendarPage),
      },
      {
        path: 'announcements',
        title: 'Announcement administration · Waypoint',
        loadComponent: () =>
          import('@waypoint/admin').then((module) => module.AdminAnnouncementsPage),
      },
      {
        path: 'attendance',
        title: 'Attendance administration · Waypoint',
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorAttendanceHistoryPage),
      },
      {
        path: 'attendance/:sessionId',
        title: 'Take attendance · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('@waypoint/mentor').then((module) => module.MentorAttendanceCapturePage),
      },
      {
        path: 'settings',
        title: 'Organization settings · Waypoint',
        loadComponent: () =>
          import('@waypoint/admin').then((module) => module.OrganizationSettingsPage),
      },
    ],
  },
  {
    path: 'volunteer',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'home',
        title: 'Today · Waypoint',
        loadComponent: () => import('@waypoint/volunteer').then((m) => m.VolunteerToday),
      },
      {
        path: 'schedule',
        title: 'Schedule · Waypoint',
        loadComponent: () => import('@waypoint/volunteer').then((m) => m.VolunteerSchedule),
      },
      { path: 'attendance', pathMatch: 'full', redirectTo: 'home' },
      { path: 'notifications', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'attendance/:sessionId',
        title: 'Take attendance · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('@waypoint/volunteer').then((module) => module.VolunteerAttendance),
      },
    ],
  },
  {
    path: 'guardian',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'home',
        title: 'Guardian home · Waypoint',
        loadComponent: () => import('@waypoint/guardian').then((module) => module.GuardianHome),
      },
      {
        path: 'progress',
        title: 'Child progress · Waypoint',
        loadComponent: () => import('@waypoint/guardian').then((module) => module.GuardianProgress),
      },
      {
        path: 'announcements',
        title: 'Guardian news · Waypoint',
        loadComponent: () =>
          import('@waypoint/guardian').then((module) => module.GuardianAnnouncements),
      },
      {
        path: 'consent',
        title: 'Consent and contact · Waypoint',
        loadComponent: () => import('@waypoint/guardian').then((module) => module.GuardianConsent),
      },
      { path: 'notifications', pathMatch: 'full', redirectTo: 'announcements' },
    ],
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'profile' },
      {
        path: 'profile',
        title: 'Profile · Waypoint',
        loadComponent: () => import('@waypoint/account').then((m) => m.SettingsProfile),
      },
      {
        path: 'notifications',
        title: 'Notification settings · Waypoint',
        loadComponent: () => import('@waypoint/account').then((m) => m.SettingsNotifications),
      },
      {
        path: 'display',
        title: 'Display & accessibility · Waypoint',
        loadComponent: () => import('@waypoint/account').then((m) => m.SettingsDisplay),
      },
      {
        path: 'privacy',
        title: 'Privacy & data rights · Waypoint',
        loadComponent: () =>
          import('@waypoint/account').then((module) => module.SettingsPrivacyPage),
      },
    ],
  },
  {
    path: 'help',
    title: 'Help center · Waypoint',
    canActivate: [authGuard],
    loadComponent: () => import('@waypoint/account').then((module) => module.HelpPage),
  },
  {
    path: 'support',
    title: 'Contact support · Waypoint',
    canActivate: [authGuard],
    loadComponent: () => import('@waypoint/account').then((m) => m.Support),
  },
  {
    path: 'consent-required',
    title: 'Consent needed · Waypoint',
    canActivate: [authGuard],
    loadComponent: () => import('@waypoint/account').then((m) => m.ConsentRequired),
  },
  {
    path: 'session-expired',
    title: 'Session expired · Waypoint',
    loadComponent: () => import('@waypoint/account').then((m) => m.SessionExpired),
  },
  {
    path: 'forbidden',
    title: 'No access · Waypoint',
    loadComponent: () => import('@waypoint/account').then((m) => m.Forbidden),
  },
  {
    path: 'server-error',
    title: 'Something went wrong · Waypoint',
    loadComponent: () => import('@waypoint/account').then((m) => m.ServerError),
  },
  {
    path: 'maintenance',
    title: 'Maintenance · Waypoint',
    loadComponent: () => import('@waypoint/account').then((m) => m.Maintenance),
  },
  {
    path: 'offline',
    title: 'Offline · Waypoint',
    loadComponent: () => import('@waypoint/account').then((m) => m.OfflineState),
  },
  {
    path: '**',
    title: 'Not found · Waypoint',
    loadComponent: () => import('@waypoint/account').then((m) => m.NotFound),
  },
];
