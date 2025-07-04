import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { CommonModule } from '@angular/common';
import { UserProfileService } from '../../../core/services/user-profile.service';
import { UserProfile } from '../../../core/services/auth.service';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [RouterModule, AngularSvgIconModule, CommonModule],
  templateUrl: './dashboard-home.component.html',
  styleUrls: ['./dashboard-home.component.css']
})
export class DashboardHomeComponent implements OnInit {
  currentUser: UserProfile | null = null;

  constructor(private userProfileService: UserProfileService) { }

  ngOnInit() {
    // Load user profile to check admin status
    this.currentUser = this.userProfileService.getProfile();
    if (!this.currentUser) {
      this.userProfileService.loadProfile().subscribe({
        next: (profile) => {
          this.currentUser = profile;
        },
        error: (error) => {
          console.error('Error loading user profile:', error);
        }
      });
    }
  }

  get isAdmin(): boolean {
    return this.currentUser?.isAdmin ?? false;
  }
}
