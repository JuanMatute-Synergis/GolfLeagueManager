import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScorecardData, Course } from '../../../scoring/models/scorecard.model';
import { ScorecardService } from '../../../scoring/services/scorecard.service';

@Component({
  selector: 'app-scorecard-viewer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './scorecard-viewer.component.html',
  styleUrls: ['./scorecard-viewer.component.css']
})
export class ScorecardViewerComponent implements OnInit, OnChanges {
  @Input() matchupId!: string;
  @Input() playerAName!: string;
  @Input() playerBName!: string;
  @Input() flightName!: string;
  @Input() isOpen: boolean = false;
  @Output() close = new EventEmitter<void>();

  scorecardData!: ScorecardData;
  isLoading = false;
  error: string | null = null;

  constructor(private scorecardService: ScorecardService) {}

  // Default course - in real app this would come from a service
  course: Course = {
    name: "Championship Course",
    holes: [
      { number: 1, par: 4, yardage: 380, handicap: 10 },
      { number: 2, par: 3, yardage: 165, handicap: 16 },
      { number: 3, par: 5, yardage: 520, handicap: 2 },
      { number: 4, par: 4, yardage: 420, handicap: 6 },
      { number: 5, par: 3, yardage: 180, handicap: 14 },
      { number: 6, par: 4, yardage: 400, handicap: 8 },
      { number: 7, par: 5, yardage: 540, handicap: 4 },
      { number: 8, par: 4, yardage: 360, handicap: 12 },
      { number: 9, par: 4, yardage: 390, handicap: 18 },
      { number: 10, par: 4, yardage: 410, handicap: 9 },
      { number: 11, par: 3, yardage: 170, handicap: 15 },
      { number: 12, par: 5, yardage: 560, handicap: 1 },
      { number: 13, par: 4, yardage: 440, handicap: 5 },
      { number: 14, par: 3, yardage: 190, handicap: 13 },
      { number: 15, par: 4, yardage: 380, handicap: 7 },
      { number: 16, par: 5, yardage: 530, handicap: 3 },
      { number: 17, par: 4, yardage: 370, handicap: 11 },
      { number: 18, par: 4, yardage: 415, handicap: 17 }
    ]
  };

  ngOnInit() {
    this.initializeScorecardData();
  }

  ngOnChanges(changes: SimpleChanges) {
    // Load scorecard data when modal opens
    if (changes['isOpen'] && changes['isOpen'].currentValue === true) {
      this.loadScorecardData();
    }
  }

  private initializeScorecardData() {
    this.scorecardData = {
      matchupId: this.matchupId,
      playerAId: '',
      playerBId: '',
      playerAName: this.playerAName,
      playerBName: this.playerBName,
      flightName: this.flightName,
      holes: [],
      playerATotalScore: 0,
      playerBTotalScore: 0,
      playerAHolesWon: 0,
      playerBHolesWon: 0,
      holesHalved: 0
    };
  }

  loadScorecardData() {
    if (!this.matchupId) {
      this.error = 'No matchup ID provided';
      return;
    }

    this.isLoading = true;
    this.error = null;
    
    this.scorecardService.getCompleteScorecard(this.matchupId).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success && response.holeScores && response.holeScores.length > 0) {
          // Convert backend hole scores to frontend format
          this.scorecardData.holes = response.holeScores.map(hs => ({
            hole: hs.holeNumber,
            par: hs.par,
            playerAScore: hs.playerAScore,
            playerBScore: hs.playerBScore,
            holeHandicap: hs.holeHandicap,
            playerAMatchPoints: hs.playerAMatchPoints,
            playerBMatchPoints: hs.playerBMatchPoints
          }));
          
          // Calculate total scores
          this.calculateTotals();
          
          // Update match play data
          this.scorecardData.playerAMatchPoints = response.playerAMatchPoints;
          this.scorecardData.playerBMatchPoints = response.playerBMatchPoints;
          this.scorecardData.playerAHolePoints = response.playerAHolePoints;
          this.scorecardData.playerBHolePoints = response.playerBHolePoints;
          this.scorecardData.playerAMatchWin = response.playerAMatchWin;
          this.scorecardData.playerBMatchWin = response.playerBMatchWin;
          this.scorecardData.playerAAbsent = response.playerAAbsent;
          this.scorecardData.playerBAbsent = response.playerBAbsent;
          this.scorecardData.playerAAbsentWithNotice = response.playerAAbsentWithNotice;
          this.scorecardData.playerBAbsentWithNotice = response.playerBAbsentWithNotice;
        } else {
          this.error = 'No scorecard data found for this matchup';
        }
      },
      error: (error) => {
        console.error('Error loading scorecard:', error);
        this.isLoading = false;
        this.error = 'Failed to load scorecard data';
      }
    });
  }

  calculateTotals() {
    this.scorecardData.playerATotalScore = 0;
    this.scorecardData.playerBTotalScore = 0;
    this.scorecardData.playerAHolesWon = 0;
    this.scorecardData.playerBHolesWon = 0;
    this.scorecardData.holesHalved = 0;

    this.scorecardData.holes.forEach(hole => {
      if (hole.playerAScore) {
        this.scorecardData.playerATotalScore += hole.playerAScore;
      }
      if (hole.playerBScore) {
        this.scorecardData.playerBTotalScore += hole.playerBScore;
      }

      // Calculate hole wins
      if (hole.playerAScore && hole.playerBScore) {
        if (hole.playerAScore < hole.playerBScore) {
          this.scorecardData.playerAHolesWon++;
        } else if (hole.playerBScore < hole.playerAScore) {
          this.scorecardData.playerBHolesWon++;
        } else {
          this.scorecardData.holesHalved++;
        }
      }
    });
  }

  getFrontNine() {
    return this.course.holes.slice(0, 9);
  }

  getBackNine() {
    return this.course.holes.slice(9, 18);
  }

  getFrontNinePar(): number {
    return this.getFrontNine().reduce((total, hole) => total + hole.par, 0);
  }

  getBackNinePar(): number {
    return this.getBackNine().reduce((total, hole) => total + hole.par, 0);
  }

  getTotalPar(): number {
    return this.getFrontNinePar() + this.getBackNinePar();
  }

  getPlayerFrontNineTotal(player: 'A' | 'B'): number {
    const frontNineHoles = this.scorecardData.holes.slice(0, 9);
    return frontNineHoles.reduce((total, hole) => {
      const score = player === 'A' ? hole.playerAScore : hole.playerBScore;
      return total + (score || 0);
    }, 0);
  }

  getPlayerBackNineTotal(player: 'A' | 'B'): number {
    const backNineHoles = this.scorecardData.holes.slice(9, 18);
    return backNineHoles.reduce((total, hole) => {
      const score = player === 'A' ? hole.playerAScore : hole.playerBScore;
      return total + (score || 0);
    }, 0);
  }

  getScoreDisplay(score: number | undefined): string {
    return score ? score.toString() : '-';
  }

  onClose() {
    this.close.emit();
  }

  onBackdropClick(event: Event) {
    // Close modal when clicking on backdrop
    this.onClose();
  }
}
