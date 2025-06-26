import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { UserProfileService } from '../../../../core/services/user-profile.service';

interface SettingsMenuItem {
  id: string;
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-settings-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './settings-layout.component.html',
  styleUrls: ['./settings-layout.component.css']
})
export class SettingsLayoutComponent implements OnInit {
  menuItems: SettingsMenuItem[] = [
    {
      id: 'players-accounts',
      label: 'Players & Accounts',
      icon: 'fas fa-users',
      route: '/settings/players-accounts'
    },
    {
      id: 'seasons',
      label: 'Seasons',
      icon: 'fas fa-calendar-alt',
      route: '/settings/seasons'
    },
    {
      id: 'scheduling',
      label: 'Scheduling',
      icon: 'fas fa-clock',
      route: '/settings/scheduling'
    },
    {
      id: 'weeks',
      label: 'Week Management',
      icon: 'fas fa-calendar-week',
      route: '/settings/weeks'
    },
    {
      id: 'score-entry',
      label: 'Score Entry',
      icon: 'fas fa-edit',
      route: '/settings/score-entry'
    }
  ];

  isSidebarCollapsed = false;
  activeRoute = '';

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private userProfileService: UserProfileService
  ) {}

  ngOnInit() {
    // Track active route
    this.router.events.subscribe(() => {
      this.activeRoute = this.router.url;
    });
    
    // Set initial active route
    this.activeRoute = this.router.url;
  }

  toggleSidebar() {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  isActive(route: string): boolean {
    return this.activeRoute === route || this.activeRoute.startsWith(route + '/');
  }

  navigateTo(route: string) {
    this.router.navigate([route]);
  }
}
