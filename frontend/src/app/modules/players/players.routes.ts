import { Routes } from '@angular/router';

export const playersRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/players-list/players-list.component').then(m => m.PlayersListComponent)
  }
];
