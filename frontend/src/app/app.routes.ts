import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./modules/layout/layout.routes').then((m) => m.layoutRoutes),
  },
  {
    path: 'auth',
    loadChildren: () => import('./modules/auth/auth.routes').then((m) => m.authRoutes),
  },
  {
    path: 'errors',
    loadChildren: () => import('./modules/error/error.routes').then((m) => m.errorRoutes),
  },
  { path: '**', redirectTo: 'errors/404' },
];
