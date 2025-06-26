import { Routes } from '@angular/router';

export const leagueSummaryRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/league-summary/league-summary.component').then(m => m.LeagueSummaryComponent)
  }
];
