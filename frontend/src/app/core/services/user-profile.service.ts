import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthService, UserProfile } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class UserProfileService {
  private profileSubject = new BehaviorSubject<UserProfile | null>(null);
  public profile$ = this.profileSubject.asObservable();

  constructor(private authService: AuthService) {}

  loadProfile(): Observable<UserProfile> {
    return new Observable(observer => {
      this.authService.getUserProfile().subscribe({
        next: (profile) => {
          this.profileSubject.next(profile);
          observer.next(profile);
          observer.complete();
        },
        error: (error) => {
          observer.error(error);
        }
      });
    });
  }

  getProfile(): UserProfile | null {
    return this.profileSubject.value;
  }

  refreshProfile(): void {
    this.loadProfile().subscribe();
  }
}
