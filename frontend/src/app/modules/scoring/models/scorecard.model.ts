export interface HoleScore {
  hole: number;
  par: number;
  playerAScore?: number;
  playerBScore?: number;
  playerAHolesWon?: number;
  playerBHolesWon?: number;
  // Match play fields
  holeHandicap?: number; // Stroke index (1-9)
  playerAMatchPoints?: number; // 0, 1, or 2 points for this hole
  playerBMatchPoints?: number; // 0, 1, or 2 points for this hole
  playerANetScore?: number; // Net score after handicap
  playerBNetScore?: number; // Net score after handicap
}

export interface ScorecardData {
  matchupId: string;
  playerAId: string;
  playerBId: string;
  playerAName: string;
  playerBName: string;
  flightName: string;
  holes: HoleScore[];
  playerATotalScore: number;
  playerBTotalScore: number;
  playerAHolesWon: number;
  playerBHolesWon: number;
  holesHalved: number;
  // Match play fields
  playerAHolePoints?: number; // Points from individual holes (0-18)
  playerBHolePoints?: number; // Points from individual holes (0-18)
  playerAMatchPoints?: number; // Total match play points (0-20)
  playerBMatchPoints?: number; // Total match play points (0-20)
  playerAMatchWin?: boolean; // Player A won the overall match
  playerBMatchWin?: boolean; // Player B won the overall match
  playerAHandicap?: number;
  playerBHandicap?: number;
  // Absence scenarios
  playerAAbsent?: boolean;
  playerBAbsent?: boolean;
  playerAAbsentWithNotice?: boolean;
  playerBAbsentWithNotice?: boolean;
}

export interface Course {
  name: string;
  holes: CourseHole[];
}

export interface CourseHole {
  number: number;
  par: number;
  yardage?: number;
  handicap?: number;
}
