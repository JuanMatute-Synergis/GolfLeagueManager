import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScorecardData, HoleScore, Course } from '../../models/scorecard.model';
import { ScorecardService } from '../../services/scorecard.service';

@Component({
  selector: 'app-scorecard-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './scorecard-modal.component.html',
  styleUrls: ['./scorecard-modal.component.css']
})
export class ScorecardModalComponent implements OnInit, OnChanges {
  @Input() scorecardData!: ScorecardData;
  @Input() isOpen: boolean = false;
  @Output() save = new EventEmitter<ScorecardData>();
  @Output() close = new EventEmitter<void>();

  isLoading = false;
  saveError: string | null = null;

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
    // Component initialization
  }

  ngOnChanges(changes: SimpleChanges) {
    // Load scorecard data when modal opens
    if (changes['isOpen'] && changes['isOpen'].currentValue === true) {
      this.loadScorecardData();
    }
  }

  loadScorecardData() {
    if (!this.scorecardData.matchupId) {
      this.initializeHoles();
      return;
    }

    this.isLoading = true;
    this.scorecardService.getCompleteScorecard(this.scorecardData.matchupId).subscribe({
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
          // No existing scorecard, initialize with empty holes
          this.initializeHoles();
        }
      },
      error: (error) => {
        this.isLoading = false;
        console.log('No existing scorecard found, initializing empty scorecard');
        // If no scorecard exists, initialize with empty holes
        this.initializeHoles();
      }
    });
  }

  initializeHoles() {
    this.scorecardData.holes = this.course.holes.map(hole => ({
      hole: hole.number,
      par: hole.par,
      playerAScore: undefined,
      playerBScore: undefined
    }));
  }

  getFrontNine() {
    return this.course.holes.slice(0, 9);
  }

  getBackNine() {
    return this.course.holes.slice(9, 18);
  }

  getFrontNinePar(): number {
    return this.getFrontNine().reduce((sum, hole) => sum + hole.par, 0);
  }

  getBackNinePar(): number {
    return this.getBackNine().reduce((sum, hole) => sum + hole.par, 0);
  }

  getTotalPar(): number {
    return this.course.holes.reduce((sum, hole) => sum + hole.par, 0);
  }

  getPlayerFrontNineTotal(player: 'A' | 'B'): number {
    const scores = this.scorecardData.holes.slice(0, 9);
    return scores.reduce((sum, hole) => {
      const score = player === 'A' ? hole.playerAScore : hole.playerBScore;
      return sum + (score || 0);
    }, 0);
  }

  getPlayerBackNineTotal(player: 'A' | 'B'): number {
    const scores = this.scorecardData.holes.slice(9, 18);
    return scores.reduce((sum, hole) => {
      const score = player === 'A' ? hole.playerAScore : hole.playerBScore;
      return sum + (score || 0);
    }, 0);
  }

  calculateTotals() {
    // Calculate total scores
    this.scorecardData.playerATotalScore = this.scorecardData.holes.reduce((sum, hole) => {
      return sum + (hole.playerAScore || 0);
    }, 0);

    this.scorecardData.playerBTotalScore = this.scorecardData.holes.reduce((sum, hole) => {
      return sum + (hole.playerBScore || 0);
    }, 0);
  }

  onPlayerAbsenceChange(player: 'A' | 'B') {
    if (player === 'A') {
      if (this.scorecardData.playerAAbsent) {
        // Initialize absence notice option to false (no notice)
        this.scorecardData.playerAAbsentWithNotice = false;
        // Clear player A scores when marked as absent
        this.scorecardData.holes.forEach(hole => hole.playerAScore = undefined);
        this.scorecardData.playerATotalScore = 0;
      } else {
        // Clear absence flags when player is no longer marked as absent
        this.scorecardData.playerAAbsentWithNotice = false;
      }
    } else {
      if (this.scorecardData.playerBAbsent) {
        // Initialize absence notice option to false (no notice)
        this.scorecardData.playerBAbsentWithNotice = false;
        // Clear player B scores when marked as absent
        this.scorecardData.holes.forEach(hole => hole.playerBScore = undefined);
        this.scorecardData.playerBTotalScore = 0;
      } else {
        // Clear absence flags when player is no longer marked as absent
        this.scorecardData.playerBAbsentWithNotice = false;
      }
    }
  }

  getScoreClass(score: number | undefined, par: number): string {
    if (!score) return '';
    
    const toPar = score - par;
    
    if (toPar <= -2) return 'score-eagle';
    if (toPar === -1) return 'score-birdie';
    if (toPar === 0) return 'score-par';
    if (toPar === 1) return 'score-bogey';
    if (toPar === 2) return 'score-double-bogey';
    return 'score-worse';
  }

  getMatchResult(): string {
    if (!this.scorecardData.playerATotalScore || !this.scorecardData.playerBTotalScore) {
      return 'In Progress';
    }

    if (this.scorecardData.playerATotalScore < this.scorecardData.playerBTotalScore) {
      return `${this.scorecardData.playerAName} Wins`;
    } else if (this.scorecardData.playerBTotalScore < this.scorecardData.playerATotalScore) {
      return `${this.scorecardData.playerBName} Wins`;
    } else {
      return 'Tie';
    }
  }

  getMatchPlayResult(): string {
    // Handle absence scenarios
    if (this.scorecardData.playerAAbsent && this.scorecardData.playerBAbsent) {
      return 'Both Absent';
    }
    if (this.scorecardData.playerAAbsent) {
      return `${this.scorecardData.playerBName} Wins`;
    }
    if (this.scorecardData.playerBAbsent) {
      return `${this.scorecardData.playerAName} Wins`;
    }

    // Standard match play result
    const playerAPoints = this.scorecardData.playerAMatchPoints || 0;
    const playerBPoints = this.scorecardData.playerBMatchPoints || 0;

    if (playerAPoints === 0 && playerBPoints === 0) {
      return 'Not Yet Calculated';
    }

    if (playerAPoints > playerBPoints) {
      return `${this.scorecardData.playerAName} Wins`;
    } else if (playerBPoints > playerAPoints) {
      return `${this.scorecardData.playerBName} Wins`;
    } else {
      return 'Tie';
    }
  }

  getScoreDifference(): string {
    if (!this.scorecardData.playerATotalScore || !this.scorecardData.playerBTotalScore) {
      return '';
    }

    const diff = Math.abs(this.scorecardData.playerATotalScore - this.scorecardData.playerBTotalScore);
    return diff > 0 ? `by ${diff} strokes` : 'All Square';
  }

  getMatchPlayScore(): string {
    const playerAPoints = this.scorecardData.playerAMatchPoints || 0;
    const playerBPoints = this.scorecardData.playerBMatchPoints || 0;
    
    if (playerAPoints === 0 && playerBPoints === 0) {
      return 'Scores pending calculation';
    }
    
    return `${playerAPoints} - ${playerBPoints}`;
  }

  isValidScorecard(): boolean {
    // Check if at least some scores are entered
    return this.scorecardData.holes.some(hole => hole.playerAScore || hole.playerBScore);
  }

  onSave() {
    this.calculateTotals();
    this.isLoading = true;
    this.saveError = null;

    this.scorecardService.saveScorecard(this.scorecardData).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          // Update scorecard data with match play results from backend
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
          
          this.save.emit(this.scorecardData);
        } else {
          this.saveError = response.message || 'Failed to save scorecard';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.saveError = error.error?.message || 'An error occurred while saving the scorecard';
        console.error('Error saving scorecard:', error);
      }
    });
  }

  onClose() {
    this.close.emit();
  }

  onBackdropClick(event: Event) {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }
}
