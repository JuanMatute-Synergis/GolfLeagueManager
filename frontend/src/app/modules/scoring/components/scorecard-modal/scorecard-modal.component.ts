import { Component, Input, Output, EventEmitter, OnInit, OnChanges, OnDestroy, SimpleChanges, ViewChildren, QueryList, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScorecardData, HoleScore, Course } from '../../models/scorecard.model';
import { ScorecardViewModel, ScorecardHoleView, ScorecardPlayerView, ScorecardSummaryView } from '../../models/scorecard-view.model';
import { ScorecardService } from '../../services/scorecard.service';
import { CourseService } from '../../../../core/services/course.service';
import { ScoreCalculationService } from '../../services/score-calculation.service';
import { Course as CourseModel } from '../../../../core/models/course.model';
import { Week, NineHoles } from '../../models/week.model';
import { LeagueSettingsService, LeagueSettings } from '../../../settings/services/league-settings.service';

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
  @Input() week?: Week; // Week data to determine which holes to show

  // For view mode, we can accept individual matchup properties as alternative inputs
  @Input() matchupId?: string;
  @Input() playerAName?: string;
  @Input() playerBName?: string;
  @Input() flightName?: string;

  @Output() save = new EventEmitter<ScorecardData>();
  @Output() close = new EventEmitter<void>();

  // View model for template binding
  viewModel: ScorecardViewModel = this.createEmptyViewModel();

  // Loading guard to prevent multiple simultaneous loads
  private isLoadingScorecard = false;

  // Cache for relevant holes to avoid redundant calculations
  private cachedRelevantHoles: any[] | null = null;
  private cachedWeekId: string | undefined = undefined;

  // Cache for stroke holes calculation
  private cachedStrokeHoles: number[] | null = null;
  private cachedHandicapKey: string | undefined = undefined;

  // League settings
  private leagueSettings: LeagueSettings | null = null;
  private isLoadingLeagueSettings = false;

  constructor(
    private scorecardService: ScorecardService,
    private courseService: CourseService,
    private scoreCalculationService: ScoreCalculationService,
    private leagueSettingsService: LeagueSettingsService
  ) { }

  // Convenience getters for backward compatibility
  get isLoading(): boolean { return this.viewModel.isLoading; }
  set isLoading(value: boolean) { this.viewModel.isLoading = value; }

  get error(): string | null { return this.viewModel.error; }
  set error(value: string | null) { this.viewModel.error = value; }

  get saveError(): string | null { return this.viewModel.saveError; }
  set saveError(value: string | null) { this.viewModel.saveError = value; }

  // Default course - will be replaced by database data when available
  course: Course = {
    name: "Loading...",
    holes: [
      { number: 1, par: 4, yardage: 380, handicap: 3 },
      { number: 2, par: 4, yardage: 354, handicap: 5 },
      { number: 3, par: 3, yardage: 104, handicap: 17 },
      { number: 4, par: 5, yardage: 452, handicap: 7 },
      { number: 5, par: 3, yardage: 154, handicap: 13 },
      { number: 6, par: 5, yardage: 469, handicap: 1 },
      { number: 7, par: 4, yardage: 320, handicap: 15 },
      { number: 8, par: 4, yardage: 352, handicap: 9 },
      { number: 9, par: 4, yardage: 364, handicap: 11 },
      { number: 10, par: 4, yardage: 373, handicap: 4 },
      { number: 11, par: 4, yardage: 274, handicap: 14 },
      { number: 12, par: 3, yardage: 135, handicap: 18 },
      { number: 13, par: 4, yardage: 264, handicap: 16 },
      { number: 14, par: 5, yardage: 478, handicap: 6 },
      { number: 15, par: 4, yardage: 413, handicap: 2 },
      { number: 16, par: 3, yardage: 173, handicap: 12 },
      { number: 17, par: 4, yardage: 335, handicap: 10 },
      { number: 18, par: 4, yardage: 351, handicap: 8 }
    ]
  };

  // ViewChildren to access all score input elements - simplified approach
  @ViewChildren('scoreInput') scoreInputs!: QueryList<ElementRef<HTMLInputElement>>; ngOnInit() {
    console.log('ngOnInit called, initializing scorecard modal, mode:', this.mode);

    // Load course data first - this is critical for proper par/handicap display
    this.loadCourseData();

    // Load league settings if week is available
    if (this.week?.seasonId) {
      this.loadLeagueSettings();
    }

    // For view mode, create scorecardData from individual inputs if not provided
    if (this.mode === 'view' && !this.scorecardData && this.matchupId) {
      this.initializeViewModeData();
    }

    // Always ensure holes array is initialized to prevent template errors
    // But only if no holes exist or they are empty
    if (!this.scorecardData?.holes || this.scorecardData.holes.length === 0) {
      this.initializeHoles();
    }

    // Only build initial view model if modal is not open
    // If modal is open, ngOnChanges will handle the scorecard loading
    if (!this.isOpen) {
      this.buildViewModel();
    }

    // Note: We don't call loadScorecardData here anymore to avoid duplication
    // ngOnChanges will handle loading when the modal opens
  }

  private loadCourseData(): void {
    this.courseService.getAllCourses().subscribe({
      next: (courses: CourseModel[]) => {
        console.log('Raw course data from API:', courses);
        if (courses.length > 0) {
          // Use the first course from the database
          const dbCourse = courses[0];
          console.log('Selected course from database:', dbCourse);

          // Check if we have existing scorecard data with scores before updating
          const hasExistingScores = this.scorecardData?.holes?.some(hole =>
            hole.playerAScore !== undefined || hole.playerBScore !== undefined
          );
          console.log('Has existing scores before course update:', hasExistingScores);

          this.course = {
            name: dbCourse.name,
            holes: dbCourse.courseHoles.map(hole => ({
              number: hole.holeNumber,
              par: hole.par,
              yardage: hole.yardage,
              handicap: hole.handicapIndex
            }))
          };

          console.log('Course data loaded from database:', this.course);
          console.log('Course holes mapped:', this.course.holes);

          // Clear the relevant holes cache since course data changed
          this.clearRelevantHolesCache();

          // If scorecard data exists, update hole pars/handicaps without overwriting scores
          if (this.scorecardData && this.scorecardData.holes) {
            console.log('Updating hole data with new course data without overwriting scores');
            this.updateHolesWithCourseData();
            this.buildViewModel();
          }
        }
      },
      error: (error) => {
        console.error('Error loading course data:', error);
        // Keep default course configuration as fallback
        console.warn('Using fallback course data due to API error');
      }
    });
  }

  private loadLeagueSettings(): void {
    if (!this.week?.seasonId || this.isLoadingLeagueSettings) {
      return;
    }

    this.isLoadingLeagueSettings = true;

    this.leagueSettingsService.getLeagueSettings(this.week.seasonId).subscribe({
      next: (settings) => {
        console.log('League settings loaded:', settings);
        this.leagueSettings = settings;
        this.isLoadingLeagueSettings = false;

        // Recalculate match play points if scorecard data is already loaded
        if (this.scorecardData?.holes) {
          this.calculateMatchPlayPoints();
          this.buildViewModel();
        }
      },
      error: (error) => {
        console.error('Error loading league settings:', error);
        this.isLoadingLeagueSettings = false;
        // Fall back to default values if league settings can't be loaded
        this.leagueSettings = null;
      }
    });
  }

  ngAfterViewInit(): void {
    // ViewChildren are now available
    console.log('ngAfterViewInit called - score inputs available:', this.scoreInputs?.length);
  }

  ngOnChanges(changes: SimpleChanges) {
    console.log('ngOnChanges called:', changes);

    // Debug week changes
    if (changes['week']) {
      console.log('Week changed:', changes['week'].currentValue);
      // Clear all caches when week changes
      this.clearAllCaches();
      // Load league settings for the new week's season
      this.loadLeagueSettings();
      // Rebuild view model when week changes
      if (this.scorecardData) {
        this.buildViewModel();
      }
    }

    // For view mode, initialize data from individual inputs if needed
    if (this.mode === 'view' && (changes['matchupId'] || changes['isOpen']) && this.isOpen) {
      if (!this.scorecardData || (this.scorecardData.matchupId !== this.matchupId && this.matchupId)) {
        // Only initialize view mode data if we don't have scorecard data yet
        // Don't modify existing scorecardData to avoid change detection errors
        if (!this.scorecardData) {
          this.initializeViewModeData();
        }
        // For view mode, we'll use the input matchupId directly in loadScorecardData()
        // instead of modifying the scorecardData object
      }
    }

    // Load scorecard data when modal opens (but only once)
    // For view mode, ensure we have the matchupId before loading
    if (changes['isOpen'] && changes['isOpen'].currentValue === true && !this.isLoadingScorecard) {
      console.log('Modal opened, loading scorecard data');

      // For view mode, ensure we have the matchupId available but don't modify scorecardData
      if (this.mode === 'view' && this.matchupId) {
        // Ensure scorecardData exists without modifying the original
        if (!this.scorecardData) {
          this.initializeViewModeData();
        }
        // Don't modify the existing scorecardData.matchupId to avoid change detection errors
      }

      this.loadScorecardData();

      // Auto-focus first hole for Player A after modal opens and data loads
      if (this.mode === 'edit') {
        setTimeout(() => {
          this.autoFocusFirstEmptyHole();
        }, 100);
      }
    }

    // Only recalculate when scorecard data changes if modal is not currently loading
    if (changes['scorecardData'] && changes['scorecardData'].currentValue && !this.isLoading && !this.isLoadingScorecard) {
      console.log('Scorecard data changed externally, recalculating totals');
      // Small delay to ensure all data is loaded
      setTimeout(() => {
        this.calculateTotals();
      }, 0);
    }

    // Only recalculate when player/matchup data changes if not currently loading
    if ((changes['playerAName'] || changes['playerBName'] || changes['matchupId']) && this.isOpen && !this.isLoading && !this.isLoadingScorecard) {
      console.log('Player or matchup data changed, may need to reload scorecard');
      // Only reload if the matchupId actually changed
      if (changes['matchupId'] && changes['matchupId'].currentValue !== changes['matchupId'].previousValue) {
        this.loadScorecardData();
      }
    }
  }

  loadScorecardData() {
    // For view mode, prioritize the input matchupId over scorecardData.matchupId
    let matchupId: string;
    if (this.mode === 'view' && this.matchupId) {
      matchupId = this.matchupId;
    } else {
      matchupId = this.scorecardData?.matchupId || this.matchupId || '';
    }

    console.log('loadScorecardData called with matchupId:', matchupId, 'isLoadingScorecard:', this.isLoadingScorecard);
    console.log('Mode:', this.mode);
    console.log('Scorecard data at load:', this.scorecardData);
    console.log('Input matchupId:', this.matchupId);

    // Prevent multiple simultaneous loads
    if (this.isLoadingScorecard) {
      console.log('Already loading scorecard data, skipping duplicate call');
      return;
    }

    if (!matchupId || matchupId === '') {
      console.log('No matchupId provided, cannot load scorecard data');
      if (this.mode === 'view') {
        this.error = 'No matchup ID available to load scorecard data.';
        this.buildViewModel();
      } else {
        this.initializeHoles();
      }
      return;
    }

    this.isLoadingScorecard = true;
    this.isLoading = true;
    this.error = null; // Clear any previous errors

    this.scorecardService.getCompleteScorecard(matchupId).subscribe({
      next: (response) => {
        console.log('Scorecard loaded from backend:', response);
        this.isLoading = false;
        this.isLoadingScorecard = false;

        if (response.success && response.holeScores && response.holeScores.length > 0) {
          // Ensure scorecardData exists for view mode, but don't overwrite if it already exists
          if (this.mode === 'view' && !this.scorecardData) {
            this.initializeViewModeData();
          }
          // Always ensure scorecardData exists (edit mode or view mode without existing data)
          if (!this.scorecardData) {
            console.error('scorecardData is null when trying to load backend data');
            return;
          }
          // Use relevant holes based on week setting, map scores by actual hole number
          const relevantHoles = this.getRelevantHoles();
          console.log('[DEBUG] Backend holeScores:', response.holeScores);
          console.log('[DEBUG] Relevant holes for mapping:', relevantHoles.map(h => h.number));

          const holesArray = relevantHoles.map((hole) => {
            // Map scores by actual hole number (1-9, 10-18)
            const hs = (response.holeScores ?? []).find(h => h.holeNumber === hole.number);
            console.log(`[DEBUG] Mapping hole ${hole.number}:`, {
              foundHoleScore: hs,
              playerAScore: hs?.playerAScore,
              playerBScore: hs?.playerBScore
            });
            return {
              hole: hole.number // Display the actual hole number (1-9 for front, 10-18 for back)
              , par: hole.par
              , playerAScore: hs?.playerAScore ?? undefined
              , playerBScore: hs?.playerBScore ?? undefined
              , holeHandicap: hs?.holeHandicap ?? hole.handicap
              , playerAMatchPoints: hs?.playerAMatchPoints ?? 0
              , playerBMatchPoints: hs?.playerBMatchPoints ?? 0
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

          console.log('[DEBUG] Scorecard data after loading from backend:', {
            mode: this.mode,
            holesCount: this.scorecardData.holes.length,
            firstHoleAScore: this.scorecardData.holes[0]?.playerAScore,
            firstHoleBScore: this.scorecardData.holes[0]?.playerBScore,
            playerAMatchPoints: response.playerAMatchPoints,
            playerBMatchPoints: response.playerBMatchPoints,
            playerAAbsent: response.playerAAbsent,
            playerBAbsent: response.playerBAbsent
          });

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

          // Clear stroke holes cache when handicaps change
          this.clearStrokeHolesCache();

          // Build view model with updated backend data (this already includes all calculations)
          this.buildViewModel();
        } else {
          // No existing scorecard, initialize with empty holes
          if (this.mode === 'view') {
            this.error = 'No scorecard data available for this matchup.';
            // Still build view model for consistent state
            this.buildViewModel();
          } else {
            this.initializeHoles();
            // Auto-focus first hole after initializing empty scorecard
            setTimeout(() => {
              this.autoFocusFirstEmptyHole();
            }, 150);
          }
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.isLoadingScorecard = false;
        console.log('Error loading scorecard:', error);

        if (this.mode === 'view') {
          this.error = 'Failed to load scorecard data.';
          // Build view model even for error case to maintain consistent state
          this.buildViewModel();
        } else {
          console.log('No existing scorecard found, initializing empty scorecard');
          // If no scorecard exists, initialize with empty holes for edit mode
          this.initializeHoles();
          // Auto-focus first hole after initializing empty scorecard
          setTimeout(() => {
            this.autoFocusFirstEmptyHole();
          }, 150);
        }
      }
    });
  }

  initializeHoles() {
    if (!this.scorecardData) {
      console.error('Cannot initialize holes: scorecardData is null');
      return;
    }

    console.log('initializeHoles() called - this will create empty holes');

    // Filter holes based on week setting
    const relevantHoles = this.getRelevantHoles();

    this.scorecardData.holes = relevantHoles.map(hole => ({
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

    console.log('Initialized holes with empty scores:', this.scorecardData.holes.length, 'holes');

    // Initialize match play data
    this.scorecardData.playerAMatchPoints = 0;
    this.scorecardData.playerBMatchPoints = 0;
    this.scorecardData.playerAHolePoints = 0;
    this.scorecardData.playerBHolePoints = 0;
    this.scorecardData.playerAMatchWin = false;
    this.scorecardData.playerBMatchWin = false;

    // Only calculate totals if we're in edit mode and creating a new scorecard
    // Don't trigger calculations during component initialization
    if (this.mode === 'edit' && this.isOpen) {
      this.calculateTotals();
    }
  }

  /**
   * Clear the relevant holes cache when week data changes
   */
  private clearRelevantHolesCache(): void {
    this.cachedRelevantHoles = null;
    this.cachedWeekId = undefined;
  }

  /**
   * Clear stroke holes cache when handicaps or week change
   */
  private clearStrokeHolesCache(): void {
    this.cachedStrokeHoles = null;
    this.cachedHandicapKey = undefined;
  }

  /**
   * Clear all caches
   */
  private clearAllCaches(): void {
    this.clearRelevantHolesCache();
    this.clearStrokeHolesCache();
  }
  private getRelevantHoles() {
    const currentWeekId = this.week?.id || 'no-week';

    // Return cached result if week hasn't changed
    if (this.cachedRelevantHoles && this.cachedWeekId === currentWeekId) {
      return this.cachedRelevantHoles;
    }

    console.log('getRelevantHoles called - calculating for week:', this.week);

    let relevantHoles: any[];

    if (!this.week) {
      console.log('No week data, returning all holes');
      // If no week data, default to all holes for backward compatibility
      relevantHoles = this.course.holes;
    } else {
      console.log('Week nineHoles value:', this.week.nineHoles, 'NineHoles.Front:', NineHoles.Front, 'NineHoles.Back:', NineHoles.Back);

      if (this.week.nineHoles === NineHoles.Front) {
        // Front 9: holes 1-9
        console.log('Returning front 9 holes (1-9)');
        relevantHoles = this.course.holes.filter(hole => hole.number >= 1 && hole.number <= 9);
      } else {
        // Back 9: holes 10-18
        console.log('Returning back 9 holes (10-18)');
        relevantHoles = this.course.holes.filter(hole => hole.number >= 10 && hole.number <= 18);
      }
    }

    // Cache the result
    this.cachedRelevantHoles = relevantHoles;
    this.cachedWeekId = currentWeekId;

    return relevantHoles;
  }

  // Initialize scorecard data for view mode from individual inputs
  private initializeViewModeData() {
    console.log('Initializing view mode data with matchupId:', this.matchupId);
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
    const relevantHoles = this.getRelevantHoles();
    return relevantHoles.reduce((sum, hole) => sum + hole.par, 0);
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
      isAbsenceScenario: this.scorecardData.playerAAbsent || this.scorecardData.playerBAbsent,
      hasBackendMatchPoints: this.scorecardData.playerAMatchPoints !== undefined && this.scorecardData.playerBMatchPoints !== undefined
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
      // Only calculate match play points if we don't already have backend-calculated values
      // This prevents redundant calculations when loading from backend
      const hasBackendMatchPoints = this.scorecardData.playerAMatchPoints !== undefined &&
        this.scorecardData.playerBMatchPoints !== undefined &&
        (this.scorecardData.playerAMatchPoints > 0 || this.scorecardData.playerBMatchPoints > 0);

      if (!hasBackendMatchPoints) {
        console.log('No backend match points found, calculating locally');
        this.calculateMatchPlayPoints();
      } else {
        console.log('Using backend-calculated match points, skipping local calculation');
      }
    }

    // Build view model after calculations (but don't trigger more calculations)
    this.buildViewModel();
  }

  private calculateMatchPlayPoints() {
    console.log('calculateMatchPlayPoints called');
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
            hole.playerAMatchPoints = this.getHoleWinPoints();
            hole.playerBMatchPoints = 0;
            playerAHolePoints += this.getHoleWinPoints();
            break;
          case 'playerB':
            hole.playerAMatchPoints = 0;
            hole.playerBMatchPoints = this.getHoleWinPoints();
            playerBHolePoints += this.getHoleWinPoints();
            break;
          case 'tie':
            hole.playerAMatchPoints = this.getHoleHalvePoints();
            hole.playerBMatchPoints = this.getHoleHalvePoints();
            playerAHolePoints += this.getHoleHalvePoints();
            playerBHolePoints += this.getHoleHalvePoints();
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
    let completedHoles = 0;

    for (let i = 0; i < this.scorecardData.holes.length; i++) {
      const hole = this.scorecardData.holes[i];
      if (hole?.playerAScore && hole?.playerBScore) {
        playerANetTotal += this.getNetScoreValue(i, 'A');
        playerBNetTotal += this.getNetScoreValue(i, 'B');
        completedHoles++;
      }
    }

    // Determine match play winner based on lowest total net score and award 2-point bonus
    let playerAMatchPoints = playerAHolePoints;
    let playerBMatchPoints = playerBHolePoints;

    // Only award match points if there are actually completed holes
    if (completedHoles > 0) {
      if (playerANetTotal < playerBNetTotal) {
        // Player A has lower net total - wins match
        playerAMatchPoints += this.getMatchWinBonus();
        this.scorecardData.playerAMatchWin = true;
        this.scorecardData.playerBMatchWin = false;
      } else if (playerBNetTotal < playerANetTotal) {
        // Player B has lower net total - wins match
        playerBMatchPoints += this.getMatchWinBonus();
        this.scorecardData.playerAMatchWin = false;
        this.scorecardData.playerBMatchWin = true;
      } else {
        // Tie in net total scores - each player gets tie points instead of win bonus
        playerAMatchPoints += this.getMatchTiePoints();
        playerBMatchPoints += this.getMatchTiePoints();
        this.scorecardData.playerAMatchWin = false;
        this.scorecardData.playerBMatchWin = false;
      }
    } else {
      // No completed holes - no match points awarded
      this.scorecardData.playerAMatchWin = false;
      this.scorecardData.playerBMatchWin = false;
    }

    this.scorecardData.playerAMatchPoints = playerAMatchPoints;
    this.scorecardData.playerBMatchPoints = playerBMatchPoints;
  }

  // Helper methods for league settings with fallback to default values
  private getHoleWinPoints(): number {
    return this.leagueSettings?.holeWinPoints ?? 2;
  }

  private getHoleHalvePoints(): number {
    return this.leagueSettings?.holeHalvePoints ?? 1;
  }

  private getMatchWinBonus(): number {
    return this.leagueSettings?.matchWinBonus ?? 2;
  }

  private getMatchTiePoints(): number {
    return this.leagueSettings?.matchTiePoints ?? 1;
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
    const relevantHoles = this.getRelevantHoles();
    const courseHole = relevantHoles[holeIndex];

    if (!hole || !courseHole) return '-';

    const grossScore = player === 'A' ? hole.playerAScore : hole.playerBScore;
    if (!grossScore) return '-';

    const playerHandicap = player === 'A' ?
      (this.scorecardData?.playerAHandicap || 0) :
      (this.scorecardData?.playerBHandicap || 0);

    // Use the same logic as getNetScoreValue for consistency
    // If handicap is 0, return gross score
    if (playerHandicap === 0) {
      return grossScore.toString();
    }

    const strokesGiven = this.getStrokesGiven(holeIndex, playerHandicap);
    const netScore = grossScore - strokesGiven;

    return netScore.toString();
  }

  getHoleWinner(holeIndex: number): 'playerA' | 'playerB' | 'tie' | null {
    // Use safe hole data access
    const hole = this.getHoleData(holeIndex);

    if (!hole || !hole.playerAScore || !hole.playerBScore) {
      console.log(`[HOLE WINNER] Hole ${holeIndex + 1}: No winner - playerAScore: ${hole?.playerAScore}, playerBScore: ${hole?.playerBScore}`);
      return null;
    }

    // Get handicaps
    const playerAHandicap = this.scorecardData?.playerAHandicap || 0;
    const playerBHandicap = this.scorecardData?.playerBHandicap || 0;

    // If both players have 0 handicap, use gross scores
    if (playerAHandicap === 0 && playerBHandicap === 0) {
      const winner = hole.playerAScore < hole.playerBScore ? 'playerA' :
        hole.playerBScore < hole.playerAScore ? 'playerB' : 'tie';
      console.log(`[HOLE WINNER] Hole ${holeIndex + 1}: Gross scores - A: ${hole.playerAScore}, B: ${hole.playerBScore}, Winner: ${winner}`);
      return winner;
    }

    // Otherwise use net scores with handicap adjustments
    const playerANet = this.getNetScoreValue(holeIndex, 'A');
    const playerBNet = this.getNetScoreValue(holeIndex, 'B');

    const winner = playerANet < playerBNet ? 'playerA' :
      playerBNet < playerANet ? 'playerB' : 'tie';
    console.log(`[HOLE WINNER] Hole ${holeIndex + 1}: Net scores - A: ${playerANet}, B: ${playerBNet}, Winner: ${winner}`);
    return winner;
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
    const relevantHoles = this.getRelevantHoles();
    const courseHole = relevantHoles[holeIndex];
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
    const holesInPlay = this.getRelevantHoles(); // Use filtered holes based on week setting
    const holesWithHandicap = holesInPlay.filter(h => typeof h.handicap === 'number');
    const hardestHoles = [...holesWithHandicap]
      .sort((a, b) => (a.handicap ?? 99) - (b.handicap ?? 99))
      .slice(0, Math.round(handicapDifference))
      .map(h => h.number);

    // Debug logging for hole 10 (number 10)
    if (courseHole.number === 10) {
      console.log(`Hole 10 Stroke Allocation:`, {
        playerHandicap,
        playerAHandicap,
        playerBHandicap,
        handicapDifference,
        playerReceivesStrokes,
        hardestHoles,
        courseHoleNumber: courseHole.number,
        courseHoleHandicap: courseHole.handicap,
        strokesGiven: hardestHoles.includes(courseHole.number) ? 1 : 0
      });
    }

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
    console.log('initializeScorecard called manually, isLoadingScorecard:', this.isLoadingScorecard);

    // Prevent duplicate loads
    if (this.isLoadingScorecard) {
      console.log('Already loading scorecard, skipping manual initialization');
      return;
    }

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
      const relevantHoles = this.getRelevantHoles();
      const courseHole = relevantHoles[index];
      return {
        hole: courseHole?.number || (index + 1),
        par: courseHole?.par || 4,
        playerAScore: undefined,
        playerBScore: undefined,
        holeHandicap: courseHole?.handicap || 1,
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
    const score = player === 'A' ? hole.playerAScore : hole.playerBScore;
    return score === null ? undefined : score;
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

    // Don't call calculateTotals here - let the calling method handle updates more efficiently
  }

  // Helper method to safely get match points
  getMatchPoints(holeIndex: number, player: 'A' | 'B'): number {
    const hole = this.getHoleData(holeIndex);
    return player === 'A' ? (hole.playerAMatchPoints || 0) : (hole.playerBMatchPoints || 0);
  }

  /**
   * Update existing holes with new course data (par/handicap) without overwriting scores
   */
  private updateHolesWithCourseData(): void {
    if (!this.scorecardData?.holes) {
      return;
    }

    const relevantHoles = this.getRelevantHoles();

    // Update existing holes with new course data while preserving scores
    for (let i = 0; i < this.scorecardData.holes.length; i++) {
      const hole = this.scorecardData.holes[i];
      const courseHole = relevantHoles[i];

      if (hole && courseHole) {
        // Update par and handicap from course data
        hole.par = courseHole.par;
        hole.holeHandicap = courseHole.handicap;
        // Keep existing scores and match points intact
        console.log(`Updated hole ${hole.hole}: par=${hole.par}, handicap=${hole.holeHandicap}, preserving scores A=${hole.playerAScore}, B=${hole.playerBScore}`);
      }
    }

    console.log('Finished updating holes with course data while preserving scores');
  }

  // Stroke allocation methods
  getStrokeHoles(): number[] {
    const handicapDiff = Math.abs((this.scorecardData.playerAHandicap || 0) - (this.scorecardData.playerBHandicap || 0));
    if (handicapDiff === 0) return [];

    // Create cache key from handicaps and week
    const handicapKey = `${this.scorecardData.playerAHandicap || 0}-${this.scorecardData.playerBHandicap || 0}-${this.week?.id || 'no-week'}`;

    // Return cached result if handicaps and week haven't changed
    if (this.cachedStrokeHoles && this.cachedHandicapKey === handicapKey) {
      return this.cachedStrokeHoles;
    }

    // Debug logging
    console.log('Calculating stroke holes:', {
      playerAHandicap: this.scorecardData.playerAHandicap,
      playerBHandicap: this.scorecardData.playerBHandicap,
      handicapDiff: handicapDiff
    });

    // Get the relevant holes based on week's NineHoles setting
    const holes = this.getRelevantHoles();

    console.log('Relevant holes for strokes:', holes.map(h => h.number));

    // Sort holes by handicap index (1 = hardest)
    const sortedHoles = holes
      .map((hole, index) => ({
        holeNumber: hole.number,
        handicapIndex: hole.handicap || 18,
        arrayIndex: index
      }))
      .sort((a, b) => a.handicapIndex - b.handicapIndex);

    // Return the hole numbers that get strokes (based on handicap difference)
    const strokeHoles = sortedHoles.slice(0, handicapDiff).map(h => h.holeNumber);
    console.log('Stroke holes calculated:', strokeHoles);

    // Cache the result
    this.cachedStrokeHoles = strokeHoles;
    this.cachedHandicapKey = handicapKey;

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
    const relevantHoles = this.getRelevantHoles();
    this.viewModel.holes = relevantHoles.map((courseHole, index) =>
      this.buildHoleViewModel(courseHole, index)
    );

    console.log('[DEBUG] Built hole view models:', {
      relevantHolesCount: relevantHoles.length,
      viewModelHolesCount: this.viewModel.holes.length,
      firstHoleViewModel: this.viewModel.holes[0],
      scorecardDataHoles: this.scorecardData.holes?.length || 0,
      firstScorecardHole: this.scorecardData.holes?.[0]
    });

    // Build summary view model
    this.viewModel.summary = this.buildSummaryViewModel();

    // Set metadata
    this.viewModel.matchupId = this.scorecardData.matchupId || '';

    // Clear loading state if it was set
    this.viewModel.isLoading = false;
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

    // Determine if this is a stroke hole for each player
    const playerAIsStrokeHole = this.isStrokeHole(courseHole.number, 'A');
    const playerBIsStrokeHole = this.isStrokeHole(courseHole.number, 'B');

    return {
      holeNumber: courseHole.number,
      par: courseHole.par,
      handicap: courseHole.handicap,

      // Player A data
      playerAScore: holeData.playerAScore,
      playerAScoreClass: this.getScoreClass(holeData.playerAScore === null ? undefined : holeData.playerAScore, courseHole.par),
      playerANetScore: this.getNetScore(index, 'A'),
      playerAMatchPoints: holeData.playerAMatchPoints || 0,
      playerAIsStrokeHole: playerAIsStrokeHole,
      playerAStrokeTooltip: this.getStrokeTooltip(courseHole.number, 'A'),

      // Player B data
      playerBScore: holeData.playerBScore,
      playerBScoreClass: this.getScoreClass(holeData.playerBScore === null ? undefined : holeData.playerBScore, courseHole.par),
      playerBNetScore: this.getNetScore(index, 'B'),
      playerBMatchPoints: holeData.playerBMatchPoints || 0,
      playerBIsStrokeHole: playerBIsStrokeHole,
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

    // Calculate match play points first (affects all holes)
    this.updatePlayerTotals();

    // Rebuild all hole view models to ensure match points are updated everywhere
    const relevantHoles = this.getRelevantHoles();
    this.viewModel.holes = relevantHoles.map((courseHole, index) =>
      this.buildHoleViewModel(courseHole, index)
    );

    console.log(`[DEBUG] onScoreChange - holeIndex: ${holeIndex}, courseHole.number: ${relevantHoles[holeIndex]?.number}, player: ${player}, score: ${score}`);

    // Update summary
    this.viewModel.summary = this.buildSummaryViewModel();
  }

  // Efficient method to update just the player totals
  private updatePlayerTotals(): void {
    // Calculate total scores
    this.scorecardData.playerATotalScore = this.scorecardData.holes.reduce((sum, hole) => {
      return sum + (hole?.playerAScore || 0);
    }, 0);

    this.scorecardData.playerBTotalScore = this.scorecardData.holes.reduce((sum, hole) => {
      return sum + (hole?.playerBScore || 0);
    }, 0);

    // Recalculate match play for live scoring
    this.calculateMatchPlayPoints();

    // Update only the player view models
    this.viewModel.playerA = this.buildPlayerViewModel('A');
    this.viewModel.playerB = this.buildPlayerViewModel('B');
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
    } else if (event.key === 'Backspace') {
      // Clear timeout when user is editing
      this.clearAutoAdvanceTimeout(inputId);

      // Get the current input element
      const input = event.target as HTMLInputElement;
      const currentValue = input.value;

      // If the input is already empty, move to previous hole and continue chain if it's also empty
      if (!currentValue || currentValue.trim() === '') {
        event.preventDefault();

        // Move to previous hole without clearing anything (already empty)
        this.advanceToPreviousInputWithChain(holeIndex, player);
      } else if (currentValue.length === 1) {
        // If there's only one character, let the backspace clear it, then clear scorecard data and DON'T move yet
        // The user will need to press backspace again to move
        setTimeout(() => {
          // Clear the scorecard data after the browser has cleared the input
          this.onScoreChange(holeIndex, player, undefined);
        }, 0);
      } else {
        // Multiple characters - let normal backspace behavior happen
        // Don't clear scorecard data or move focus
      }
    } else if (event.key === 'Delete') {
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
    // Use the relevant holes being displayed, not the full course
    const relevantHoles = this.getRelevantHoles();
    const totalHoles = relevantHoles.length;

    if (holeIndex < totalHoles - 1) {
      // Move to next hole for same player
      this.focusInput(holeIndex + 1, player);
    } else {
      // Last hole for current player, move to other player's first hole
      const otherPlayer = player === 'A' ? 'B' : 'A';
      this.focusInput(0, otherPlayer);
      // If on Player B last hole, do not advance further
    }
  }

  private advanceToPreviousInput(holeIndex: number, player: 'A' | 'B'): void {
    // Use the relevant holes being displayed, not the full course
    const relevantHoles = this.getRelevantHoles();
    const totalHoles = relevantHoles.length;

    if (holeIndex > 0) {
      // Move to previous hole for same player
      this.focusInput(holeIndex - 1, player);
    } else {
      // First hole for current player, move to other player's last hole
      const otherPlayer = player === 'A' ? 'B' : 'A';
      this.focusInput(totalHoles - 1, otherPlayer);
      // If on Player A first hole, do not go back further
    }
  }

  private advanceToPreviousInputWithChain(holeIndex: number, player: 'A' | 'B'): void {
    // Use the relevant holes being displayed, not the full course
    const relevantHoles = this.getRelevantHoles();
    const totalHoles = relevantHoles.length;

    if (holeIndex > 0) {
      // Move to previous hole for same player
      const previousHoleIndex = holeIndex - 1;
      this.focusInput(previousHoleIndex, player);

      // Check if the previous hole is also empty after a brief delay to allow focus
      setTimeout(() => {
        const previousInputId = `player${player}-hole-${previousHoleIndex + 1}`;
        const previousInput = document.getElementById(previousInputId) as HTMLInputElement;

        if (previousInput && (!previousInput.value || previousInput.value.trim() === '')) {
          // Previous hole is also empty, continue the chain
          this.advanceToPreviousInputWithChain(previousHoleIndex, player);
        }
      }, 10);
    } else {
      // First hole for current player, move to other player's last hole
      const otherPlayer = player === 'A' ? 'B' : 'A';
      this.focusInput(totalHoles - 1, otherPlayer);

      // Check if the other player's last hole is also empty
      setTimeout(() => {
        const otherInputId = `player${otherPlayer}-hole-${totalHoles}`;
        const otherInput = document.getElementById(otherInputId) as HTMLInputElement;

        if (otherInput && (!otherInput.value || otherInput.value.trim() === '')) {
          // Other player's last hole is also empty, continue the chain
          this.advanceToPreviousInputWithChain(totalHoles - 1, otherPlayer);
        }
      }, 10);
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

          if (targetInput) {
            targetInput.nativeElement.focus();
            targetInput.nativeElement.select();
          }
        }
      }
    }, 0);
  }

  private autoFocusFirstEmptyHole(): void {
    if (!this.scorecardData.holes) {
      return;
    }

    const relevantHoles = this.getRelevantHoles();

    // First, try to find the first empty hole for Player A
    for (let i = 0; i < relevantHoles.length; i++) {
      const hole = this.scorecardData.holes[i];

      if (!hole || hole.playerAScore === null || hole.playerAScore === undefined) {
        this.focusInput(i, 'A');
        return;
      }
    }

    // If all Player A holes have scores, check Player B
    for (let i = 0; i < relevantHoles.length; i++) {
      const hole = this.scorecardData.holes[i];

      if (!hole || hole.playerBScore === null || hole.playerBScore === undefined) {
        this.focusInput(i, 'B');
        return;
      }
    }
  }

  ngOnDestroy(): void {
    // Clear any remaining auto-advance timeouts
    this.autoAdvanceTimeouts.forEach(timeout => clearTimeout(timeout));
    this.autoAdvanceTimeouts.clear();
  }
}
