import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { LeagueSettingsService } from '../../modules/settings/services/league-settings.service';

@Injectable({
  providedIn: 'root'
})
export class LeagueNameService {
  private readonly _leagueName = new BehaviorSubject<string>('Golf League Manager');
  
  constructor(private leagueSettingsService: LeagueSettingsService) {}
  
  get leagueName$(): Observable<string> {
    return this._leagueName.asObservable();
  }
  
  get leagueName(): string {
    return this._leagueName.value;
  }
  
  /**
   * Load the league name from settings
   */
  async loadLeagueName(): Promise<void> {
    try {
      const settings = await this.leagueSettingsService.getLeagueSettings().toPromise();
      if (settings && settings.leagueName && settings.leagueName.trim()) {
        this._leagueName.next(settings.leagueName);
      }
    } catch (error) {
      console.warn('Failed to load league name from settings:', error);
      // Keep the default value
    }
  }
  
  /**
   * Update the league name (should be called when settings are updated)
   */
  updateLeagueName(leagueName: string): void {
    this._leagueName.next(leagueName && leagueName.trim() ? leagueName : 'Golf League Manager');
  }
}
