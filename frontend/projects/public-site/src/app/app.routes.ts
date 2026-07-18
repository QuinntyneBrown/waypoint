import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'Word Up — young leaders who love Jesus',
    loadComponent: () => import('./features/home/home').then((m) => m.Home),
  },
  {
    path: 'programs',
    title: 'Programs · Word Up',
    loadComponent: () => import('./features/programs/programs').then((m) => m.Programs),
  },
  {
    path: 'about',
    title: 'About · Word Up',
    loadComponent: () => import('./features/about/about').then((m) => m.About),
  },
  {
    path: 'apply',
    title: 'Apply · Word Up',
    loadComponent: () => import('./features/apply/apply').then((m) => m.Apply),
  },
  {
    path: 'get-involved',
    title: 'Get involved · Word Up',
    loadComponent: () => import('./features/get-involved/get-involved').then((m) => m.GetInvolved),
  },
  {
    path: 'donate',
    title: 'Donate · Word Up',
    loadComponent: () => import('./features/donate/donate').then((m) => m.Donate),
  },
  {
    path: 'contact',
    title: 'Contact · Word Up',
    loadComponent: () => import('./features/contact/contact').then((m) => m.Contact),
  },
  {
    path: 'privacy',
    title: 'Privacy · Word Up',
    loadComponent: () => import('./features/privacy/privacy').then((m) => m.Privacy),
  },
  {
    path: '**',
    title: 'Page not found · Word Up',
    loadComponent: () => import('./features/not-found/not-found').then((m) => m.NotFound),
  },
];
