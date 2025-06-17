import { MenuItem } from '../models/menu.model';

export class Menu {
  public static pages: MenuItem[] = [
    {
      group: 'League',
      separator: false,
      items: [
        {
          icon: 'assets/icons/heroicons/outline/bookmark.svg',
          label: 'Schedule',
          route: '/schedule',
        },
        {
          icon: 'assets/icons/heroicons/outline/shield-check.svg',
          label: 'Standings',
          route: '/standings',
        },
        {
          icon: 'assets/icons/heroicons/outline/users.svg',
          label: 'Players',
          route: '/players',
        },
        {
          icon: 'assets/icons/heroicons/outline/cog-6-tooth.svg',
          label: 'Settings',
          route: '/settings',
        },
      ],
    },
    {
      group: 'Scoring',
      separator: false,
      items: [
        {
          icon: 'assets/icons/heroicons/outline/chart-bar.svg',
          label: 'Scoring Dashboard',
          route: '/scoring',
        },
        {
          icon: 'assets/icons/heroicons/outline/calendar-days.svg',
          label: 'Week Management',
          route: '/scoring/weeks',
        },
        {
          icon: 'assets/icons/heroicons/outline/pencil-square.svg',
          label: 'Score Entry',
          route: '/scoring/score-entry',
        },
        {
          icon: 'assets/icons/heroicons/outline/chart-pie.svg',
          label: 'Leaderboard',
          route: '/scoring/leaderboard',
        },
        {
          icon: 'assets/icons/heroicons/outline/trophy.svg',
          label: 'Season Standings',
          route: '/scoring/standings',
        },
      ],
    },
  ];
}
