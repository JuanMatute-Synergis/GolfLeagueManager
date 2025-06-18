import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ScorecardData } from '../models/scorecard.model';

export interface ScorecardSaveRequest {
  matchupId: string;
  holeScores: HoleScoreDto[];
  playerATotalScore: number;
  playerBTotalScore: number;
  // Absence scenario fields
  playerAAbsent?: boolean;
  playerBAbsent?: boolean;
  playerAAbsentWithNotice?: boolean;
  playerBAbsentWithNotice?: boolean;
}

export interface HoleScoreDto {
  holeNumber: number;
  par: number;
  playerAScore?: number;
  playerBScore?: number;
}

export interface ScorecardResponse {
  matchupId: string;
  success: boolean;
  message: string;
  holeScores?: any[];
  // Match play results
  playerAMatchPoints?: number;
  playerBMatchPoints?: number;
  playerAHolePoints?: number;
  playerBHolePoints?: number;
  playerAMatchWin?: boolean;
  playerBMatchWin?: boolean;
  // Player handicaps
  playerAHandicap?: number;
  playerBHandicap?: number;
  // Absence status
  playerAAbsent?: boolean;
  playerBAbsent?: boolean;
  playerAAbsentWithNotice?: boolean;
  playerBAbsentWithNotice?: boolean;
}

export interface HoleScoreBackend {
  id: string;
  matchupId: string;
  holeNumber: number;
  par: number;
  playerAScore?: number;
  playerBScore?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ScorecardService {
  private readonly apiUrl = 'http://localhost:5274/api/scorecard';

  constructor(private http: HttpClient) { }

  saveScorecard(scorecardData: ScorecardData): Observable<ScorecardResponse> {
    const request: ScorecardSaveRequest = {
      matchupId: scorecardData.matchupId,
      playerATotalScore: scorecardData.playerATotalScore || 0,
      playerBTotalScore: scorecardData.playerBTotalScore || 0,
      playerAAbsent: scorecardData.playerAAbsent || false,
      playerBAbsent: scorecardData.playerBAbsent || false,
      playerAAbsentWithNotice: scorecardData.playerAAbsentWithNotice || false,
      playerBAbsentWithNotice: scorecardData.playerBAbsentWithNotice || false,
      holeScores: scorecardData.holes.map(hole => ({
        holeNumber: hole.hole,
        par: hole.par,
        playerAScore: hole.playerAScore,
        playerBScore: hole.playerBScore
      }))
    };

    return this.http.post<ScorecardResponse>(`${this.apiUrl}/save`, request);
  }

  getScorecard(matchupId: string): Observable<HoleScoreBackend[]> {
    return this.http.get<HoleScoreBackend[]>(`${this.apiUrl}/${matchupId}`);
  }

  getCompleteScorecard(matchupId: string): Observable<ScorecardResponse> {
    return this.http.get<ScorecardResponse>(`${this.apiUrl}/${matchupId}/complete`);
  }

  deleteScorecard(matchupId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${matchupId}`);
  }

  // Convert backend hole scores to frontend scorecard format
  convertToScorecardData(
    holeScores: HoleScoreBackend[], 
    matchupId: string,
    playerAId: string,
    playerBId: string,
    playerAName: string,
    playerBName: string,
    flightName: string
  ): ScorecardData {
    const holes = holeScores.map(hs => ({
      hole: hs.holeNumber,
      par: hs.par,
      playerAScore: hs.playerAScore,
      playerBScore: hs.playerBScore
    }));

    const playerATotalScore = holes.reduce((sum, hole) => sum + (hole.playerAScore || 0), 0);
    const playerBTotalScore = holes.reduce((sum, hole) => sum + (hole.playerBScore || 0), 0);

    return {
      matchupId,
      playerAId,
      playerBId,
      playerAName,
      playerBName,
      flightName,
      holes,
      playerATotalScore,
      playerBTotalScore,
      playerAHolesWon: 0, // These would need to be calculated based on match play rules
      playerBHolesWon: 0,
      holesHalved: 0
    };
  }
}
