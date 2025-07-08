import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LeagueRules, UpdateRulesRequest } from '../models/league-rules.model';

@Injectable({
  providedIn: 'root'
})
export class RulesService {
  private apiUrl = '/api/leaguerules';

  constructor(private http: HttpClient) {}

  getRules(seasonId: string): Observable<LeagueRules> {
    return this.http.get<LeagueRules>(`${this.apiUrl}/${seasonId}`);
  }

  updateRules(seasonId: string, request: UpdateRulesRequest): Observable<LeagueRules> {
    return this.http.put<LeagueRules>(`${this.apiUrl}/${seasonId}`, request);
  }
}
