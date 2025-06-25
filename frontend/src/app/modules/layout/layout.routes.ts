import { Routes } from '@angular/router';
import { AuthGuard } from '../../core/guards/auth.guard';

export const layoutRoutes: Routes = [
  {
    path: 'dashboard',
    canActivate: [AuthGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../dashboard/dashboard.routes').then((m) => m.dashboardRoutes),
  },
  {
    path: 'scoring',
    canActivate: [AuthGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../scoring/scoring.routes').then((m) => m.scoringRoutes),
  },
  {
    path: 'settings',
    canActivate: [AuthGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../settings/settings.routes').then((m) => m.settingsRoutes),
  },
  {
    path: 'matchups',
    canActivate: [AuthGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../matchups/matchups.routes').then((m) => m.matchupsRoutes),
  },
  {
    path: 'components',
    canActivate: [AuthGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../uikit/uikit.routes').then((m) => m.uikitRoutes),
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'errors/404' },
];
