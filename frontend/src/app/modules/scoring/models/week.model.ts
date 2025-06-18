export interface Week {
  id: string;
  weekNumber: number;
  date: string; // The Wednesday date when the week is played
  name: string;
  isActive: boolean;
  seasonId: string;
  season?: Season;
  scoreEntries?: ScoreEntry[];
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
}

export interface PlayerWithFlight extends Player {
  flightId?: string;
  flightName?: string;
  handicapAtAssignment?: number;
  isFlightLeader?: boolean;
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
