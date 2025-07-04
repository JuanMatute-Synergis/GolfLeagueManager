import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { adminGuard } from '../../core/guards/admin.guard';

export const layoutRoutes: Routes = [
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../dashboard/dashboard.routes').then((m) => m.dashboardRoutes),
  },
  {
    path: 'scoring',
    canActivate: [authGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../scoring/scoring.routes').then((m) => m.scoringRoutes),
  },
  {
    path: 'settings',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../settings/settings.routes').then((m) => m.settingsRoutes),
  },
  {
    path: 'players',
    canActivate: [authGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../players/players.routes').then((m) => m.playersRoutes),
  },
  {
    path: 'matchups',
    canActivate: [authGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../matchups/matchups.routes').then((m) => m.matchupsRoutes),
  },
  {
    path: 'components',
    canActivate: [authGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../uikit/uikit.routes').then((m) => m.uikitRoutes),
  },
  {
    path: 'league-summary',
    canActivate: [authGuard],
    loadComponent: () => import('./layout.component').then(m => m.LayoutComponent),
    loadChildren: () => import('../league-summary/league-summary.routes').then((m) => m.leagueSummaryRoutes),
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'errors/404' },
];
