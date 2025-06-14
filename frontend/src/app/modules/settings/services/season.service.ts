import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Season {
  id: string;
  name: string;
  year: number;
  seasonNumber: number;
  startDate: string;
  endDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class SeasonService {
  private readonly apiUrl = 'http://localhost:5274/api/seasons';

  constructor(private http: HttpClient) {}

  getSeasons(): Observable<Season[]> {
    return this.http.get<Season[]>(this.apiUrl);
  }

  getSeasonById(id: string): Observable<Season> {
    return this.http.get<Season>(`${this.apiUrl}/${id}`);
  }
  
  getActiveSeasons(): Observable<Season[]> {
    return this.http.get<Season[]>(`${this.apiUrl}/active`);
  }

  addSeason(season: Season): Observable<Season> {
    return this.http.post<Season>(this.apiUrl, season);
  }

  updateSeason(season: Season): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${season.id}`, season);
  }

  deleteSeason(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
