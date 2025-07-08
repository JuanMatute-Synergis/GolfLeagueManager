import { MenuItem } from '../models/menu.model';

export class Menu {
  public static pages: MenuItem[] = [
    {
      group: 'League',
      separator: false,
      items: [
        // {
        //   icon: 'assets/icons/heroicons/outline/bookmark.svg',
        //   label: 'Schedule',
        //   route: '/schedule',
        // },
        // {
        //   icon: 'assets/icons/heroicons/outline/shield-check.svg',
        //   label: 'Standings',
        //   route: '/standings',
        // },
        {
          icon: 'assets/icons/heroicons/outline/users.svg',
          label: 'Players',
          route: '/players',
        },
        {
          icon: 'assets/icons/tablericons/arrows-shuffle-2.svg',
          label: 'Matchups',
          route: '/matchups',
        },
        {
          icon: 'assets/icons/heroicons/outline/document-text.svg',
          label: 'League Summary',
          route: '/league-summary',
        },
        {
          icon: 'assets/icons/heroicons/outline/flag.svg',
          label: 'Rules',
          route: '/rules',
        },
      ],
    },
    // {
    //   group: 'Scoring',
    //   separator: false,
    //   items: [
    //     {
    //       icon: 'assets/icons/heroicons/outline/chart-bar.svg',
    //       label: 'Scoring Dashboard',
    //       route: '/scoring',
    //     },
    //     {
    //       icon: 'assets/icons/heroicons/outline/chart-pie.svg',
    //       label: 'Leaderboard',
    //       route: '/scoring/leaderboard',
    //     },
    //     {
    //       icon: 'assets/icons/heroicons/outline/users.svg',
    //       label: 'Season Standings',
    //       route: '/scoring/standings',
    //     },
    //   ],
    // },
    {
      group: 'League Settings',
      separator: false,
      items: [
        {
          icon: 'assets/icons/heroicons/outline/users.svg',
          label: 'Players & Accounts',
          route: '/settings/players-accounts',
        },
        {
          icon: 'assets/icons/heroicons/outline/calendar.svg',
          label: 'Seasons',
          route: '/settings/seasons',
        },
        {
          icon: 'assets/icons/heroicons/outline/cog-6-tooth.svg',
          label: 'League Settings',
          route: '/settings/league-settings',
        },
        {
          icon: 'assets/icons/heroicons/outline/clock.svg',
          label: 'Scheduling',
          route: '/settings/scheduling',
        },
        {
          icon: 'assets/icons/heroicons/outline/calendar-days.svg',
          label: 'Week Management',
          route: '/settings/weeks',
        },
        {
          icon: 'assets/icons/heroicons/outline/pencil-square.svg',
          label: 'Score Entry',
          route: '/settings/score-entry',
        },
      ],
    },
  ];
}
