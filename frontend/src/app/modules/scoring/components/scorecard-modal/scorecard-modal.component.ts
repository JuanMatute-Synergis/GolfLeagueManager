import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScorecardData, HoleScore, Course } from '../../models/scorecard.model';
import { ScorecardService } from '../../services/scorecard.service';
import { CourseService } from '../../../../core/services/course.service';
import { Course as CourseModel } from '../../../../core/models/course.model';

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
  @Input() mode: 'edit' | 'view' = 'edit'; // New input to determine mode
  
  // For view mode, we can accept individual matchup properties as alternative inputs
  @Input() matchupId?: string;
  @Input() playerAName?: string;
  @Input() playerBName?: string;
  @Input() flightName?: string;
  
  @Output() save = new EventEmitter<ScorecardData>();
  @Output() close = new EventEmitter<void>();

  isLoading = false;
  saveError: string | null = null;
  error: string | null = null; // For view mode errors

  constructor(
    private scorecardService: ScorecardService,
    private courseService: CourseService
  ) {}

  // Default course - in real app this would come from a service
  course: Course = {
    name: "Allentown Municipal Golf Course",
    holes: [
      { number: 1, par: 4, yardage: 380, handicap: 3 },
      { number: 2, par: 3, yardage: 165, handicap: 11 },
      { number: 3, par: 5, yardage: 520, handicap: 1 },
      { number: 4, par: 4, yardage: 420, handicap: 5 },
      { number: 5, par: 3, yardage: 180, handicap: 17 },
      { number: 6, par: 4, yardage: 400, handicap: 7 },
      { number: 7, par: 5, yardage: 540, handicap: 13 },
      { number: 8, par: 4, yardage: 360, handicap: 9 },
      { number: 9, par: 4, yardage: 390, handicap: 15 },
      { number: 10, par: 4, yardage: 410, handicap: 12 },
      { number: 11, par: 3, yardage: 170, handicap: 4 },
      { number: 12, par: 5, yardage: 560, handicap: 2 },
      { number: 13, par: 4, yardage: 440, handicap: 6 },
      { number: 14, par: 3, yardage: 190, handicap: 14 },
      { number: 15, par: 4, yardage: 380, handicap: 10 },
      { number: 16, par: 5, yardage: 530, handicap: 8 },
      { number: 17, par: 4, yardage: 370, handicap: 18 },
      { number: 18, par: 4, yardage: 415, handicap: 16 }
    ]
  };

  ngOnInit() {
    console.log('ngOnInit called, initializing scorecard modal, mode:', this.mode);
    
    // Load course data first
    this.loadCourseData();
    
    // For view mode, create scorecardData from individual inputs if not provided
    if (this.mode === 'view' && !this.scorecardData && this.matchupId) {
      this.initializeViewModeData();
    }
    
    // Always ensure holes array is initialized to prevent template errors
    if (!this.scorecardData?.holes) {
      this.initializeHoles();
    }
    
    // Initialize the scorecard when component loads
    if (((this.scorecardData && this.mode === 'edit') || (this.matchupId && this.mode === 'view')) && this.isOpen) {
      this.loadScorecardData();
    }
  }

  private loadCourseData(): void {
    this.courseService.getAllCourses().subscribe({
      next: (courses) => {
        if (courses.length > 0) {
          // Use the first course (Allentown Municipal)
          const dbCourse = courses[0];
          this.course = {
            name: dbCourse.name,
            holes: dbCourse.courseHoles.map(hole => ({
              number: hole.holeNumber,
              par: hole.par,
              yardage: hole.yardage,
              handicap: hole.handicapIndex
            }))
          };
        }
      },
      error: (error) => {
        console.error('Error loading course data:', error);
        // Keep default course configuration as fallback
      }
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    console.log('ngOnChanges called:', changes);
    
    // For view mode, initialize data from individual inputs if needed
    if (this.mode === 'view' && (changes['matchupId'] || changes['isOpen']) && this.isOpen) {
      if (!this.scorecardData || this.scorecardData.matchupId !== this.matchupId) {
        this.initializeViewModeData();
      }
    }
    
    // Load scorecard data when modal opens
    if (changes['isOpen'] && changes['isOpen'].currentValue === true) {
      console.log('Modal opened, loading scorecard data');
      this.loadScorecardData();
    }
    
    // Recalculate when scorecard data changes (e.g., handicaps updated)
    if (changes['scorecardData'] && changes['scorecardData'].currentValue) {
      console.log('Scorecard data changed, recalculating totals');
      // Small delay to ensure all data is loaded
      setTimeout(() => {
        this.calculateTotals();
      }, 0);
    }
    
    // Also recalculate when any input changes that might affect calculations
    if ((changes['playerAName'] || changes['playerBName'] || changes['matchupId']) && this.isOpen) {
      console.log('Player or matchup data changed, recalculating');
      setTimeout(() => {
        if (this.scorecardData?.holes) {
          this.calculateTotals();
        }
      }, 100);
    }
  }

  loadScorecardData() {
    const matchupId = this.scorecardData?.matchupId || this.matchupId;
    console.log('loadScorecardData called with matchupId:', matchupId);
    console.log('Mode:', this.mode);
    console.log('Scorecard data at load:', this.scorecardData);
    
    if (!matchupId) {
      console.log('No matchupId, initializing holes');
      this.initializeHoles();
      return;
    }

    this.isLoading = true;
    this.error = null; // Clear any previous errors
    
    this.scorecardService.getCompleteScorecard(matchupId).subscribe({
      next: (response) => {
        console.log('Scorecard loaded from backend:', response);
        this.isLoading = false;
        if (response.success && response.holeScores && response.holeScores.length > 0) {
          // Ensure scorecardData exists for view mode
          if (this.mode === 'view' && !this.scorecardData) {
            this.initializeViewModeData();
          }
          
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
          
          // Calculate total scores and match play points
          this.calculateTotals();
          
          // Update match play data from backend response
          this.scorecardData.playerAMatchPoints = response.playerAMatchPoints || this.scorecardData.playerAMatchPoints;
          this.scorecardData.playerBMatchPoints = response.playerBMatchPoints || this.scorecardData.playerBMatchPoints;
          this.scorecardData.playerAHolePoints = response.playerAHolePoints || this.scorecardData.playerAHolePoints;
          this.scorecardData.playerBHolePoints = response.playerBHolePoints || this.scorecardData.playerBHolePoints;
          this.scorecardData.playerAMatchWin = response.playerAMatchWin || this.scorecardData.playerAMatchWin;
          this.scorecardData.playerBMatchWin = response.playerBMatchWin || this.scorecardData.playerBMatchWin;
          this.scorecardData.playerAAbsent = response.playerAAbsent;
          this.scorecardData.playerBAbsent = response.playerBAbsent;
          this.scorecardData.playerAAbsentWithNotice = response.playerAAbsentWithNotice;
          this.scorecardData.playerBAbsentWithNotice = response.playerBAbsentWithNotice;
          
          // Force another calculation after a short delay to ensure everything is updated
          setTimeout(() => {
            console.log('Final calculation after data load');
            this.calculateTotals();
          }, 50);
        } else {
          // No existing scorecard, initialize with empty holes
          if (this.mode === 'view') {
            this.error = 'No scorecard data available for this matchup.';
          } else {
            this.initializeHoles();
          }
        }
      },
      error: (error) => {
        this.isLoading = false;
        console.log('Error loading scorecard:', error);
        
        if (this.mode === 'view') {
          this.error = 'Failed to load scorecard data.';
        } else {
          console.log('No existing scorecard found, initializing empty scorecard');
          // If no scorecard exists, initialize with empty holes for edit mode
          this.initializeHoles();
        }
      }
    });
  }

  initializeHoles() {
    if (!this.scorecardData) {
      console.error('Cannot initialize holes: scorecardData is null');
      return;
    }

    this.scorecardData.holes = this.course.holes.map(hole => ({
      hole: hole.number,
      par: hole.par,
      playerAScore: undefined,
      playerBScore: undefined,
      holeHandicap: hole.handicap,
      playerAMatchPoints: 0,
      playerBMatchPoints: 0,
      playerANetScore: 0,
      playerBNetScore: 0,
      winner: undefined,
      strokesGiven: 0
    }));
    
    // Initialize match play data
    this.scorecardData.playerAMatchPoints = 0;
    this.scorecardData.playerBMatchPoints = 0;
    this.scorecardData.playerAHolePoints = 0;
    this.scorecardData.playerBHolePoints = 0;
    this.scorecardData.playerAMatchWin = false;
    this.scorecardData.playerBMatchWin = false;
    
    // Calculate totals (including match play points)
    this.calculateTotals();
  }

  // Initialize scorecard data for view mode from individual inputs
  private initializeViewModeData() {
    this.scorecardData = {
      matchupId: this.matchupId || '',
      playerAId: '', // Not needed for view mode
      playerBId: '', // Not needed for view mode
      playerAName: this.playerAName || '',
      playerBName: this.playerBName || '',
      flightName: this.flightName || 'Unknown Flight',
      holes: [],
      playerATotalScore: 0,
      playerBTotalScore: 0,
      playerAHolesWon: 0,
      playerBHolesWon: 0,
      holesHalved: 0,
      playerAHandicap: 0,
      playerBHandicap: 0
    };
    
    // Initialize holes to prevent template errors
    this.initializeHoles();
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
    if (!this.scorecardData?.holes) return 0;
    
    const scores = this.scorecardData.holes.slice(0, 9);
    return scores.reduce((sum, hole) => {
      if (!hole) return sum;
      const score = player === 'A' ? hole.playerAScore : hole.playerBScore;
      return sum + (score || 0);
    }, 0);
  }

  getPlayerBackNineTotal(player: 'A' | 'B'): number {
    if (!this.scorecardData?.holes) return 0;
    
    const scores = this.scorecardData.holes.slice(9, 18);
    return scores.reduce((sum, hole) => {
      if (!hole) return sum;
      const score = player === 'A' ? hole.playerAScore : hole.playerBScore;
      return sum + (score || 0);
    }, 0);
  }

  calculateTotals() {
    // Ensure holes array exists
    if (!this.scorecardData?.holes) {
      this.initializeHoles();
      return;
    }

    // Calculate total scores
    this.scorecardData.playerATotalScore = this.scorecardData.holes.reduce((sum, hole) => {
      return sum + (hole?.playerAScore || 0);
    }, 0);

    this.scorecardData.playerBTotalScore = this.scorecardData.holes.reduce((sum, hole) => {
      return sum + (hole?.playerBScore || 0);
    }, 0);

    // Debug logging
    console.log('calculateTotals called:', {
      playerATotalScore: this.scorecardData.playerATotalScore,
      playerBTotalScore: this.scorecardData.playerBTotalScore,
      playerAHandicap: this.scorecardData.playerAHandicap,
      playerBHandicap: this.scorecardData.playerBHandicap
    });

    // Calculate match play points for each hole
    this.calculateMatchPlayPoints();
  }

  private calculateMatchPlayPoints() {
    // Ensure holes array exists
    if (!this.scorecardData?.holes) {
      return;
    }

    let playerAHolePoints = 0;
    let playerBHolePoints = 0;

    console.log('calculateMatchPlayPoints called with handicaps:', {
      playerAHandicap: this.scorecardData.playerAHandicap,
      playerBHandicap: this.scorecardData.playerBHandicap
    });

    // Calculate points for each hole
    for (let i = 0; i < this.scorecardData.holes.length; i++) {
      const hole = this.scorecardData.holes[i];
      
      if (!hole) {
        continue; // Skip undefined holes
      }
      
      if (hole.playerAScore && hole.playerBScore) {
        const winner = this.getHoleWinner(i);
        
        console.log(`Hole ${i + 1}:`, {
          playerAScore: hole.playerAScore,
          playerBScore: hole.playerBScore,
          playerANet: this.getNetScoreValue(i, 'A'),
          playerBNet: this.getNetScoreValue(i, 'B'),
          winner: winner
        });
        
        switch (winner) {
          case 'playerA':
            hole.playerAMatchPoints = 2;
            hole.playerBMatchPoints = 0;
            playerAHolePoints += 2;
            break;
          case 'playerB':
            hole.playerAMatchPoints = 0;
            hole.playerBMatchPoints = 2;
            playerBHolePoints += 2;
            break;
          case 'tie':
            hole.playerAMatchPoints = 1;
            hole.playerBMatchPoints = 1;
            playerAHolePoints += 1;
            playerBHolePoints += 1;
            break;
          default:
            hole.playerAMatchPoints = 0;
            hole.playerBMatchPoints = 0;
        }
      } else {
        // No scores entered yet
        hole.playerAMatchPoints = 0;
        hole.playerBMatchPoints = 0;
      }
    }

    // Store hole points
    this.scorecardData.playerAHolePoints = playerAHolePoints;
    this.scorecardData.playerBHolePoints = playerBHolePoints;

    // Determine match play winner based on hole points and add 2-point bonus
    let playerAMatchPoints = playerAHolePoints;
    let playerBMatchPoints = playerBHolePoints;

    if (playerAHolePoints > playerBHolePoints) {
      // Player A wins the match - gets 2 bonus points
      playerAMatchPoints += 2;
      this.scorecardData.playerAMatchWin = true;
      this.scorecardData.playerBMatchWin = false;
    } else if (playerBHolePoints > playerAHolePoints) {
      // Player B wins the match - gets 2 bonus points
      playerBMatchPoints += 2;
      this.scorecardData.playerAMatchWin = false;
      this.scorecardData.playerBMatchWin = true;
    } else {
      // Tie - no bonus points
      this.scorecardData.playerAMatchWin = false;
      this.scorecardData.playerBMatchWin = false;
    }

    this.scorecardData.playerAMatchPoints = playerAMatchPoints;
    this.scorecardData.playerBMatchPoints = playerBMatchPoints;
  }

  onPlayerAbsenceChange(player: 'A' | 'B') {
    // Ensure scorecard data and holes exist
    if (!this.scorecardData) return;
    if (!this.scorecardData.holes) {
      this.initializeHoles();
    }

    if (player === 'A') {
      if (this.scorecardData.playerAAbsent) {
        // Initialize absence notice option to false (no notice)
        this.scorecardData.playerAAbsentWithNotice = false;
        // Clear player A scores when marked as absent
        this.scorecardData.holes.forEach(hole => {
          if (hole) hole.playerAScore = undefined;
        });
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
        this.scorecardData.holes.forEach(hole => {
          if (hole) hole.playerBScore = undefined;
        });
        this.scorecardData.playerBTotalScore = 0;
      } else {
        // Clear absence flags when player is no longer marked as absent
        this.scorecardData.playerBAbsentWithNotice = false;
      }
    }
  }

  getScoreClass(score: number | undefined, par: number): string {
    // Always include large font size with !important, even for empty inputs
    const baseClasses = '!text-xl !font-bold';
    
    if (!score) return baseClasses;
    
    const toPar = score - par;
    
    // Return Tailwind classes for score performance colors with consistent large font
    if (toPar <= -2) return `${baseClasses} bg-primary text-primary-foreground rounded px-1 py-0.5`;
    if (toPar === -1) return `${baseClasses} bg-primary/80 text-primary-foreground rounded px-1 py-0.5`;
    if (toPar === 0) return `${baseClasses} bg-muted text-foreground rounded px-1 py-0.5`;
    if (toPar === 1) return `${baseClasses} bg-destructive/70 text-destructive-foreground rounded px-1 py-0.5`;
    if (toPar === 2) return `${baseClasses} bg-destructive text-destructive-foreground rounded px-1 py-0.5`;
    return `${baseClasses} bg-destructive text-destructive-foreground rounded px-1 py-0.5`;
  }

  getNetScore(holeIndex: number, player: 'A' | 'B'): string {
    // Use safe hole data access
    const hole = this.getHoleData(holeIndex);
    const courseHole = this.course.holes[holeIndex];
    
    if (!hole || !courseHole) return '-';
    
    const grossScore = player === 'A' ? hole.playerAScore : hole.playerBScore;
    if (!grossScore) return '-';
    
    const playerHandicap = player === 'A' ? 
      (this.scorecardData?.playerAHandicap || 0) : 
      (this.scorecardData?.playerBHandicap || 0);
    
    // If handicap is 0, just show gross score
    if (playerHandicap === 0) {
      return grossScore.toString();
    }
    
    // Calculate strokes given on this hole
    const strokesGiven = this.getStrokesGiven(holeIndex, playerHandicap);
    const netScore = grossScore - strokesGiven;
    
    return netScore.toString();
  }

  getHoleWinner(holeIndex: number): 'playerA' | 'playerB' | 'tie' | null {
    // Use safe hole data access
    const hole = this.getHoleData(holeIndex);
    
    if (!hole || !hole.playerAScore || !hole.playerBScore) return null;
    
    // Get handicaps
    const playerAHandicap = this.scorecardData?.playerAHandicap || 0;
    const playerBHandicap = this.scorecardData?.playerBHandicap || 0;
    
    // If both players have 0 handicap, use gross scores
    if (playerAHandicap === 0 && playerBHandicap === 0) {
      if (hole.playerAScore < hole.playerBScore) return 'playerA';
      if (hole.playerBScore < hole.playerAScore) return 'playerB';
      return 'tie';
    }
    
    // Otherwise use net scores with handicap adjustments
    const playerANet = this.getNetScoreValue(holeIndex, 'A');
    const playerBNet = this.getNetScoreValue(holeIndex, 'B');
    
    if (playerANet < playerBNet) return 'playerA';
    if (playerBNet < playerANet) return 'playerB';
    return 'tie';
  }

  private getNetScoreValue(holeIndex: number, player: 'A' | 'B'): number {
    // Use safe hole data access
    const hole = this.getHoleData(holeIndex);
    
    if (!hole) return 999; // High number for missing scores
    
    const grossScore = player === 'A' ? hole.playerAScore : hole.playerBScore;
    if (!grossScore) return 999;
    
    const playerHandicap = player === 'A' ? 
      (this.scorecardData?.playerAHandicap || 0) : 
      (this.scorecardData?.playerBHandicap || 0);
    
    // If handicap is 0, return gross score
    if (playerHandicap === 0) {
      return grossScore;
    }
    
    const strokesGiven = this.getStrokesGiven(holeIndex, playerHandicap);
    return grossScore - strokesGiven;
  }

  private getStrokesGiven(holeIndex: number, playerHandicap: number): number {
    const courseHole = this.course.holes[holeIndex];
    if (!courseHole || !courseHole.handicap) return 0;
    
    // Standard golf handicap system: 
    // - If handicap is 18 or less, player gets 1 stroke on holes with handicap index <= player handicap
    // - If handicap is more than 18, player gets 1 stroke on all holes plus additional strokes
    const holeHandicap = courseHole.handicap;
    
    if (playerHandicap <= 0) return 0;
    
    let strokes = 0;
    
    // First round of strokes (1-18)
    if (playerHandicap >= holeHandicap) {
      strokes += 1;
    }
    
    // Second round of strokes (19-36)
    if (playerHandicap > 18 && (playerHandicap - 18) >= holeHandicap) {
      strokes += 1;
    }
    
    return strokes;
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

  // Public method to trigger scorecard initialization
  public initializeScorecard() {
    console.log('initializeScorecard called manually');
    this.loadScorecardData();
  }

  // Public method to trigger recalculation
  public recalculateScores() {
    console.log('recalculateScores called manually');
    this.calculateTotals();
  }

  // Helper methods for mode checking
  isEditMode(): boolean {
    return this.mode === 'edit';
  }

  isViewMode(): boolean {
    return this.mode === 'view';
  }

  // Helper method to get flight name
  getFlightName(): string {
    return this.scorecardData?.flightName || this.flightName || 'Unknown Flight';
  }

  // Helper method to safely get hole data
  getHoleData(index: number): HoleScore {
    if (!this.scorecardData?.holes || !this.scorecardData.holes[index]) {
      return {
        hole: index + 1,
        par: this.course.holes[index]?.par || 4,
        playerAScore: undefined,
        playerBScore: undefined,
        holeHandicap: this.course.holes[index]?.handicap || 1,
        playerAMatchPoints: 0,
        playerBMatchPoints: 0,
        playerANetScore: 0,
        playerBNetScore: 0,
        winner: undefined,
        strokesGiven: 0
      };
    }
    return this.scorecardData.holes[index];
  }

  // Helper method to safely get player score
  getPlayerScore(holeIndex: number, player: 'A' | 'B'): number | undefined {
    const hole = this.getHoleData(holeIndex);
    return player === 'A' ? hole.playerAScore : hole.playerBScore;
  }

  // Helper method to safely set player score
  setPlayerScore(holeIndex: number, player: 'A' | 'B', score: number | undefined): void {
    // Ensure holes array exists and has the right length
    if (!this.scorecardData?.holes) {
      this.initializeHoles();
    }
    
    // Ensure the specific hole exists
    if (!this.scorecardData.holes[holeIndex]) {
      this.scorecardData.holes[holeIndex] = this.getHoleData(holeIndex);
    }
    
    if (player === 'A') {
      this.scorecardData.holes[holeIndex].playerAScore = score;
    } else {
      this.scorecardData.holes[holeIndex].playerBScore = score;
    }
    
    this.calculateTotals();
  }

  // Helper method to safely get match points
  getMatchPoints(holeIndex: number, player: 'A' | 'B'): number {
    const hole = this.getHoleData(holeIndex);
    return player === 'A' ? (hole.playerAMatchPoints || 0) : (hole.playerBMatchPoints || 0);
  }
}
