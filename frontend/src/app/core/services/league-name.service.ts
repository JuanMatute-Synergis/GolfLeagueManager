import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { LeagueSettingsService } from '../../modules/settings/services/league-settings.service';
import { SeasonService } from '../../modules/settings/services/season.service';

@Injectable({
  providedIn: 'root'
})
export class LeagueNameService {
  private leagueNameSubject = new BehaviorSubject<string>('Golf League');
  public leagueName$ = this.leagueNameSubject.asObservable();
  private initialized = false;

  constructor(
    private leagueSettingsService: LeagueSettingsService,
    private seasonService: SeasonService
  ) {
    this.initializeLeagueName();
  }

  /**
   * Initialize the league name from the active season
   */
  private initializeLeagueName(): void {
    if (this.initialized) return;
    this.initialized = true;

    // First, try to get active seasons
    this.seasonService.getActiveSeasons().subscribe({
      next: (activeSeasons) => {
        if (activeSeasons.length > 0) {
          this.loadLeagueNameFromSettings(activeSeasons[0].id).subscribe();
        } else {
          // No active seasons, try to get the most recent one
          this.seasonService.getSeasons().subscribe({
            next: (allSeasons) => {
              if (allSeasons.length > 0) {
                // Sort by start date (most recent first)
                const sortedSeasons = allSeasons.sort((a, b) =>
                  new Date(b.startDate).getTime() - new Date(a.startDate).getTime()
                );
                this.loadLeagueNameFromSettings(sortedSeasons[0].id).subscribe();
              }
            },
            error: (error) => {
              console.error('Error loading seasons for league name:', error);
              // Keep default name
            }
          });
        }
      },
      error: (error) => {
        console.error('Error loading active seasons for league name:', error);
        // Keep default name
      }
    });
  }

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
