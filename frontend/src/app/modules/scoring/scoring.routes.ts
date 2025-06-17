import { Routes } from '@angular/router';

export const scoringRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/scoring-dashboard/scoring-dashboard.component').then(m => m.ScoringDashboardComponent)
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./components/scoring-dashboard/scoring-dashboard.component').then(m => m.ScoringDashboardComponent)
  },
  {
    path: 'weeks',
    loadComponent: () => import('./components/week-management/week-management.component').then(m => m.WeekManagementComponent)
  },
  {
    path: 'score-entry',
    loadComponent: () => import('./components/score-entry/score-entry.component').then(m => m.ScoreEntryComponent)
  },
  {
    path: 'leaderboard',
    loadComponent: () => import('./components/leaderboard/leaderboard.component').then(m => m.LeaderboardComponent)
  },
  {
    path: 'standings',
    loadComponent: () => import('./components/season-standings/season-standings.component').then(m => m.SeasonStandingsComponent)
  }
];
