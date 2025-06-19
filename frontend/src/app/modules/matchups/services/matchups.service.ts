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
  playerA?: Player;
  playerB?: Player;
  week?: Week;
}

export interface MatchupWithFlightInfo extends Matchup {
  playerAName: string;
  playerBName: string;
  flightName: string;
  flightId: string;
}

export interface Player {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  handicap?: number;
  initialAverageScore?: number;
  currentAverageScore?: number;
}

export interface Week {
  id: string;
  weekNumber: number;
  date: string; // The Wednesday date when the week is played
  name: string;
  isActive: boolean;
  seasonId: string;
}

export interface Season {
  id: string;
  name: string;
  year: number;
  startDate: string;
  endDate: string;
  seasonNumber: number;
}

export interface Flight {
  id: string;
  name: string;
  maxPlayers: number;
  description?: string;
  isActive: boolean;
  seasonId?: string;
}

export interface PlayerFlightAssignment {
  id?: string;
  playerId: string;
  flightId: string;
  handicapAtAssignment?: number;
  isFlightLeader?: boolean;
  player?: Player;
  flight?: Flight;
}

@Injectable({
  providedIn: 'root'
})
export class MatchupsService {
  private readonly apiUrl = 'http://localhost:5274/api';

  constructor(private http: HttpClient) {}

  // Get all seasons
  getSeasons(): Observable<Season[]> {
    return this.http.get<Season[]>(`${this.apiUrl}/seasons`);
  }

  // Get weeks by season
  getWeeksBySeason(seasonId: string): Observable<Week[]> {
    return this.http.get<Week[]>(`${this.apiUrl}/weeks/season/${seasonId}`);
  }

  // Get matchups by week
  getMatchupsByWeek(weekId: string): Observable<Matchup[]> {
    return this.http.get<Matchup[]>(`${this.apiUrl}/matchups/week/${weekId}`);
  }

  // Get flights by season
  getFlightsBySeason(seasonId: string): Observable<Flight[]> {
    return this.http.get<Flight[]>(`${this.apiUrl}/flights/season/${seasonId}`);
  }

  // Get player flight assignments
  getPlayerFlightAssignments(): Observable<PlayerFlightAssignment[]> {
    return this.http.get<PlayerFlightAssignment[]>(`${this.apiUrl}/player-flight-assignments`);
  }

  // Get all players with flight information
  getPlayers(): Observable<Player[]> {
    return this.http.get<Player[]>(`${this.apiUrl}/players`);
  }

  // Get players in flights for a specific season (includes handicap and average score)
  getPlayersInFlights(seasonId: string): Observable<Player[]> {
    return this.http.get<Player[]>(`${this.apiUrl}/players/season/${seasonId}/flights`);
  }
}
