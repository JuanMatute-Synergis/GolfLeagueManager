import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PlayerScoringStats {
  playerId: string;
  playerName: string;
  initialAverageScore: number;
  currentAverageScore: number;
  roundsPlayed: number;
  bestScore: number | null;
  worstScore: number | null;
  totalStrokes: number;
  averageScoreCalculated: number;
}

@Injectable({
  providedIn: 'root'
})
export class AverageScoreService {
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) { }

  /**
   * Get scoring statistics for a player in a specific season
   */
  getPlayerStats(playerId: string, seasonId: string): Observable<PlayerScoringStats> {
    return this.http.get<PlayerScoringStats>(`${this.baseUrl}/AverageScore/player/${playerId}/season/${seasonId}/stats`);
  }

  /**
   * Update average score for a specific player in a season
   */
  updatePlayerAverageScore(playerId: string, seasonId: string): Observable<{ playerId: string, seasonId: string, currentAverageScore: number }> {
    return this.http.post<{ playerId: string, seasonId: string, currentAverageScore: number }>(`${this.baseUrl}/AverageScore/player/${playerId}/season/${seasonId}/update`, {});
  }

  /**
   * Update average scores for all players who played in a specific week
   */
  updateAverageScoresForWeek(weekId: string): Observable<{ [playerId: string]: number }> {
    return this.http.post<{ [playerId: string]: number }>(`${this.baseUrl}/AverageScore/week/${weekId}/update-all`, {});
  }

  /**
   * Recalculate all player average scores for a season
   */
  recalculateAllAverageScoresForSeason(seasonId: string): Observable<{ [playerId: string]: number }> {
    return this.http.post<{ [playerId: string]: number }>(`${this.baseUrl}/AverageScore/season/${seasonId}/recalculate-all`, {});
  }

  /**
   * Get scoring statistics for all players in a season
   */
  getAllPlayerStatsForSeason(seasonId: string): Observable<PlayerScoringStats[]> {
    return this.http.get<PlayerScoringStats[]>(`${this.baseUrl}/AverageScore/season/${seasonId}/all-stats`);
  }
}
