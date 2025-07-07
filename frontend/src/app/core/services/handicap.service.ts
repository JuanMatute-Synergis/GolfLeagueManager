import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PlayerSessionHandicap {
  id: string;
  playerId: string;
  seasonId: string;
  sessionStartWeekNumber: number;
  sessionInitialHandicap: number;
  createdDate: string;
}

export interface SetSessionHandicapRequest {
  playerId: string;
  seasonId: string;
  sessionStartWeekNumber: number;
  sessionInitialHandicap: number;
}

export interface BulkSetSessionHandicapsRequest {
  seasonId: string;
  sessionStartWeekNumber: number;
  playerHandicaps: Array<{
    playerId: string;
    sessionInitialHandicap: number;
  }>;
}

@Injectable({
  providedIn: 'root'
})
export class HandicapService {
  private readonly apiUrl = '/api/handicap';

  constructor(private http: HttpClient) { }

  /**
   * Get a player's session-specific handicap for a given season and week
   */
  getPlayerSessionHandicap(playerId: string, seasonId: string, weekNumber: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${playerId}/${seasonId}/${weekNumber}`);
  }

  /**
   * Get a player's handicap up to a specific week (uses the proper calculation method)
   */
  getPlayerHandicapUpToWeek(playerId: string, seasonId: string, weekNumber: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${playerId}/${seasonId}/${weekNumber}`);
  }

  /**
   * Set session initial handicap for a player
   */
  setPlayerSessionHandicap(request: SetSessionHandicapRequest): Observable<PlayerSessionHandicap> {
    return this.http.post<PlayerSessionHandicap>(`${this.apiUrl}/set`, request);
  }

  /**
   * Bulk set session handicaps for multiple players
   */
  bulkSetSessionHandicaps(request: BulkSetSessionHandicapsRequest): Observable<{ count: number }> {
    return this.http.post<{ count: number }>(`${this.apiUrl}/bulk-set`, request);
  }

  /**
   * Get all session handicaps for a player in a season
   */
  getPlayerSessionHandicaps(playerId: string, seasonId: string): Observable<PlayerSessionHandicap[]> {
    return this.http.get<PlayerSessionHandicap[]>(`${this.apiUrl}/${playerId}/${seasonId}`);
  }

  /**
   * Get handicaps for all players in a season up to and including a specific week
   * This bulk endpoint replaces multiple individual calls for better performance
   */
  getAllPlayerHandicapsUpToWeek(seasonId: string, weekNumber: number): Observable<{[playerId: string]: number}> {
    return this.http.get<{[playerId: string]: number}>(`${this.apiUrl}/get-all-handicaps/up-to-week/${seasonId}/${weekNumber}`);
  }
}
