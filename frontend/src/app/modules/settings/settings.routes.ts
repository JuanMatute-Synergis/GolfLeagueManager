import { Routes } from '@angular/router';

export const settingsRoutes: Routes = [
  { path: '', redirectTo: 'players-accounts', pathMatch: 'full' },
  {
    path: 'players-accounts',
    loadComponent: () => import('./components/accounts-settings/accounts-settings.component').then(m => m.AccountsSettingsComponent)
  },
  {
    path: 'seasons',
    loadComponent: () => import('./components/seasons-settings/seasons-settings.component').then(m => m.SeasonsSettingsComponent)
  },
  {
    path: 'league-settings',
    loadComponent: () => import('./components/league-settings/league-settings.component').then(m => m.LeagueSettingsComponent)
  },
  {
    path: 'scheduling',
    loadComponent: () => import('./components/scheduling-settings/scheduling-settings.component').then(m => m.SchedulingSettingsComponent)
  },
  {
    path: 'weeks',
    loadComponent: () => import('../scoring/components/week-management/week-management.component').then(m => m.WeekManagementComponent)
  },
  {
    path: 'score-entry',
    loadComponent: () => import('../scoring/components/score-entry/score-entry.component').then(m => m.ScoreEntryComponent)
  }
];
