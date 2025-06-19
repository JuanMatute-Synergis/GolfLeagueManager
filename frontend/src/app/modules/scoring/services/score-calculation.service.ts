import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ScoreCalculationService {
  private readonly apiUrl = 'http://localhost:5274/api/score-calculation';

  constructor(private http: HttpClient) {}

  /**
   * Calculate net score for a specific hole
   */
  calculateNetScore(request: NetScoreRequest): Observable<NetScoreResponse> {
    return this.http.post<NetScoreResponse>(`${this.apiUrl}/net-score`, request);
  }

  /**
   * Calculate match play results for hole scores
   */
  calculateMatchPlay(request: MatchPlayCalculationRequest): Observable<MatchPlayCalculationResponse> {
    return this.http.post<MatchPlayCalculationResponse>(`${this.apiUrl}/match-play`, request);
  }

  /**
   * Get all calculated scores for a matchup
   */
  getMatchupScores(matchupId: string): Observable<MatchupScoresResponse> {
    return this.http.get<MatchupScoresResponse>(`${this.apiUrl}/matchup/${matchupId}`);
  }
}

// Request/Response interfaces
export interface NetScoreRequest {
  grossScore: number;
  handicap: number;
  opponentHandicap: number;
  holeHandicap: number;
}

export interface NetScoreResponse {
  netScore: number;
  strokesReceived: number;
}

export interface MatchPlayCalculationRequest {
  holeScores: HoleScoreInput[];
  playerAHandicap: number;
  playerBHandicap: number;
}

export interface HoleScoreInput {
  holeNumber: number;
  playerAScore?: number;
  playerBScore?: number;
}

export interface MatchPlayCalculationResponse {
  playerAHolePoints: number;
  playerBHolePoints: number;
  playerATotalPoints: number;
  playerBTotalPoints: number;
  playerAMatchWin: boolean;
  playerBMatchWin: boolean;
  holeResults: HoleResultDto[];
}

export interface HoleResultDto {
  holeNumber: number;
  playerAPoints: number;
  playerBPoints: number;
  winner: string;
}

export interface MatchupScoresResponse {
  matchupId: string;
  playerAGrossScore?: number;
  playerBGrossScore?: number;
  playerANetScore?: number;
  playerBNetScore?: number;
  playerAHandicap: number;
  playerBHandicap: number;
  playerAPoints?: number;
  playerBPoints?: number;
  holeScores: HoleScoreDto[];
}

export interface HoleScoreDto {
  holeNumber: number;
  playerAGrossScore?: number;
  playerBGrossScore?: number;
  playerANetScore?: number;
  playerBNetScore?: number;
  playerAMatchPoints?: number;
  playerBMatchPoints?: number;
}
