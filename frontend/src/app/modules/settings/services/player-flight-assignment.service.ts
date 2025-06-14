import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Player } from './player.service';

export interface PlayerFlightAssignment {
  id?: string;  // Changed from number to string for GUID compatibility
  playerId: string;  // Changed from number to string for GUID compatibility
  flightId: string;  // Changed from number to string for GUID compatibility
  isFlightLeader: boolean;
  handicapAtAssignment?: number;
  player?: Player; // For joined data
}

@Injectable({
  providedIn: 'root'
})
export class PlayerFlightAssignmentService {
  private readonly apiUrl = 'http://localhost:5274/api/player-flight-assignments';

  // Mock data for development until backend is ready
  private mockAssignments: PlayerFlightAssignment[] = [
    // Sample mock assignments for initial testing
    { id: '1', playerId: '1', flightId: '101', isFlightLeader: true, handicapAtAssignment: 12 },
    { id: '2', playerId: '2', flightId: '101', isFlightLeader: false, handicapAtAssignment: 15 },
    { id: '3', playerId: '3', flightId: '101', isFlightLeader: false, handicapAtAssignment: 8 },
    { id: '4', playerId: '1', flightId: '102', isFlightLeader: false, handicapAtAssignment: 12 },
    { id: '5', playerId: '4', flightId: '102', isFlightLeader: true, handicapAtAssignment: 10 }
  ];

  constructor(private http: HttpClient) {}

  getAssignmentsByFlight(flightId: string): Observable<PlayerFlightAssignment[]> {
    // Later, when API is ready:
    // return this.http.get<PlayerFlightAssignment[]>(`${this.apiUrl}/flight/${flightId}`);
    
    // For now, return mock data
    return of(this.mockAssignments.filter(a => a.flightId === flightId));
  }

  addAssignment(assignment: PlayerFlightAssignment): Observable<PlayerFlightAssignment> {
    // Later, when API is ready:
    // return this.http.post<PlayerFlightAssignment>(this.apiUrl, assignment);
    
    // For now, add to mock data
    const newAssignment = { ...assignment, id: this.getNextId() };
    this.mockAssignments.push(newAssignment);
    return of(newAssignment);
  }

  removeAssignment(assignmentId: string): Observable<void> {
    // Later, when API is ready:
    // return this.http.delete<void>(`${this.apiUrl}/${assignmentId}`);
    
    // For now, remove from mock data
    this.mockAssignments = this.mockAssignments.filter(a => a.id !== assignmentId);
    return of(void 0);
  }

  updateAssignment(assignment: PlayerFlightAssignment): Observable<void> {
    // Later, when API is ready:
    // return this.http.put<void>(`${this.apiUrl}/${assignment.id}`, assignment);
    
    // For now, update mock data
    const index = this.mockAssignments.findIndex(a => a.id === assignment.id);
    if (index !== -1) {
      this.mockAssignments[index] = assignment;
    }
    return of(void 0);
  }

  private getNextId(): string {
    const maxNumericId = this.mockAssignments.length > 0 
      ? Math.max(...this.mockAssignments.map(a => parseInt(a.id || '0'))) 
      : 0;
    return (maxNumericId + 1).toString();
  }
  
  /**
   * Gets players with their current handicap for a particular flight
   * @param flightId The flight ID to get handicaps for
   * @param players The list of all players
   */
  getPlayersWithHandicap(flightId: string, players: Player[]): Observable<(Player & { handicap?: number })[]> {
    // In a real implementation, we would fetch this from the backend
    // For now, return some mock handicap data for existing players
    
    // Get existing assignments for this flight
    const assignedPlayerIds = this.mockAssignments
      .filter(a => a.flightId === flightId)
      .map(a => a.playerId);
    
    // Return enhanced player objects with handicap data
    return of(players.map(player => {
      const assignment = this.mockAssignments.find(
        a => a.playerId === player.id && a.flightId === flightId
      );
      
      return {
        ...player,
        handicap: assignment?.handicapAtAssignment || Math.floor(Math.random() * 20) + 5 // Random handicap between 5-25
      };
    }));
  }
}