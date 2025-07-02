import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Flight {
  id?: string;  // Changed from number to string for GUID compatibility
  name: string;
  maxPlayers: number;
  description: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
  seasonId?: string; // Changed from number to string for GUID compatibility
}

@Injectable({
  providedIn: 'root'
})
export class FlightService {
  private readonly apiUrl = '/api/flights';

  constructor(private http: HttpClient) { }

  getFlights(): Observable<Flight[]> {
    return this.http.get<Flight[]>(this.apiUrl);
  }

  getFlightById(id: string): Observable<Flight> {
    return this.http.get<Flight>(`${this.apiUrl}/${id}`);
  }

  getFlightsBySeason(seasonId: string): Observable<Flight[]> {
    return this.http.get<Flight[]>(`${this.apiUrl}/season/${seasonId}`);
  }

  getActiveFlights(): Observable<Flight[]> {
    return this.http.get<Flight[]>(`${this.apiUrl}/active`);
  }

  addFlight(flight: Flight): Observable<Flight> {
    return this.http.post<Flight>(this.apiUrl, flight);
  }

  updateFlight(flight: Flight): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${flight.id}`, flight);
  }

  deleteFlight(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
