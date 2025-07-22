import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Week,
  ScoreEntry,
  PlayerSeasonStats,
  BulkScoreEntry,
  Season,
  Player,
  PlayerWithFlight
} from '../models/week.model';

@Injectable({
  providedIn: 'root'
})
export class ScoringService {
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) { }

  // Week Management
  getWeeks(): Observable<Week[]> {
    return this.http.get<Week[]>(`${this.baseUrl}/weeks`);
  }

  getWeek(id: string): Observable<Week> {
    return this.http.get<Week>(`${this.baseUrl}/weeks/${id}`);
  }

  getWeekById(id: string): Observable<Week> {
    return this.http.get<Week>(`${this.baseUrl}/weeks/${id}`);
  }

  getWeeksBySeason(seasonId: string): Observable<Week[]> {
    return this.http.get<Week[]>(`${this.baseUrl}/weeks/season/${seasonId}`);
  }

  getCurrentWeek(seasonId: string): Observable<Week> {
    return this.http.get<Week>(`${this.baseUrl}/weeks/season/${seasonId}/current`);
  }

  getNextWeek(seasonId: string): Observable<Week> {
    return this.http.get<Week>(`${this.baseUrl}/weeks/season/${seasonId}/next`);
  }

  createWeek(week: Partial<Week>): Observable<Week> {
    return this.http.post<Week>(`${this.baseUrl}/weeks`, week);
  }

  updateWeek(id: string, week: Week): Observable<Week> {
    return this.http.put<Week>(`${this.baseUrl}/weeks/${id}`, week);
  }

  deleteWeek(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/weeks/${id}`);
  }

  // Score Entry Management
  getScoreEntries(): Observable<ScoreEntry[]> {
    return this.http.get<ScoreEntry[]>(`${this.baseUrl}/scoreentries`);
  }

  getScoreEntry(id: string): Observable<ScoreEntry> {
    return this.http.get<ScoreEntry>(`${this.baseUrl}/scoreentries/${id}`);
  }

  getScoreEntriesByWeek(weekId: string): Observable<ScoreEntry[]> {
    return this.http.get<ScoreEntry[]>(`${this.baseUrl}/scoreentries/week/${weekId}`);
  }

  getScoresByWeek(weekId: string): Observable<ScoreEntry[]> {
    return this.http.get<ScoreEntry[]>(`${this.baseUrl}/scoreentries/week/${weekId}`);
  }

  getScoreEntriesByPlayer(playerId: string): Observable<ScoreEntry[]> {
    return this.http.get<ScoreEntry[]>(`${this.baseUrl}/scoreentries/player/${playerId}`);
  }

  getWeekLeaderboard(weekId: string): Observable<ScoreEntry[]> {
    return this.http.get<ScoreEntry[]>(`${this.baseUrl}/scoreentries/week/${weekId}/leaderboard`);
  }

  getSeasonStandings(seasonId: string): Observable<PlayerSeasonStats[]> {
    return this.http.get<PlayerSeasonStats[]>(`${this.baseUrl}/scoreentries/season/${seasonId}/standings`);
  }

  createScoreEntry(scoreEntry: Partial<ScoreEntry>): Observable<ScoreEntry> {
    return this.http.post<ScoreEntry>(`${this.baseUrl}/scoreentries`, scoreEntry);
  }

  bulkCreateScoreEntries(scoreEntries: BulkScoreEntry[]): Observable<ScoreEntry[]> {
    return this.http.post<ScoreEntry[]>(`${this.baseUrl}/scoreentries/bulk`, scoreEntries);
  }

  // Convenience methods for score entry component
  enterScore(scoreEntry: Partial<ScoreEntry>): Observable<ScoreEntry> {
    return this.createScoreEntry(scoreEntry);
  }

  enterBulkScores(scoreEntries: BulkScoreEntry[]): Observable<ScoreEntry[]> {
    return this.bulkCreateScoreEntries(scoreEntries);
  }

  getWeeklyLeaderboard(weekId: string): Observable<ScoreEntry[]> {
    return this.getWeekLeaderboard(weekId);
  }

  updateScoreEntry(id: string, scoreEntry: ScoreEntry): Observable<ScoreEntry> {
    return this.http.put<ScoreEntry>(`${this.baseUrl}/scoreentries/${id}`, scoreEntry);
  }

  deleteScoreEntry(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/scoreentries/${id}`);
  }

  // Helper methods to get existing data
  getSeasons(): Observable<Season[]> {
    return this.http.get<Season[]>(`${this.baseUrl}/seasons`);
  }

  getActiveSeasons(): Observable<Season[]> {
    return this.http.get<Season[]>(`${this.baseUrl}/seasons/active`);
  }

  getPlayers(): Observable<Player[]> {
    return this.http.get<Player[]>(`${this.baseUrl}/players`);
  }

  // Get players assigned to flights in a specific season
  getPlayersInFlights(seasonId: string): Observable<PlayerWithFlight[]> {
    return this.http.get<PlayerWithFlight[]>(`${this.baseUrl}/players/season/${seasonId}/flights`);
  }
}
