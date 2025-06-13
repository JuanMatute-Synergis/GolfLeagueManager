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
  ];
}
