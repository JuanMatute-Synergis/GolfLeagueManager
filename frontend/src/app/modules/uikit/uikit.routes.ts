import { Routes } from '@angular/router';

export const uikitRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./uikit.component').then(m => m.UikitComponent),
    children: [
      { path: '', redirectTo: 'components', pathMatch: 'full' },
      { 
        path: 'table', 
        loadComponent: () => import('./pages/table/table.component').then(m => m.TableComponent)
      },
      { path: '**', redirectTo: 'errors/404' },
    ],
  },
];
