import { Routes } from '@angular/router';

export const matchupsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/matchups-wrapper/matchups-wrapper.component').then(m => m.MatchupsWrapperComponent)
  }
];
