import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PlayerSeasonRecord {
  id?: string;
  playerId: string;
  seasonId: string;
  initialHandicap: number;
  initialAverageScore: number;
  createdDate?: string;
  modifiedDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PlayerSeasonStatsService {
  private readonly apiUrl = '/api/player-season-stats';

  constructor(private http: HttpClient) { }

  createPlayerSeasonRecord(record: PlayerSeasonRecord): Observable<PlayerSeasonRecord> {
    return this.http.post<PlayerSeasonRecord>(this.apiUrl, record);
  }

  updatePlayerSeasonRecord(record: PlayerSeasonRecord): Observable<PlayerSeasonRecord> {
    return this.http.put<PlayerSeasonRecord>(`${this.apiUrl}/${record.id}`, record);
  }

  getPlayerSeasonRecord(playerId: string, seasonId: string): Observable<PlayerSeasonRecord | null> {
    return this.http.get<PlayerSeasonRecord | null>(`${this.apiUrl}/player/${playerId}/season/${seasonId}`);
  }

  getSeasonRecordsByPlayer(playerId: string): Observable<PlayerSeasonRecord[]> {
    return this.http.get<PlayerSeasonRecord[]>(`${this.apiUrl}/player/${playerId}`);
  }

  getSeasonRecordsBySeason(seasonId: string): Observable<PlayerSeasonRecord[]> {
    return this.http.get<PlayerSeasonRecord[]>(`${this.apiUrl}/season/${seasonId}`);
  }

  deletePlayerSeasonRecord(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
