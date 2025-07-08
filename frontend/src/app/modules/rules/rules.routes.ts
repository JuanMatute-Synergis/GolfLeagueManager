import { Routes } from '@angular/router';

export const rulesRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/rules.component').then(m => m.RulesComponent)
  }
];
