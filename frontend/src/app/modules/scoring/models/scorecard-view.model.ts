// View models for scorecard UI binding
export interface ScorecardHoleView {
  holeNumber: number;
  par: number;
  handicap: number;
  
  // Player A data
  playerAScore?: number;
  playerAScoreClass: string;
  playerANetScore: string;
  playerAMatchPoints: number;
  playerAIsStrokeHole: boolean;
  playerAStrokeTooltip: string;
  
  // Player B data
  playerBScore?: number;
  playerBScoreClass: string;
  playerBNetScore: string;
  playerBMatchPoints: number;
  playerBIsStrokeHole: boolean;
  playerBStrokeTooltip: string;
  
  // Hole result
  holeWinner: 'playerA' | 'playerB' | 'tie' | null;
}

export interface ScorecardPlayerView {
  name: string;
  handicap: number;
  totalScore: number;
  matchPoints: number;
  holePoints: number;
  matchWin: boolean;
  absent: boolean;
  absentWithNotice: boolean;
  strokeCount: number;
  isStrokeRecipient: boolean;
}

export interface ScorecardSummaryView {
  totalPar: number;
  matchPlayResult: string;
  matchPlayScore: string;
  strokePlayResult: string;
  scoreDifference: string;
  flightName: string;
  courseName: string;
}

export interface ScorecardViewModel {
  // Core data
  matchupId: string;
  
  // Players
  playerA: ScorecardPlayerView;
  playerB: ScorecardPlayerView;
  
  // Holes
  holes: ScorecardHoleView[];
  
  // Summary
  summary: ScorecardSummaryView;
  
  // State
  isLoading: boolean;
  error: string | null;
  saveError: string | null;
}
