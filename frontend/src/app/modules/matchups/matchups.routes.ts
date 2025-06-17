import { Routes } from '@angular/router';

export const matchupsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/matchups-dashboard/matchups-dashboard.component').then(m => m.MatchupsDashboardComponent)
  }
];
