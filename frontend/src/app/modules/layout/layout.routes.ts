import { Routes } from '@angular/router';

export const layoutRoutes: Routes = [
  {
    path: 'dashboard',
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../dashboard/dashboard.routes').then((m) => m.dashboardRoutes),
  },
  {
    path: 'scoring',
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../scoring/scoring.routes').then((m) => m.scoringRoutes),
  },
  {
    path: 'settings',
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../settings/settings.routes').then((m) => m.settingsRoutes),
  },
  {
    path: 'matchups',
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../matchups/matchups.routes').then((m) => m.matchupsRoutes),
  },
  {
    path: 'components',
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../uikit/uikit.routes').then((m) => m.uikitRoutes),
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'errors/404' },
];
