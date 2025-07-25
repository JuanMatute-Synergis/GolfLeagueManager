import { animate, state, style, transition, trigger } from '@angular/animations';
import { NgClass, CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { ThemeService } from '../../../../../core/services/theme.service';
import { AuthService, UserProfile } from '../../../../../core/services/auth.service';
import { UserProfileService } from '../../../../../core/services/user-profile.service';
import { ClickOutsideDirective } from '../../../../../shared/directives/click-outside.directive';

@Component({
  selector: 'app-profile-menu',
  standalone: true,
  templateUrl: './profile-menu.component.html',
  styleUrls: ['./profile-menu.component.css'],
  imports: [ClickOutsideDirective, NgClass, RouterLink, AngularSvgIconModule, CommonModule],
  animations: [
    trigger('openClose', [
      state(
        'open',
        style({
          opacity: 1,
          transform: 'translateY(0)',
          visibility: 'visible',
        }),
      ),
      state(
        'closed',
        style({
          opacity: 0,
          transform: 'translateY(-20px)',
          visibility: 'hidden',
        }),
      ),
      transition('open => closed', [animate('0.2s')]),
      transition('closed => open', [animate('0.2s')]),
    ]),
  ],
})
export class ProfileMenuComponent implements OnInit {
  public isOpen = false;
  public userProfile: UserProfile | null = null;
  public defaultAvatarUrl = 'assets/icons/heroicons/outline/user-circle.svg';

  public profileMenu = [
    {
      title: 'Your Profile',
      icon: './assets/icons/heroicons/outline/user-circle.svg',
      link: '/profile',
    },
    {
      title: 'Settings',
      icon: './assets/icons/heroicons/outline/cog-6-tooth.svg',
      link: '/settings',
    },
    {
      title: 'Log out',
      icon: './assets/icons/heroicons/outline/logout.svg',
      link: '/auth',
    },
  ];

  public themeColors = [
    {
      name: 'base',
      code: '#e11d48',
    },
    {
      name: 'yellow',
      code: '#f59e0b',
    },
    {
      name: 'green',
      code: '#22c55e',
    },
    {
      name: 'blue',
      code: '#3b82f6',
    },
    {
      name: 'orange',
      code: '#ea580c',
    },
    {
      name: 'red',
      code: '#cc0022',
    },
    {
      name: 'violet',
      code: '#6d28d9',
    },
  ];

  public themeMode = ['light', 'dark'];
  public themeDirection = ['ltr', 'rtl'];

  constructor(
    public themeService: ThemeService, 
    private authService: AuthService, 
    private userProfileService: UserProfileService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadUserProfile();
    
    // Subscribe to profile updates
    this.userProfileService.profile$.subscribe(profile => {
      this.userProfile = profile;
    });
  }

  private loadUserProfile(): void {
    this.userProfileService.loadProfile().subscribe({
      error: (error) => {
        console.error('Error loading user profile:', error);
      }
    });
  }

  public getDisplayName(): string {
    if (this.userProfile?.player?.firstName && this.userProfile?.player?.lastName) {
      return `${this.userProfile.player.firstName} ${this.userProfile.player.lastName}`;
    }
    return this.userProfile?.username || 'User';
  }

  public getAvatarUrl(): string {
    return this.userProfile?.player?.imageUrl || this.defaultAvatarUrl;
  }

  public getUsername(): string {
    return this.userProfile?.username || '';
  }

  public toggleMenu(): void {
    this.isOpen = !this.isOpen;
  }

  public logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/auth']);
      },
      error: (error) => {
        console.error('Logout error:', error);
        // Even if logout fails, redirect to auth page
        this.router.navigate(['/auth']);
      }
    });
  }

  toggleThemeMode() {
    this.themeService.theme.update((theme) => {
      const mode = !this.themeService.isDark ? 'dark' : 'light';
      return { ...theme, mode: mode };
    });
  }

  toggleThemeColor(color: string) {
    this.themeService.theme.update((theme) => {
      return { ...theme, color: color };
    });
  }

  setDirection(value: string) {
    this.themeService.theme.update((theme) => {
      return { ...theme, direction: value };
    });
  }

  public refreshProfile(): void {
    this.userProfileService.refreshProfile();
  }
}
