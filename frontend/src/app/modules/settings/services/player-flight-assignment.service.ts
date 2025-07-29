import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Player } from './player.service';

export interface PlayerFlightAssignment {
  id?: string;  // GUID compatibility
  playerId: string;  // GUID compatibility
  flightId: string;  // GUID compatibility
  seasonId: string;  // GUID compatibility
  sessionStartWeekNumber: number;
  isFlightLeader: boolean;
  handicapAtAssignment?: number;
  assignmentDate?: string;
  player?: Player; // For joined data
}

@Injectable({
  providedIn: 'root'
})
export class PlayerFlightAssignmentService {
  private readonly apiUrl = '/api/player-flight-assignments';

  constructor(private http: HttpClient) { }

  getAssignmentsByFlight(flightId: string): Observable<PlayerFlightAssignment[]> {
    return this.http.get<PlayerFlightAssignment[]>(`${this.apiUrl}/flight/${flightId}`);
  }

  getAssignmentsByFlightAndSession(flightId: string, seasonId: string, sessionStartWeekNumber: number): Observable<PlayerFlightAssignment[]> {
    const params = new HttpParams()
      .set('seasonId', seasonId)
      .set('sessionStartWeekNumber', sessionStartWeekNumber.toString());
    return this.http.get<PlayerFlightAssignment[]>(`${this.apiUrl}/flight/${flightId}/session`, { params });
  }

  getAssignmentsByPlayerAndSeason(playerId: string, seasonId: string): Observable<PlayerFlightAssignment[]> {
    return this.http.get<PlayerFlightAssignment[]>(`${this.apiUrl}/player/${playerId}/season/${seasonId}`);
  }

  getAssignmentsBySession(seasonId: string, sessionStartWeekNumber: number): Observable<PlayerFlightAssignment[]> {
    const params = new HttpParams()
      .set('seasonId', seasonId)
      .set('sessionStartWeekNumber', sessionStartWeekNumber.toString());
    return this.http.get<PlayerFlightAssignment[]>(`${this.apiUrl}/session`, { params });
  }

  getPlayerAssignmentForSession(playerId: string, seasonId: string, sessionStartWeekNumber: number): Observable<PlayerFlightAssignment> {
    const params = new HttpParams()
      .set('seasonId', seasonId)
      .set('sessionStartWeekNumber', sessionStartWeekNumber.toString());
    return this.http.get<PlayerFlightAssignment>(`${this.apiUrl}/player/${playerId}/session`, { params });
  }

  addAssignment(assignment: PlayerFlightAssignment): Observable<PlayerFlightAssignment> {
    return this.http.post<PlayerFlightAssignment>(this.apiUrl, assignment);
  }

  removeAssignment(assignmentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${assignmentId}`);
  }

  updateAssignment(assignment: PlayerFlightAssignment): Observable<PlayerFlightAssignment> {
    return this.http.put<PlayerFlightAssignment>(`${this.apiUrl}/${assignment.id}`, assignment);
  }

  getAssignmentsByPlayer(playerId: string): Observable<PlayerFlightAssignment[]> {
    return this.http.get<PlayerFlightAssignment[]>(`${this.apiUrl}/player/${playerId}`);
  }

  getAllAssignments(): Observable<PlayerFlightAssignment[]> {
    return this.http.get<PlayerFlightAssignment[]>(this.apiUrl);
  }
}