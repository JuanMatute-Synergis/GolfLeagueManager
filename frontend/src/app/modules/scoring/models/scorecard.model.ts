export interface HoleScore {
  hole: number;
  par: number;
  playerAScore?: number;
  playerBScore?: number;
  playerAHolesWon?: number;
  playerBHolesWon?: number;
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
