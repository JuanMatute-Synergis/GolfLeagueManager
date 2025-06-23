import { Component, Input, Output, EventEmitter, OnInit, OnChanges, OnDestroy, SimpleChanges, ViewChildren, QueryList, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScorecardData, HoleScore, Course } from '../../models/scorecard.model';
import { ScorecardViewModel, ScorecardHoleView, ScorecardPlayerView, ScorecardSummaryView } from '../../models/scorecard-view.model';
import { ScorecardService } from '../../services/scorecard.service';
import { CourseService } from '../../../../core/services/course.service';
import { ScoreCalculationService } from '../../services/score-calculation.service';
import { Course as CourseModel } from '../../../../core/models/course.model';

@Component({
  selector: 'app-scorecard-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './scorecard-modal.component.html',
  styleUrls: ['./scorecard-modal.component.css']
})
export class ScorecardModalComponent implements OnInit, OnChanges, OnDestroy, AfterViewInit {
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

  // View model for template binding
  viewModel: ScorecardViewModel = this.createEmptyViewModel();

  constructor(
    private scorecardService: ScorecardService,
    private courseService: CourseService,
    private scoreCalculationService: ScoreCalculationService
  ) {}

  // Convenience getters for backward compatibility
  get isLoading(): boolean { return this.viewModel.isLoading; }
  set isLoading(value: boolean) { this.viewModel.isLoading = value; }
  
  get error(): string | null { return this.viewModel.error; }
  set error(value: string | null) { this.viewModel.error = value; }
  
  get saveError(): string | null { return this.viewModel.saveError; }
  set saveError(value: string | null) { this.viewModel.saveError = value; }

  // Default course - updated to match current database values
  course: Course = {
    name: "Allentown Municipal Golf Course",
    holes: [
      { number: 1, par: 4, yardage: 380, handicap: 3 },
      { number: 2, par: 4, yardage: 165, handicap: 11 },
      { number: 3, par: 5, yardage: 520, handicap: 1 },
      { number: 4, par: 4, yardage: 420, handicap: 5 },
      { number: 5, par: 3, yardage: 180, handicap: 17 },
      { number: 6, par: 5, yardage: 400, handicap: 7 },
      { number: 7, par: 4, yardage: 540, handicap: 13 },
      { number: 8, par: 4, yardage: 360, handicap: 9 },
      { number: 9, par: 3, yardage: 390, handicap: 15 },
      { number: 10, par: 4, yardage: 410, handicap: 2 },
      { number: 11, par: 3, yardage: 170, handicap: 12 },
      { number: 12, par: 4, yardage: 560, handicap: 4 },
      { number: 13, par: 5, yardage: 440, handicap: 16 },
      { number: 14, par: 4, yardage: 190, handicap: 6 },
      { number: 15, par: 4, yardage: 380, handicap: 14 },
      { number: 16, par: 3, yardage: 530, handicap: 10 },
      { number: 17, par: 4, yardage: 370, handicap: 8 },
      { number: 18, par: 5, yardage: 415, handicap: 18 }
    ]
  };

  // ViewChildren to access all score input elements - simplified approach
  @ViewChildren('scoreInput') scoreInputs!: QueryList<ElementRef<HTMLInputElement>>;

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
    
    // Build the view model
    this.buildViewModel();
    
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

  ngAfterViewInit(): void {
    // ViewChildren are now available
    console.log('ngAfterViewInit called - score inputs available:', this.scoreInputs?.length);
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
          // Always display all 18 holes, assign scores to correct hole number
          const holesArray = this.course.holes.map(hole => {
            const hs = (response.holeScores ?? []).find(h => h.holeNumber === hole.number);
            return {
              hole: hole.number,
              par: hole.par,
              playerAScore: hs ? hs.playerAScore : undefined,
              playerBScore: hs ? hs.playerBScore : undefined,
              holeHandicap: hs ? hs.holeHandicap : hole.handicap,
              playerAMatchPoints: hs ? hs.playerAMatchPoints : 0,
              playerBMatchPoints: hs ? hs.playerBMatchPoints : 0
            };
          });
          this.scorecardData.holes = holesArray;
          
          // Update absence status FIRST before calculating totals
          this.scorecardData.playerAAbsent = response.playerAAbsent;
          this.scorecardData.playerBAbsent = response.playerBAbsent;
          this.scorecardData.playerAAbsentWithNotice = response.playerAAbsentWithNotice;
          this.scorecardData.playerBAbsentWithNotice = response.playerBAbsentWithNotice;
          
          // Update match play data from backend response BEFORE calculating totals
          this.scorecardData.playerAMatchPoints = response.playerAMatchPoints ?? 0;
          this.scorecardData.playerBMatchPoints = response.playerBMatchPoints ?? 0;
          this.scorecardData.playerAHolePoints = response.playerAHolePoints ?? 0;
          this.scorecardData.playerBHolePoints = response.playerBHolePoints ?? 0;
          this.scorecardData.playerAMatchWin = response.playerAMatchWin ?? false;
          this.scorecardData.playerBMatchWin = response.playerBMatchWin ?? false;
          
          // Calculate total scores and match play points AFTER setting backend values
          this.calculateTotals();
          
          console.log('[DEBUG] Backend response match points:', {
            playerAMatchPoints: response.playerAMatchPoints,
            playerBMatchPoints: response.playerBMatchPoints,
            playerAAbsent: response.playerAAbsent,
            playerBAbsent: response.playerBAbsent
          });
          console.log('[DEBUG] Scorecard data after assignment:', {
            playerAMatchPoints: this.scorecardData.playerAMatchPoints,
            playerBMatchPoints: this.scorecardData.playerBMatchPoints
          });
          
          // Update player handicaps from backend response
          this.scorecardData.playerAHandicap = response.playerAHandicap;
          this.scorecardData.playerBHandicap = response.playerBHandicap;
          
          // Rebuild view model with updated backend data
          this.buildViewModel();
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
      playerBHandicap: this.scorecardData.playerBHandicap,
      isAbsenceScenario: this.scorecardData.playerAAbsent || this.scorecardData.playerBAbsent
    });

    // For absence scenarios, trust the backend calculation and skip local match play calculation
    if (this.scorecardData.playerAAbsent || this.scorecardData.playerBAbsent) {
      console.log('[DEBUG] Absence scenario detected, skipping local match play calculation');
      console.log('[DEBUG] Backend points preserved:', {
        playerAMatchPoints: this.scorecardData.playerAMatchPoints,
        playerBMatchPoints: this.scorecardData.playerBMatchPoints,
        playerAAbsent: this.scorecardData.playerAAbsent,
        playerBAbsent: this.scorecardData.playerBAbsent,
        playerAAbsentWithNotice: this.scorecardData.playerAAbsentWithNotice,
        playerBAbsentWithNotice: this.scorecardData.playerBAbsentWithNotice
      });
      
      // Alert for testing - can be removed once confirmed working
      console.warn('🚨 ABSENCE SCENARIO POINTS TEST: Player A=' + this.scorecardData.playerAMatchPoints + ', Player B=' + this.scorecardData.playerBMatchPoints);
    } else {
      // Normal scenario - calculate match play points for each hole
      this.calculateMatchPlayPoints();
    }
    
    // Rebuild view model after calculations
    this.buildViewModel();
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

    // Calculate total net scores for both players
    let playerANetTotal = 0;
    let playerBNetTotal = 0;
    
    for (let i = 0; i < this.scorecardData.holes.length; i++) {
      const hole = this.scorecardData.holes[i];
      if (hole?.playerAScore && hole?.playerBScore) {
        playerANetTotal += this.getNetScoreValue(i, 'A');
        playerBNetTotal += this.getNetScoreValue(i, 'B');
      }
    }

    // Determine match play winner based on lowest total net score and award 2-point bonus
    let playerAMatchPoints = playerAHolePoints;
    let playerBMatchPoints = playerBHolePoints;

    if (playerANetTotal < playerBNetTotal) {
      // Player A has lower net total - wins match
      playerAMatchPoints += 2;
      this.scorecardData.playerAMatchWin = true;
      this.scorecardData.playerBMatchWin = false;
    } else if (playerBNetTotal < playerANetTotal) {
      // Player B has lower net total - wins match
      playerBMatchPoints += 2;
      this.scorecardData.playerAMatchWin = false;
      this.scorecardData.playerBMatchWin = true;
    } else {
      // Tie in net total scores - each player gets 1 point instead of 2-point bonus
      playerAMatchPoints += 1;
      playerBMatchPoints += 1;
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
        // Don't automatically set AbsentWithNotice to false here
        // Let the user choose and preserve their choice
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
        // Don't automatically set AbsentWithNotice to false here
        // Let the user choose and preserve their choice
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

    // Don't automatically save here - let the user set "with notice" first if they want
    // The save will happen when they click the save button
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
    const opponentHandicap = player === 'A' ? 
      (this.scorecardData?.playerBHandicap || 0) : 
      (this.scorecardData?.playerAHandicap || 0);
    
    const holeHandicap = courseHole.handicap || 1;
    
    // Use backend calculation for net score
    this.scoreCalculationService.calculateNetScore({
      grossScore: grossScore,
      handicap: playerHandicap,
      opponentHandicap: opponentHandicap,
      holeHandicap: holeHandicap
    }).subscribe({
      next: (response) => {
        // Store the calculated net score for display
        // Since this is asynchronous, we need to handle it differently
        // For now, fall back to simple calculation to maintain sync behavior
      },
      error: (error) => {
        console.error('Error calculating net score:', error);
      }
    });

    // Fallback synchronous calculation to maintain UI responsiveness
    // This should match the backend logic
    if (playerHandicap <= opponentHandicap) {
      return grossScore.toString(); // No strokes for equal or lower handicap
    }

    const handicapDifference = playerHandicap - opponentHandicap;
    const strokesReceived = holeHandicap <= handicapDifference ? 1 : 0;
    const netScore = grossScore - strokesReceived;
    
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
    const playerAHandicap = this.scorecardData?.playerAHandicap || 0;
    const playerBHandicap = this.scorecardData?.playerBHandicap || 0;
    const handicapDifference = Math.abs(playerAHandicap - playerBHandicap);
    if (handicapDifference === 0) return 0;
    const playerAReceivesStrokes = playerAHandicap > playerBHandicap;
    const playerBReceivesStrokes = playerBHandicap > playerAHandicap;
    const playerReceivesStrokes = (playerHandicap === playerAHandicap && playerAReceivesStrokes) ||
                                 (playerHandicap === playerBHandicap && playerBReceivesStrokes);
    if (!playerReceivesStrokes) return 0;
    // --- 9-hole stroke allocation fix ---
    // Only allocate strokes to the hardest holes within the 9 being played
    const holesInPlay = this.course.holes.slice(0, 9); // assumes 9-hole match, holes 1-9 or adjust as needed
    const holesWithHandicap = holesInPlay.filter(h => typeof h.handicap === 'number');
    const hardestHoles = [...holesWithHandicap]
      .sort((a, b) => (a.handicap ?? 99) - (b.handicap ?? 99))
      .slice(0, Math.round(handicapDifference))
      .map(h => h.number);
    if (hardestHoles.includes(courseHole.number)) return 1;
    return 0;
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
    const playerAHolePoints = this.scorecardData.playerAHolePoints || 0;
    const playerBHolePoints = this.scorecardData.playerBHolePoints || 0;
    const playerAMatchPoints = this.scorecardData.playerAMatchPoints || 0;
    const playerBMatchPoints = this.scorecardData.playerBMatchPoints || 0;

    if (playerAMatchPoints === 0 && playerBMatchPoints === 0) {
      return 'Not Yet Calculated';
    }

    // Check if there was a tie in net scores (both players have matchWin = false but no one wins overall)
    const wasNetScoreTie = !this.scorecardData.playerAMatchWin && !this.scorecardData.playerBMatchWin && 
                          (playerAMatchPoints > 0 || playerBMatchPoints > 0);

    if (playerAMatchPoints > playerBMatchPoints) {
      const result = `${this.scorecardData.playerAName} Wins`;
      if (wasNetScoreTie) {
        return `${result} (Tied Net Scores, Won More Holes)`;
      } else if (this.scorecardData.playerAMatchWin) {
        return `${result} (Lower Net Score)`;
      }
      return result;
    } else if (playerBMatchPoints > playerAMatchPoints) {
      const result = `${this.scorecardData.playerBName} Wins`;
      if (wasNetScoreTie) {
        return `${result} (Tied Net Scores, Won More Holes)`;
      } else if (this.scorecardData.playerBMatchWin) {
        return `${result} (Lower Net Score)`;
      }
      return result;
    } else {
      return wasNetScoreTie ? 'Tie (Equal Net Scores & Hole Points)' : 'Tie';
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
    // Check if any player is marked absent (makes scorecard valid for saving)
    if (this.scorecardData.playerAAbsent || this.scorecardData.playerBAbsent) {
      return true;
    }
    
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

  // Public method to trigger recalculation using backend services
  public recalculateScores() {
    console.log('recalculateScores called - now using backend calculations');
    // Instead of frontend calculations, the display will use backend-calculated values
    // from the saved scorecard data which is already calculated on the server
    this.buildViewModel();
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

  // Stroke allocation methods
  getStrokeHoles(): number[] {
    const handicapDiff = Math.abs((this.scorecardData.playerAHandicap || 0) - (this.scorecardData.playerBHandicap || 0));
    if (handicapDiff === 0) return [];

    // Debug logging
    console.log('Calculating stroke holes:', {
      playerAHandicap: this.scorecardData.playerAHandicap,
      playerBHandicap: this.scorecardData.playerBHandicap,
      handicapDiff: handicapDiff
    });

    // Determine which 9 holes to consider based on which holes have scores
    const playingFront = this.isPlayingFrontNine();
    const holes = playingFront ? this.course.holes.slice(0, 9) : this.course.holes.slice(9, 18);
    
    console.log('Playing front nine:', playingFront, 'holes considered:', holes.map(h => h.number));
    
    // Sort holes by handicap index (1 = hardest)
    const sortedHoles = holes
      .map((hole, index) => ({ 
        holeNumber: hole.number, 
        handicapIndex: hole.handicap || 18,
        arrayIndex: playingFront ? index : index + 9
      }))
      .sort((a, b) => a.handicapIndex - b.handicapIndex);
    
    // Return the hole numbers that get strokes (based on handicap difference)
    const strokeHoles = sortedHoles.slice(0, handicapDiff).map(h => h.holeNumber);
    console.log('Stroke holes calculated:', strokeHoles);
    
    return strokeHoles;
  }

  getStrokeRecipient(): 'A' | 'B' | null {
    const playerAHandicap = this.scorecardData.playerAHandicap || 0;
    const playerBHandicap = this.scorecardData.playerBHandicap || 0;
    
    if (playerAHandicap > playerBHandicap) return 'A';
    if (playerBHandicap > playerAHandicap) return 'B';
    return null;
  }

  isStrokeHole(holeNumber: number, player: 'A' | 'B'): boolean {
    const strokeRecipient = this.getStrokeRecipient();
    const strokeHoles = this.getStrokeHoles();
    
    const result = strokeRecipient === player && strokeHoles.includes(holeNumber);
    
    // Debug logging for specific holes
    if (holeNumber <= 3) {
      console.log(`isStrokeHole(${holeNumber}, ${player}):`, {
        strokeRecipient,
        strokeHoles,
        result
      });
    }
    
    return result;
  }

  private isPlayingFrontNine(): boolean {
    // Check if there are more scores in front nine vs back nine
    const frontScores = this.scorecardData.holes.slice(0, 9).filter(hole => 
      (hole.playerAScore && hole.playerAScore > 0) || (hole.playerBScore && hole.playerBScore > 0)
    ).length;
    
    const backScores = this.scorecardData.holes.slice(9, 18).filter(hole => 
      (hole.playerAScore && hole.playerAScore > 0) || (hole.playerBScore && hole.playerBScore > 0)
    ).length;
    
    // Default to front nine if no scores yet
    return frontScores >= backScores;
  }

  getStrokeTooltip(holeNumber: number, player: 'A' | 'B'): string {
    if (!this.isStrokeHole(holeNumber, player)) return '';
    
    const handicapDiff = Math.abs((this.scorecardData.playerAHandicap || 0) - (this.scorecardData.playerBHandicap || 0));
    const playerName = player === 'A' ? this.scorecardData.playerAName : this.scorecardData.playerBName;
    
    return `${playerName} gets a stroke on this hole (handicap difference: ${handicapDiff})`;
  }

  private createEmptyViewModel(): ScorecardViewModel {
    return {
      matchupId: '',
      playerA: this.createEmptyPlayerView('A'),
      playerB: this.createEmptyPlayerView('B'),
      holes: [],
      summary: this.createEmptySummaryView(),
      isLoading: false,
      error: null,
      saveError: null
    };
  }

  private createEmptyPlayerView(player: 'A' | 'B'): ScorecardPlayerView {
    return {
      name: '',
      handicap: 0,
      totalScore: 0,
      matchPoints: 0,
      holePoints: 0,
      matchWin: false,
      absent: false,
      absentWithNotice: false,
      strokeCount: 0,
      isStrokeRecipient: false
    };
  }

  private createEmptySummaryView(): ScorecardSummaryView {
    return {
      totalPar: 0,
      matchPlayResult: '',
      matchPlayScore: '',
      strokePlayResult: '',
      scoreDifference: '',
      flightName: '',
      courseName: ''
    };
  }

  // Build view model from scorecard data
  private buildViewModel(): void {
    console.log('[DEBUG] buildViewModel called with match points:', {
      playerAMatchPoints: this.scorecardData?.playerAMatchPoints,
      playerBMatchPoints: this.scorecardData?.playerBMatchPoints,
      playerAAbsent: this.scorecardData?.playerAAbsent,
      playerBAbsent: this.scorecardData?.playerBAbsent
    });
    
    if (!this.scorecardData) {
      this.viewModel = this.createEmptyViewModel();
      return;
    }

    // Build player view models
    this.viewModel.playerA = this.buildPlayerViewModel('A');
    this.viewModel.playerB = this.buildPlayerViewModel('B');

    // Build hole view models
    this.viewModel.holes = this.course.holes.map((courseHole, index) => 
      this.buildHoleViewModel(courseHole, index)
    );

    // Build summary view model
    this.viewModel.summary = this.buildSummaryViewModel();

    // Set metadata
    this.viewModel.matchupId = this.scorecardData.matchupId || '';
  }

  private buildPlayerViewModel(player: 'A' | 'B'): ScorecardPlayerView {
    const isPlayerA = player === 'A';
    const handicapDiff = Math.abs((this.scorecardData.playerAHandicap || 0) - (this.scorecardData.playerBHandicap || 0));
    const strokeRecipient = this.getStrokeRecipient();
    
    const matchPoints = isPlayerA ? (this.scorecardData.playerAMatchPoints || 0) : (this.scorecardData.playerBMatchPoints || 0);
    
    console.log(`[DEBUG] Building view model for player ${player}:`, {
      matchPoints: matchPoints,
      scorecardDataMatchPoints: isPlayerA ? this.scorecardData.playerAMatchPoints : this.scorecardData.playerBMatchPoints
    });
    
    return {
      name: isPlayerA ? this.scorecardData.playerAName : this.scorecardData.playerBName,
      handicap: isPlayerA ? (this.scorecardData.playerAHandicap || 0) : (this.scorecardData.playerBHandicap || 0),
      totalScore: isPlayerA ? (this.scorecardData.playerATotalScore || 0) : (this.scorecardData.playerBTotalScore || 0),
      matchPoints: matchPoints,
      holePoints: isPlayerA ? (this.scorecardData.playerAHolePoints || 0) : (this.scorecardData.playerBHolePoints || 0),
      matchWin: isPlayerA ? (this.scorecardData.playerAMatchWin || false) : (this.scorecardData.playerBMatchWin || false),
      absent: isPlayerA ? (this.scorecardData.playerAAbsent || false) : (this.scorecardData.playerBAbsent || false),
      absentWithNotice: isPlayerA ? (this.scorecardData.playerAAbsentWithNotice || false) : (this.scorecardData.playerBAbsentWithNotice || false),
      strokeCount: strokeRecipient === player ? handicapDiff : 0,
      isStrokeRecipient: strokeRecipient === player
    };
  }

  private buildHoleViewModel(courseHole: any, index: number): ScorecardHoleView {
    const holeData = this.getHoleData(index);
    const strokeHoles = this.getStrokeHoles();
    
    return {
      holeNumber: courseHole.number,
      par: courseHole.par,
      handicap: courseHole.handicap,
      
      // Player A data
      playerAScore: holeData.playerAScore,
      playerAScoreClass: this.getScoreClass(holeData.playerAScore, courseHole.par),
      playerANetScore: this.getNetScore(index, 'A'),
      playerAMatchPoints: holeData.playerAMatchPoints || 0,
      playerAIsStrokeHole: this.isStrokeHole(courseHole.number, 'A'),
      playerAStrokeTooltip: this.getStrokeTooltip(courseHole.number, 'A'),
      
      // Player B data
      playerBScore: holeData.playerBScore,
      playerBScoreClass: this.getScoreClass(holeData.playerBScore, courseHole.par),
      playerBNetScore: this.getNetScore(index, 'B'),
      playerBMatchPoints: holeData.playerBMatchPoints || 0,
      playerBIsStrokeHole: this.isStrokeHole(courseHole.number, 'B'),
      playerBStrokeTooltip: this.getStrokeTooltip(courseHole.number, 'B'),
      
      // Hole result
      holeWinner: this.getHoleWinner(index)
    };
  }

  private buildSummaryViewModel(): ScorecardSummaryView {
    return {
      totalPar: this.getTotalPar(),
      matchPlayResult: this.getMatchPlayResult(),
      matchPlayScore: this.getMatchPlayScore(),
      strokePlayResult: this.getMatchResult(),
      scoreDifference: this.getScoreDifference(),
      flightName: this.getFlightName(),
      courseName: this.course.name
    };
  }

  // Combined method to update view model when scores change AND handle auto-advance
  onScoreChangeWithAutoAdvance(holeIndex: number, player: 'A' | 'B', score: number | undefined): void {
    // First, do the normal score change logic
    this.onScoreChange(holeIndex, player, score);
    
    // Then handle auto-advance logic
    this.handleAutoAdvance(holeIndex, player, score);
  }

  private handleAutoAdvance(holeIndex: number, player: 'A' | 'B', score: number | undefined): void {
    const inputId = `${player}-${holeIndex}`;
    
    // Clear any existing timeout for this input
    if (this.autoAdvanceTimeouts.has(inputId)) {
      clearTimeout(this.autoAdvanceTimeouts.get(inputId));
      this.autoAdvanceTimeouts.delete(inputId);
    }
    
    // Don't auto-advance if the score is empty or undefined
    if (!score || score === 0) {
      return;
    }
    
    // If score is 2-9, advance immediately
    if (score >= 2 && score <= 9) {
      setTimeout(() => this.advanceToNextInput(holeIndex, player), 10);
    }
    // If score is 10-19, advance immediately
    else if (score >= 10 && score <= 19) {
      setTimeout(() => this.advanceToNextInput(holeIndex, player), 10);
    }
    // If score is 1, wait to see if user might want to enter 10+
    else if (score === 1) {
      // Set a timeout to advance after 800ms if no more input
      const timeoutId = setTimeout(() => {
        this.advanceToNextInput(holeIndex, player);
        this.autoAdvanceTimeouts.delete(inputId);
      }, 800);
      this.autoAdvanceTimeouts.set(inputId, timeoutId);
    }
  }

  // Method to update view model when scores change
  onScoreChange(holeIndex: number, player: 'A' | 'B', score: number | undefined): void {
    // Update the underlying scorecard data
    this.setPlayerScore(holeIndex, player, score);
    
    // Rebuild the affected hole view model
    const courseHole = this.course.holes[holeIndex];
    this.viewModel.holes[holeIndex] = this.buildHoleViewModel(courseHole, holeIndex);
    
    // Update player totals
    this.viewModel.playerA = this.buildPlayerViewModel('A');
    this.viewModel.playerB = this.buildPlayerViewModel('B');
    
    // Update summary
    this.viewModel.summary = this.buildSummaryViewModel();
  }

  // Method to update view model when absence status changes
  onAbsenceChange(player: 'A' | 'B'): void {
    // Update the underlying scorecard data
    if (player === 'A') {
      this.scorecardData.playerAAbsent = this.viewModel.playerA.absent;
      this.scorecardData.playerAAbsentWithNotice = this.viewModel.playerA.absentWithNotice;
    } else {
      this.scorecardData.playerBAbsent = this.viewModel.playerB.absent;
      this.scorecardData.playerBAbsentWithNotice = this.viewModel.playerB.absentWithNotice;
    }
    
    this.onPlayerAbsenceChange(player);
    
    // Rebuild the entire view model since absence affects multiple aspects
    this.buildViewModel();
  }

  getTieBreaker(): string {
    // Check if there was a tie in net scores
    const wasNetScoreTie = !this.scorecardData.playerAMatchWin && !this.scorecardData.playerBMatchWin;
    const playerAMatchPoints = this.scorecardData.playerAMatchPoints || 0;
    const playerBMatchPoints = this.scorecardData.playerBMatchPoints || 0;
    
    if (wasNetScoreTie && (playerAMatchPoints > 0 || playerBMatchPoints > 0)) {
      const playerANetTotal = this.calculateNetTotal('A');
      const playerBNetTotal = this.calculateNetTotal('B');
      
      if (playerANetTotal === playerBNetTotal) {
        return `Tied Net Scores (${playerANetTotal})`;
      }
    }
    
    return '';
  }

  calculateNetTotal(player: 'A' | 'B'): number {
    if (!this.scorecardData?.holes) return 0;
    
    let netTotal = 0;
    for (let i = 0; i < this.scorecardData.holes.length; i++) {
      const hole = this.scorecardData.holes[i];
      if (hole && (player === 'A' ? hole.playerAScore : hole.playerBScore)) {
        netTotal += this.getNetScoreValue(i, player);
      }
    }
    return netTotal;
  }

  // Auto-advance functionality for score inputs
  private autoAdvanceTimeouts = new Map<string, any>();

  onScoreInput(event: Event, holeIndex: number, player: 'A' | 'B'): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
    const inputId = `${player}-${holeIndex}`;
    
    console.log('Score input:', value, 'for player:', player, 'hole:', holeIndex + 1);
    
    // Clear any existing timeout for this input
    if (this.autoAdvanceTimeouts.has(inputId)) {
      clearTimeout(this.autoAdvanceTimeouts.get(inputId));
      this.autoAdvanceTimeouts.delete(inputId);
    }
    
    // Don't auto-advance if the input is empty
    if (!value) {
      return;
    }
    
    const numValue = parseInt(value, 10);
    
    // If value is 2-9, advance immediately
    if (numValue >= 2 && numValue <= 9) {
      setTimeout(() => this.advanceToNextInput(holeIndex, player), 50);
    }
    // If value is 10-19, advance immediately
    else if (numValue >= 10 && numValue <= 19) {
      setTimeout(() => this.advanceToNextInput(holeIndex, player), 50);
    }
    // If value starts with 1, wait to see if another digit follows
    else if (value === '1') {
      // Set a timeout to advance after 800ms if no more input
      const timeoutId = setTimeout(() => {
        this.advanceToNextInput(holeIndex, player);
        this.autoAdvanceTimeouts.delete(inputId);
      }, 800);
      this.autoAdvanceTimeouts.set(inputId, timeoutId);
    }
    // If value is two digits starting with 1 (10-19)
    else if (value.length === 2 && value.startsWith('1')) {
      setTimeout(() => this.advanceToNextInput(holeIndex, player), 50);
    }
  }

  onKeyDown(event: KeyboardEvent, holeIndex: number, player: 'A' | 'B'): void {
    const inputId = `${player}-${holeIndex}`;
    
    // Handle arrow keys for manual navigation
    if (event.key === 'ArrowRight' || event.key === 'Tab' && !event.shiftKey) {
      this.clearAutoAdvanceTimeout(inputId);
      // Let default behavior happen for Tab and ArrowRight
      if (event.key === 'ArrowRight') {
        event.preventDefault();
        this.advanceToNextInput(holeIndex, player);
      }
    } else if (event.key === 'ArrowLeft' || event.key === 'Tab' && event.shiftKey) {
      this.clearAutoAdvanceTimeout(inputId);
      // Let default behavior happen for Shift+Tab and ArrowLeft
      if (event.key === 'ArrowLeft') {
        event.preventDefault();
        this.advanceToPreviousInput(holeIndex, player);
      }
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.clearAutoAdvanceTimeout(inputId);
      // Move to the other player's input for the same hole
      const otherPlayer = player === 'A' ? 'B' : 'A';
      this.focusInput(holeIndex, otherPlayer);
    } else if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.clearAutoAdvanceTimeout(inputId);
      // Move to the other player's input for the same hole
      const otherPlayer = player === 'A' ? 'B' : 'A';
      this.focusInput(holeIndex, otherPlayer);
    } else if (event.key === 'Backspace' || event.key === 'Delete') {
      // Clear timeout when user is editing
      this.clearAutoAdvanceTimeout(inputId);
    }
  }

  onPaste(event: ClipboardEvent, holeIndex: number, player: 'A' | 'B'): void {
    // After paste, advance to next input
    setTimeout(() => {
      this.advanceToNextInput(holeIndex, player);
    }, 10);
  }

  private clearAutoAdvanceTimeout(inputId: string): void {
    if (this.autoAdvanceTimeouts.has(inputId)) {
      clearTimeout(this.autoAdvanceTimeouts.get(inputId));
      this.autoAdvanceTimeouts.delete(inputId);
    }
  }

  private advanceToNextInput(holeIndex: number, player: 'A' | 'B'): void {
    // For a 9-hole league
    if (holeIndex < 8) {
      // Move to next hole for same player
      this.focusInput(holeIndex + 1, player);
    } else {
      // Last hole for current player, move to other player's first hole
      const otherPlayer = player === 'A' ? 'B' : 'A';
      if (player === 'A') {
        // Move from Player A hole 9 to Player B hole 1
        this.focusInput(0, otherPlayer);
      }
      // If we're on Player B hole 9, we're done - don't advance
    }
  }

  private advanceToPreviousInput(holeIndex: number, player: 'A' | 'B'): void {
    if (holeIndex > 0) {
      // Move to previous hole for same player
      this.focusInput(holeIndex - 1, player);
    } else {
      // First hole for current player, move to other player's last hole
      const otherPlayer = player === 'A' ? 'B' : 'A';
      if (player === 'B') {
        // Move from Player B hole 1 to Player A hole 9
        this.focusInput(8, otherPlayer);
      }
      // If we're on Player A hole 1, we're at the beginning - don't go back
    }
  }

  private focusInput(holeIndex: number, player: 'A' | 'B'): void {
    // Use setTimeout to ensure DOM is updated
    setTimeout(() => {
      const inputId = `player${player}-hole-${holeIndex + 1}`;
      const input = document.getElementById(inputId) as HTMLInputElement;
      
      if (input && !input.readOnly && !input.disabled) {
        try {
          input.focus();
          input.select();
        } catch (error) {
          console.error('Error focusing input:', error);
        }
      } else {
        
        // Fallback: try ViewChildren approach
        if (this.scoreInputs) {
          const inputs = this.scoreInputs.toArray();
          const targetInput = inputs.find(inputRef => {
            const element = inputRef.nativeElement;
            const holeAttr = element.getAttribute('data-hole');
            const playerAttr = element.getAttribute('data-player');
            return holeAttr === holeIndex.toString() && playerAttr === player;
          });

          if (targetInput && !targetInput.nativeElement.readOnly) {
            targetInput.nativeElement.focus();
            targetInput.nativeElement.select();
            console.log('Focused using ViewChildren fallback');
          }
        }
      }
    }, 10);
  }

  ngOnDestroy(): void {
    // Clean up all auto-advance timeouts
    this.autoAdvanceTimeouts.forEach(timeout => clearTimeout(timeout));
    this.autoAdvanceTimeouts.clear();
  }

  isMatchupComplete(): boolean {
    // Consider matchup complete if any player is marked absent
    if (this.scorecardData.playerAAbsent || this.scorecardData.playerBAbsent) {
      return true;
    }
    
    // Traditional completion check - at least some scores are entered
    return this.scorecardData.holes.some(hole => hole.playerAScore || hole.playerBScore);
  }
}
