export interface Week {
  id: string;
  weekNumber: number;
  date: string; // The Wednesday date when the week is played
  name: string;
  isActive: boolean;
  seasonId: string;
  countsForScoring: boolean; // Whether this week counts for scoring
  countsForHandicap: boolean; // Whether this week counts for handicap calculation
  season?: Season;
  scoreEntries?: ScoreEntry[];
  matchups?: BasicMatchup[];
}

export interface BasicMatchup {
  id: string;
  weekId: string;
  playerAId: string;
  playerBId: string;
  playerAScore?: number;
  playerBScore?: number;
}

export interface Season {
  id: string;
  name: string;
  year: number;
  startDate: string;
  endDate: string;
  seasonNumber: number;
}

export interface ScoreEntry {
  id: string;
  playerId: string;
  weekId: string;
  score: number;
  pointsEarned: number;
  player?: Player;
  week?: Week;
}

export interface Player {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  initialHandicap: number;
  currentHandicap: number;
  initialAverageScore: number;
  currentAverageScore: number;
}

export interface PlayerWithFlight extends Player {
  flightId?: string;
  flightName?: string;
  handicapAtAssignment?: number;
  isFlightLeader?: boolean;
  initialAverageScore: number;
  currentAverageScore: number;
}

export interface PlayerSeasonStats {
  playerId: string;
  playerName: string;
  totalPoints: number;
  averageScore: number;
  roundsPlayed: number;
  bestScore: number;
  worstScore: number;
}

export interface BulkScoreEntry {
  playerId: string;
  weekId: string;
  score: number | null;
  pointsEarned?: number;
}
