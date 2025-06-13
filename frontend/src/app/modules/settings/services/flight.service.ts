import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Flight {
  id?: number;
  name: string;
  date: string; // ISO date string
  startTime: string; // TimeSpan as string (e.g., "08:00:00")
  course: string;
  maxPlayers: number;
  description: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
  seasonId?: number; // Reference to the season this flight belongs to
}

@Injectable({
  providedIn: 'root'
})
export class FlightService {
  private readonly apiUrl = 'http://localhost:5274/api/flights';

  constructor(private http: HttpClient) {}

  getFlights(): Observable<Flight[]> {
    return this.http.get<Flight[]>(this.apiUrl);
  }

  getFlightById(id: number): Observable<Flight> {
    return this.http.get<Flight>(`${this.apiUrl}/${id}`);
  }
  
  getFlightsBySeason(seasonId: number): Observable<Flight[]> {
    return this.http.get<Flight[]>(`${this.apiUrl}/season/${seasonId}`);
  }

  getActiveFlights(): Observable<Flight[]> {
    return this.http.get<Flight[]>(`${this.apiUrl}/active`);
  }

  getUpcomingFlights(): Observable<Flight[]> {
    return this.http.get<Flight[]>(`${this.apiUrl}/upcoming`);
  }

  getFlightsByDateRange(startDate: string, endDate: string): Observable<Flight[]> {
    return this.http.get<Flight[]>(`${this.apiUrl}/date-range?startDate=${startDate}&endDate=${endDate}`);
  }

  addFlight(flight: Flight): Observable<Flight> {
    return this.http.post<Flight>(this.apiUrl, flight);
  }

  updateFlight(flight: Flight): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${flight.id}`, flight);
  }

  deleteFlight(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
