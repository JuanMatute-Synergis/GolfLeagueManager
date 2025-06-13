import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard.component';
import { DashboardHomeComponent } from './pages/dashboard-home.component';
import { NftComponent } from './pages/nft/nft.component';

export const dashboardRoutes: Routes = [
  {
    path: '',
    component: DashboardComponent,
    children: [
      { path: '', component: DashboardHomeComponent },
      { path: 'nfts', component: NftComponent },
      { path: '**', redirectTo: 'errors/404' },
    ],
  },
];
