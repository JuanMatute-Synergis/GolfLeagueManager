import { Routes } from '@angular/router';

export const errorRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./error.component').then(m => m.ErrorComponent),
    children: [
      { path: '', redirectTo: '404', pathMatch: 'full' },
      { 
        path: '404', 
        loadComponent: () => import('./pages/error404/error404.component').then(m => m.Error404Component)
      },
      { 
        path: '500', 
        loadComponent: () => import('./pages/error500/error500.component').then(m => m.Error500Component)
      },
      { path: '**', redirectTo: 'errors/404' },
    ],
  },
];
