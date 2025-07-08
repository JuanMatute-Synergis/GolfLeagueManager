import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { LeagueSettingsService } from '../../modules/settings/services/league-settings.service';

@Injectable({
  providedIn: 'root'
})
export class LeagueNameService {
  private leagueNameSubject = new BehaviorSubject<string>('Golf League');
  public leagueName$ = this.leagueNameSubject.asObservable();

  constructor(private leagueSettingsService: LeagueSettingsService) {}

  /**
   * Update the league name
   */
  setLeagueName(name: string): void {
    this.leagueNameSubject.next(name || 'Golf League');
  }

  /**
   * Get the current league name
   */
  getCurrentLeagueName(): string {
    return this.leagueNameSubject.value;
  }

  /**
   * Load league name from settings for a specific season
   */
  loadLeagueNameFromSettings(seasonId: string): Observable<string> {
    return new Observable(observer => {
      this.leagueSettingsService.getLeagueSettings(seasonId).subscribe({
        next: (settings) => {
          const leagueName = settings.leagueName || 'Golf League';
          this.setLeagueName(leagueName);
          observer.next(leagueName);
          observer.complete();
        },
        error: (error) => {
          console.error('Error loading league name from settings:', error);
          // Fall back to default name
          const defaultName = 'Golf League';
          this.setLeagueName(defaultName);
          observer.next(defaultName);
          observer.complete();
        }
      });
    });
  }
}
