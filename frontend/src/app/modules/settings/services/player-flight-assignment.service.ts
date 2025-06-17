import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Player } from './player.service';

export interface PlayerFlightAssignment {
  id?: string;  // GUID compatibility
  playerId: string;  // GUID compatibility
  flightId: string;  // GUID compatibility
  isFlightLeader: boolean;
  handicapAtAssignment?: number;
  player?: Player; // For joined data
}

@Injectable({
  providedIn: 'root'
})
export class PlayerFlightAssignmentService {
  private readonly apiUrl = 'http://localhost:5274/api/player-flight-assignments';

  constructor(private http: HttpClient) {}

  getAssignmentsByFlight(flightId: string): Observable<PlayerFlightAssignment[]> {
    return this.http.get<PlayerFlightAssignment[]>(`${this.apiUrl}/flight/${flightId}`);
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