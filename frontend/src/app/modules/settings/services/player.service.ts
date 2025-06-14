import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

export interface Player {
  id?: string;  // Changed from number to string for GUID compatibility
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
}

@Injectable({
  providedIn: 'root'
})
export class PlayerService {
  private readonly apiUrl = 'http://localhost:5274/api/players';
  
  // Mock data for development until backend is ready
  private mockPlayers: Player[] = [
    { id: '1', firstName: 'John', lastName: 'Smith', email: 'john.smith@example.com', phone: '555-123-4567' },
    { id: '2', firstName: 'Emma', lastName: 'Johnson', email: 'emma.j@example.com', phone: '555-234-5678' },
    { id: '3', firstName: 'Michael', lastName: 'Williams', email: 'mwilliams@example.com', phone: '555-345-6789' },
    { id: '4', firstName: 'Olivia', lastName: 'Brown', email: 'obrown@example.com', phone: '555-456-7890' },
    { id: '5', firstName: 'James', lastName: 'Davis', email: 'james.davis@example.com', phone: '555-567-8901' }
  ];

  constructor(private http: HttpClient) {}

  getPlayers(): Observable<Player[]> {
    // Use mock data for now
    return of(this.mockPlayers);
    
    // When backend is ready, use this:
    // return this.http.get<Player[]>(this.apiUrl);
  }

  addPlayer(player: Player): Observable<Player> {
    // Mock implementation
    const newId = this.mockPlayers.length > 0 
      ? Math.max(...this.mockPlayers.map(p => parseInt(p.id || '0'))) + 1 
      : 1;
    const newPlayer = { ...player, id: newId.toString() };
    this.mockPlayers.push(newPlayer);
    return of(newPlayer);
    
    // When backend is ready, use this:
    // return this.http.post<Player>(this.apiUrl, player);
  }

  updatePlayer(player: Player): Observable<void> {
    // Mock implementation
    const index = this.mockPlayers.findIndex(p => p.id === player.id);
    if (index !== -1) {
      this.mockPlayers[index] = player;
    }
    return of(void 0);
    
    // When backend is ready, use this:
    // return this.http.put<void>(`${this.apiUrl}/${player.id}`, player);
  }

  deletePlayer(id: string): Observable<void> {
    // Mock implementation
    this.mockPlayers = this.mockPlayers.filter(p => p.id !== id);
    return of(void 0);
    
    // When backend is ready, use this:
    // return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
