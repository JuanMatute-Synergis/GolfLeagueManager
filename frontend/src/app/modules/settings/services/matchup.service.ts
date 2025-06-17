import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Matchup {
  id?: string;
  weekId: string;
  playerAId: string;
  playerBId: string;
  playerAScore?: number;
  playerBScore?: number;
  playerAPoints?: number;
  playerBPoints?: number;
  // Navigation properties for display
  playerA?: any;
  playerB?: any;
  week?: any;
}

export interface MatchupWithPlayers extends Matchup {
  playerAName: string;
  playerBName: string;
  flightName: string;
}

@Injectable({
  providedIn: 'root'
})
export class MatchupService {
  private readonly apiUrl = 'http://localhost:5274/api/matchups';

  constructor(private http: HttpClient) {}

  // Get all matchups
  getMatchups(): Observable<Matchup[]> {
    return this.http.get<Matchup[]>(this.apiUrl);
  }

  // Get matchups by week
  getMatchupsByWeek(weekId: string): Observable<Matchup[]> {
    return this.http.get<Matchup[]>(`${this.apiUrl}/week/${weekId}`);
  }

  // Get matchups by season
  getMatchupsBySeason(seasonId: string): Observable<Matchup[]> {
    return this.http.get<Matchup[]>(`${this.apiUrl}/season/${seasonId}`);
  }

  // Get matchup by ID
  getMatchup(id: string): Observable<Matchup> {
    return this.http.get<Matchup>(`${this.apiUrl}/${id}`);
  }

  // Create a new matchup
  createMatchup(matchup: Partial<Matchup>): Observable<Matchup> {
    return this.http.post<Matchup>(this.apiUrl, matchup);
  }

  // Update an existing matchup
  updateMatchup(matchup: Matchup): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${matchup.id}`, matchup);
  }

  // Delete a matchup
  deleteMatchup(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Generate round-robin matchups for a week
  generateRoundRobinMatchups(weekId: string, seasonId: string): Observable<Matchup[]> {
    return this.http.post<Matchup[]>(`${this.apiUrl}/generate-round-robin`, {
      weekId,
      seasonId
    });
  }
}
