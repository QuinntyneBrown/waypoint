import { Routes } from '@angular/router';
import { authGuard, guestGuard } from '@wu/auth';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'auth/sign-in' },
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
    path: 'youth',
    canActivate: [authGuard],
    children: [
      {
        path: 'home',
        title: 'Home · Word Up',
        loadComponent: () => import('./features/youth/home/youth-home').then((m) => m.YouthHome),
      },
    ],
  },
  {
    path: 'session-expired',
    title: 'Session expired · Word Up',
    loadComponent: () =>
      import('./states/session-expired/session-expired').then((m) => m.SessionExpired),
  },
  {
    path: '**',
    title: 'Not found · Word Up',
    loadComponent: () => import('./states/not-found/not-found').then((m) => m.NotFound),
  },
];
