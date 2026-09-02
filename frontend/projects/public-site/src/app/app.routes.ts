import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'Waypoint — young leaders who love Jesus',
    loadComponent: () => import('./features/home/home').then((m) => m.Home),
  },
  {
    path: 'programs',
    title: 'Programs · Waypoint',
    loadComponent: () => import('./features/programs/programs').then((m) => m.Programs),
  },
  {
    path: 'about',
    title: 'About · Waypoint',
    loadComponent: () => import('./features/about/about').then((m) => m.About),
  },
  {
    path: 'apply',
    title: 'Apply · Waypoint',
    loadComponent: () => import('./features/apply/apply').then((m) => m.Apply),
  },
  {
    path: 'get-involved',
    title: 'Get involved · Waypoint',
    loadComponent: () => import('./features/get-involved/get-involved').then((m) => m.GetInvolved),
  },
  {
    path: 'donate',
    title: 'Donate · Waypoint',
    loadComponent: () => import('./features/donate/donate').then((m) => m.Donate),
  },
  {
    path: 'contact',
    title: 'Contact · Waypoint',
    loadComponent: () => import('./features/contact/contact').then((m) => m.Contact),
  },
  {
    path: 'privacy',
    title: 'Privacy · Waypoint',
    loadComponent: () => import('./features/privacy/privacy').then((m) => m.Privacy),
  },
  {
    path: '**',
    title: 'Page not found · Waypoint',
    loadComponent: () => import('./features/not-found/not-found').then((m) => m.NotFound),
  },
];
