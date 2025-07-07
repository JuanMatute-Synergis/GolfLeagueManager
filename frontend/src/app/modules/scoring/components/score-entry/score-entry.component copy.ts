import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { trigger, transition, style, animate } from '@angular/animations';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom, forkJoin } from 'rxjs';
import { ScoringService } from '../../services/scoring.service';
import { ScorecardService } from '../../services/scorecard.service';
import { MatchupService } from '../../../settings/services/matchup.service';
import { ScoreCalculationService } from '../../services/score-calculation.service';
import { HandicapService } from '../../../../core/services/handicap.service';
import { AverageScoreService } from '../../services/average-score.service';
import { Season, Week, ScoreEntry, Player, PlayerWithFlight } from '../../models/week.model';
import { Matchup } from '../../../settings/services/matchup.service';
import { ScorecardModalComponent } from '../scorecard-modal/scorecard-modal.component';
import { ScorecardData } from '../../models/scorecard.model';
import { HoleScoreBackend } from '../../services/scorecard.service';
import { DateUtilService } from '../../../../core/services/date-util.service';
import { HttpClient } from '@angular/common/http';

interface MatchupWithDetails extends Matchup {
  playerAName?: string;
  playerBName?: string;
  flightName?: string;
  hasChanged?: boolean;
  originalPlayerAScore?: number;
  originalPlayerBScore?: number;
  holeScores?: HoleScoreBackend[];
}

@Component({
  selector: 'app-score-entry',
  standalone: true,
  imports: [CommonModule, FormsModule, ScorecardModalComponent],
  templateUrl: './score-entry.component.html',
  styleUrls: ['./score-entry.component.css'],
  animations: [
    trigger('rowFade', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.98)' }),
        animate('120ms cubic-bezier(.4,0,.2,1)', style({ opacity: 1, transform: 'none' }))
      ]),
      transition(':leave', [
        animate('120ms cubic-bezier(.4,0,.2,1)', style({ opacity: 0, transform: 'scale(0.98)' }))
      ])
    ])
  ]
})
export class ScoreEntryComponent implements OnInit {
  @Input() mode: 'score-entry' | 'matchups' = 'score-entry';

  seasons: Season[] = [];
  weeks: Week[] = [];
  matchups: MatchupWithDetails[] = [];
  players: PlayerWithFlight[] = [];
  selectedSeasonId: string = '';
  selectedWeekId: string = '';
  selectedWeek: Week | null = null;
  isLoading: boolean = false;
  isLoadingData: boolean = false; // For initial data loading (averages, handicaps, matchups)

  // Cache for per-week averages and handicaps
  private weekAverageCache: Map<string, number> = new Map();
  private weekHandicapCache: Map<string, number> = new Map();

  // Pre-loaded averages and handicaps for display (avoids calling methods in template)
  playerAverages: Map<string, number> = new Map();
  playerHandicaps: Map<string, number> = new Map();

  // Cache for hole scores to prevent redundant API calls
  private holeScoresCache: Map<string, HoleScoreBackend[]> = new Map();
  private isLoadingHoleScores = false;

  //Cache handicaps for players in matchups
  private playerHandicapsCache: Map<string, number> = new Map();
  private isLoadingHandicaps = false;

  // Filter for matchups
  matchupFilter: string = '';
  get filteredMatchups(): MatchupWithDetails[] {
    if (!this.matchupFilter.trim()) return this.matchups;
    const filter = this.matchupFilter.trim().toLowerCase();
    return this.matchups.filter(m =>
      (m.playerAName && m.playerAName.toLowerCase().includes(filter)) ||
      (m.playerBName && m.playerBName.toLowerCase().includes(filter)) ||
      (m.flightName && m.flightName.toLowerCase().includes(filter))
    );
  }

  // Scorecard modal properties
  showScorecardModal: boolean = false;
  currentScorecardData: ScorecardData | null = null;
  @ViewChild(ScorecardModalComponent) scorecardModal!: ScorecardModalComponent;

  constructor(
    private scoringService: ScoringService,
    private scorecardService: ScorecardService,
    private matchupService: MatchupService,
    private scoreCalculationService: ScoreCalculationService,
    private handicapService: HandicapService,
    private averageScoreService: AverageScoreService,
    private route: ActivatedRoute,
    private router: Router,
    private dateUtil: DateUtilService,
    private http: HttpClient
  ) { }

  ngOnInit() {
    // Detect mode from route
    this.route.url.subscribe(url => {
      const path = url.map(segment => segment.path).join('/');
      if (path.includes('matchups')) {
        this.mode = 'matchups';
      }
    });

    this.loadSeasons();

    // Check if weekId was passed as query parameter
    this.route.queryParams.subscribe(params => {
      if (params['weekId']) {
        this.loadWeekById(params['weekId']);
      }
    });
  }

  loadSeasons() {
    this.scoringService.getSeasons().subscribe({
      next: (seasons) => {
        this.seasons = seasons;
        // Auto-select the first season if available
        if (seasons.length > 0 && !this.selectedSeasonId) {
          this.selectedSeasonId = seasons[0].id;
          this.onSeasonChange();
        }
      },
      error: (error) => console.error('Error loading seasons:', error)
    });
  }

  onSeasonChange() {
    if (this.selectedSeasonId) {
      this.selectedWeekId = '';
      this.selectedWeek = null;
      this.matchups = [];
      // Clear caches when season changes
      this.weekAverageCache.clear();
      this.weekHandicapCache.clear();
      this.playerAverages.clear();
      this.playerHandicaps.clear();
      this.holeScoresCache.clear();
      this.playerHandicapsCache.clear();
      // Reset loading flags
      this.isLoadingHandicaps = false;
      this.isLoadingHoleScores = false;
      // Only load weeks - players will be loaded when week is selected
      this.loadWeeks();
    }
  }

  loadWeeks() {
    this.scoringService.getWeeksBySeason(this.selectedSeasonId).subscribe({
      next: (weeks) => {
        this.weeks = weeks;
        // Auto-select current week if not already selected
        if (weeks.length > 0 && !this.selectedWeekId) {
          this.selectCurrentWeek();
        }
      },
      error: (error) => console.error('Error loading weeks:', error)
    });
  }

  selectCurrentWeek() {
    if (!this.selectedSeasonId || this.weeks.length === 0) return;

    this.scoringService.getCurrentWeek(this.selectedSeasonId).subscribe({
      next: (currentWeek) => {
        // Check if the current week exists in our weeks list
        const weekExists = this.weeks.find(w => w.id === currentWeek.id);
        if (weekExists) {
          this.selectedWeekId = currentWeek.id;
          this.selectedWeek = currentWeek;
          this.onWeekChange();
        } else {
          // If current week doesn't exist, fall back to the latest week
          this.selectLatestWeek();
        }
      },
      error: (error) => {
        console.error('Error loading current week:', error);
        // Fall back to selecting the latest week if current week API fails
        this.selectLatestWeek();
      }
    });
  }

  selectLatestWeek() {
    if (this.weeks.length > 0) {
      // Select the latest week (highest week number)
      const latestWeek = this.weeks[this.weeks.length - 1];
      this.selectedWeekId = latestWeek.id;
      this.selectedWeek = latestWeek;
      this.onWeekChange();
    }
  }

  loadWeekById(weekId: string) {
    this.scoringService.getWeekById(weekId).subscribe({
      next: (week) => {
        this.selectedSeasonId = week.seasonId;
        this.selectedWeekId = week.id;
        this.selectedWeek = week;
        this.loadSeasons();
        this.loadWeeks();
        // Use the robust loading method
        this.loadPlayersAndMatchups();
      },
      error: (error) => console.error('Error loading week:', error)
    });
  }

  onWeekChange() {
    if (this.selectedWeekId) {
      this.selectedWeek = this.weeks.find(w => w.id === this.selectedWeekId) || null;
      // Clear the caches when week changes
      this.weekAverageCache.clear();
      this.weekHandicapCache.clear();
      this.playerAverages.clear();
      this.playerHandicaps.clear();
      this.holeScoresCache.clear();
      this.playerHandicapsCache.clear();
      // Reset loading flags
      this.isLoadingHandicaps = false;
      this.isLoadingHoleScores = false;
      // Use the robust loading method to avoid timing issues
      this.loadPlayersAndMatchups();
    } else {
      this.selectedWeek = null;
      this.matchups = [];
      this.playerAverages.clear();
      this.holeScoresCache.clear();
      this.playerHandicapsCache.clear();
      // Reset loading flags
      this.isLoadingHandicaps = false;
      this.isLoadingHoleScores = false;
    }
  }

  loadPlayers() {
    if (!this.selectedSeasonId) return;

    console.log('Loading players for season:', this.selectedSeasonId);
    this.scoringService.getPlayersInFlights(this.selectedSeasonId).subscribe({
      next: (players: PlayerWithFlight[]) => {
        console.log('Players loaded:', players.length, 'players');
        this.players = players;

        // Re-enrich matchups with player details after players are loaded
        if (this.matchups.length > 0) {
          console.log('Re-enriching', this.matchups.length, 'matchups with player details');
          this.matchups = this.matchups.map(matchup => this.enrichMatchupWithDetails(matchup));
        }
      },
      error: (error) => console.error('Error loading players:', error)
    });
  }

  // Load both players and matchups simultaneously to avoid timing issues
  async loadPlayersAndMatchups() {
    if (!this.selectedSeasonId || !this.selectedWeekId) return;

    this.isLoadingData = true;
    console.log('Loading players and matchups simultaneously');

    forkJoin({
      players: this.scoringService.getPlayersInFlights(this.selectedSeasonId),
      matchups: this.matchupService.getMatchupsByWeek(this.selectedWeekId)
    }).subscribe({
      next: async ({ players, matchups }) => {
        console.log('Loaded:', players.length, 'players and', matchups.length, 'matchups');

        // Set players first
        this.players = players;

        // Then enrich matchups with player details
        this.matchups = this.sortMatchupsByFlight(matchups.map(matchup => this.enrichMatchupWithDetails(matchup)));

        // Pre-load player averages for all players in matchups
        this.preLoadPlayerAverages();

        // Load hole scores
        this.loadHoleScoresForMatchups();
        await this.loadPlayerHandicaps();
        this.isLoadingData = false;
      },
      error: (error) => {
        console.error('Error loading players and matchups:', error);
        this.isLoadingData = false;
      }
    });
  }

  // Helper method to refresh hole scores for a specific matchup
  private refreshMatchupHoleScores(matchupId: string): Promise<HoleScoreBackend[]> {
    return this.scorecardService.getScorecard(matchupId).toPromise().then(holeScores => {
      // Update cache
      this.holeScoresCache.set(matchupId, holeScores || []);

      // Update matchup if it exists in current list
      const matchup = this.matchups.find(m => m.id === matchupId);
      if (matchup) {
        matchup.holeScores = holeScores || [];
      }

      return holeScores || [];
    }).catch(error => {
      console.error('Error refreshing hole scores for matchup:', matchupId, error);
      return [];
    });
  }

  loadHoleScoresForMatchups() {
    console.log('loadHoleScoresForMatchups called, isLoading:', this.isLoadingHoleScores);

    // Prevent multiple simultaneous loads
    if (this.isLoadingHoleScores) {
      console.log('Already loading hole scores, skipping duplicate call');
      return;
    }

    // Filter matchups that need hole scores and aren't already cached
    const matchupsToLoad = this.matchups.filter(matchup => {
      if (!matchup.id) return false;

      // Check if already cached
      if (this.holeScoresCache.has(matchup.id)) {
        matchup.holeScores = this.holeScoresCache.get(matchup.id)!;
        return false;
      }

      return true;
    });

    if (matchupsToLoad.length === 0) {
      console.log('All hole scores already cached');
      return;
    }

    console.log(`Loading hole scores for ${matchupsToLoad.length} matchups`);
    this.isLoadingHoleScores = true;

    // Load hole scores for matchups that need them
    const requests = matchupsToLoad.map(matchup =>
      this.scorecardService.getScorecard(matchup.id!).toPromise().then(holeScores => {
        // Cache the result
        this.holeScoresCache.set(matchup.id!, holeScores || []);
        matchup.holeScores = holeScores || [];
        return { matchupId: matchup.id!, holeScores: holeScores || [] };
      }).catch(error => {
        // Cache empty result for failed requests to avoid retry
        this.holeScoresCache.set(matchup.id!, []);
        matchup.holeScores = [];
        console.warn(`Failed to load hole scores for matchup ${matchup.id}:`, error);
        return { matchupId: matchup.id!, holeScores: [] };
      })
    );

    Promise.all(requests).then(results => {
      console.log(`Loaded hole scores for ${results.length} matchups`);
      this.isLoadingHoleScores = false;
    }).catch(error => {
      console.error('Error loading hole scores:', error);
      this.isLoadingHoleScores = false;
    });
  }

  enrichMatchupWithDetails(matchup: Matchup): MatchupWithDetails {
    const playerA = this.players.find(p => p.id === matchup.playerAId);
    const playerB = this.players.find(p => p.id === matchup.playerBId);

    // Debug logging only if players are missing
    if (!playerA || !playerB) {
      console.warn('Player lookup failed for matchup:', {
        matchupId: matchup.id,
        playerAId: matchup.playerAId,
        playerBId: matchup.playerBId,
        playerAFound: !!playerA,
        playerBFound: !!playerB,
        totalPlayers: this.players.length,
        playerIds: this.players.map(p => p.id).slice(0, 5) // Show first 5 IDs
      });
    }

    return {
      ...matchup,
      playerAName: playerA ? `${playerA.firstName} ${playerA.lastName}` : `Player ${matchup.playerAId}`,
      playerBName: playerB ? `${playerB.firstName} ${playerB.lastName}` : `Player ${matchup.playerBId}`,
      flightName: playerA?.flightName || playerB?.flightName || 'Unknown Flight',
      hasChanged: false,
      originalPlayerAScore: matchup.playerAScore,
      originalPlayerBScore: matchup.playerBScore
    };
  }

  // Sort matchups by flight name (default)
  private sortMatchupsByFlight(matchups: MatchupWithDetails[]): MatchupWithDetails[] {
    return matchups.slice().sort((a, b) => {
      if (!a.flightName && !b.flightName) return 0;
      if (!a.flightName) return 1;
      if (!b.flightName) return -1;
      return a.flightName.localeCompare(b.flightName, undefined, { numeric: true });
    });
  }

  onScoreChange(matchup: MatchupWithDetails) {
    matchup.hasChanged = (
      matchup.playerAScore !== matchup.originalPlayerAScore ||
      matchup.playerBScore !== matchup.originalPlayerBScore
    );
  }
  getMatchupWinner(matchup: MatchupWithDetails): string {
    // Handle absence scenarios first
    if (matchup.playerAAbsent && matchup.playerBAbsent) {
      return 'Both Absent';
    }
    if (matchup.playerAAbsent) {
      return matchup.playerBName || 'Player B';
    }
    if (matchup.playerBAbsent) {
      return matchup.playerAName || 'Player A';
    }

    // Use match play points if available (this is the correct way for match play scoring)
    if (matchup.playerAPoints !== null && matchup.playerAPoints !== undefined &&
      matchup.playerBPoints !== null && matchup.playerBPoints !== undefined) {

      if (matchup.playerAPoints > matchup.playerBPoints) {
        return matchup.playerAName || 'Player A';
      } else if (matchup.playerBPoints > matchup.playerAPoints) {
        return matchup.playerBName || 'Player B';
      } else {
        return 'Tie';
      }
    }

    // Fall back to gross scores if match play points aren't available
    if (!matchup.playerAScore || !matchup.playerBScore) return '';

    if (matchup.playerAScore < matchup.playerBScore) {
      return matchup.playerAName || 'Player A';
    } else if (matchup.playerBScore < matchup.playerAScore) {
      return matchup.playerBName || 'Player B';
    } else {
      return 'Tie';
    }
  }

  getMatchupStatus(matchup: MatchupWithDetails): string {
    // Handle absence scenarios first
    if (matchup.playerAAbsent === true && matchup.playerBAbsent === true) {
      return 'Both Absent'; // treat as completed
    }
    if (matchup.playerAAbsent === true) {
      return 'Completed (A Absent)'; // treat as completed
    }
    if (matchup.playerBAbsent === true) {
      return 'Completed (B Absent)'; // treat as completed
    }

    // If no hole scores are loaded yet, fall back to simple total score check
    if (!matchup.holeScores || matchup.holeScores.length === 0) {
      if (matchup.playerAScore && matchup.playerBScore) {
        return 'Completed';
      } else if (matchup.playerAScore || matchup.playerBScore) {
        return 'Partial';
      } else {
        return 'Pending';
      }
    }

    // Check hole-by-hole completion status
    const front9Holes = matchup.holeScores.filter(h => h.holeNumber >= 1 && h.holeNumber <= 9);
    const back9Holes = matchup.holeScores.filter(h => h.holeNumber >= 10 && h.holeNumber <= 18);

    // Count completed holes for each 9
    const front9Completed = front9Holes.filter(h =>
      (h.playerAScore !== null && h.playerAScore !== undefined) &&
      (h.playerBScore !== null && h.playerBScore !== undefined)
    ).length;

    const back9Completed = back9Holes.filter(h =>
      (h.playerAScore !== null && h.playerAScore !== undefined) &&
      (h.playerBScore !== null && h.playerBScore !== undefined)
    ).length;

    // Count any partial entries in each 9
    const front9Partial = front9Holes.filter(h =>
      (h.playerAScore !== null && h.playerAScore !== undefined) ||
      (h.playerBScore !== null && h.playerBScore !== undefined)
    ).length;

    const back9Partial = back9Holes.filter(h =>
      (h.playerAScore !== null && h.playerAScore !== undefined) ||
      (h.playerBScore !== null && h.playerBScore !== undefined)
    ).length;

    // Logic: Completed if exactly one 9-hole set is complete (9 holes) and the other has no entries
    const front9IsComplete = front9Completed === 9;
    const back9IsComplete = back9Completed === 9;
    const front9HasNoEntries = front9Partial === 0;
    const back9HasNoEntries = back9Partial === 0;

    if ((front9IsComplete && back9HasNoEntries) || (back9IsComplete && front9HasNoEntries)) {
      return 'Completed';
    }

    // If any holes have been started, it's partial
    if (front9Partial > 0 || back9Partial > 0) {
      return 'Partial';
    }

    // No holes entered
    return 'Pending';
  }

  getMatchupStatusClass(matchup: MatchupWithDetails): string {
    const status = this.getMatchupStatus(matchup);
    switch (status) {
      case 'Completed': return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200';
      case 'Partial': return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200';
      case 'Both Absent':
      case 'Player A Absent':
      case 'Player B Absent': return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200';
      case 'Pending': return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
      default: return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
    }
  }

  getCompletedMatchupsCount(): number {
    return this.matchups.filter(m =>
      (m.playerAScore && m.playerBScore) || // Both players have scores
      (m.playerAAbsent === true) || (m.playerBAbsent === true)     // Or at least one player is absent
    ).length;
  }

  hasUnsavedChanges(): boolean {
    return this.matchups.some(m => m.hasChanged);
  }

  saveMatchupScore(matchup: MatchupWithDetails) {
    if (!matchup.id) return;

    this.isLoading = true;
    this.matchupService.updateMatchup(matchup).subscribe({
      next: async () => {
        matchup.originalPlayerAScore = matchup.playerAScore;
        matchup.originalPlayerBScore = matchup.playerBScore;
        matchup.hasChanged = false;
        
        const playerIds = [matchup.playerAId, matchup.playerBId].filter(id => id) as string[];
        
        // Refresh both player averages and handicaps since scores have changed
        this.refreshPlayerAverages(playerIds);
        await this.refreshPlayerHandicaps(playerIds);
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error saving matchup score:', error);
        this.isLoading = false;
      }
    });
  }

  saveAllScores() {
    const matchupsToSave = this.matchups.filter(m => m.hasChanged);
    if (matchupsToSave.length === 0) return;

    this.isLoading = true;
    let savedCount = 0;
    const affectedPlayerIds = new Set<string>();

    matchupsToSave.forEach(matchup => {
      if (matchup.id) {
        // Track affected players for average and handicap refresh
        if (matchup.playerAId) affectedPlayerIds.add(matchup.playerAId);
        if (matchup.playerBId) affectedPlayerIds.add(matchup.playerBId);

        this.matchupService.updateMatchup(matchup).subscribe({
          next: () => {
            matchup.originalPlayerAScore = matchup.playerAScore;
            matchup.originalPlayerBScore = matchup.playerBScore;
            matchup.hasChanged = false;
            savedCount++;

            if (savedCount === matchupsToSave.length) {
              // Refresh both averages and handicaps for all affected players
              const playerIds = Array.from(affectedPlayerIds);
              this.refreshPlayerAverages(playerIds);
              this.refreshPlayerHandicaps(playerIds).then(() => {
                this.isLoading = false;
              });
            }
          },
          error: (error) => {
            console.error('Error saving matchup score:', error);
            savedCount++;

            if (savedCount === matchupsToSave.length) {
              // Refresh averages and handicaps even on errors
              const playerIds = Array.from(affectedPlayerIds);
              this.refreshPlayerAverages(playerIds);
              this.refreshPlayerHandicaps(playerIds).then(() => {
                this.isLoading = false;
              });
            }
          }
        });
      }
    });
  }

  clearMatchupScores(matchup: MatchupWithDetails) {
    if (!matchup.id) return;

    this.isLoading = true;

    // Clear scorecard data from backend
    this.scorecardService.deleteScorecard(matchup.id).subscribe({
      next: async () => {
        // Clear cache
        this.holeScoresCache.delete(matchup.id!);
        // Clear cached hole scores
        matchup.holeScores = [];
        // Clear total scores
        matchup.playerAScore = undefined;
        matchup.playerBScore = undefined;
        this.onScoreChange(matchup);
        
        // Refresh averages and handicaps since scores were cleared
        const playerIds = [matchup.playerAId, matchup.playerBId].filter(id => id) as string[];
        this.refreshPlayerAverages(playerIds);
        await this.refreshPlayerHandicaps(playerIds);
        
        this.isLoading = false;
        console.log('Scorecard cleared successfully');
      },
      error: async (error) => {
        // Even if backend delete fails, clear the frontend data
        console.warn('Failed to clear scorecard from backend, clearing frontend data only:', error);
        this.holeScoresCache.delete(matchup.id!);
        matchup.holeScores = [];
        matchup.playerAScore = undefined;
        matchup.playerBScore = undefined;
        this.onScoreChange(matchup);
        
        // Refresh averages and handicaps even on error
        const playerIds = [matchup.playerAId, matchup.playerBId].filter(id => id) as string[];
        this.refreshPlayerAverages(playerIds);
        await this.refreshPlayerHandicaps(playerIds);
        
        this.isLoading = false;
      }
    });
  }

  clearAllScores() {
    if (this.matchups.length === 0) return;

    this.isLoading = true;
    let clearedCount = 0;
    const totalMatchups = this.matchups.length;
    const affectedPlayerIds = new Set<string>();

    this.matchups.forEach(matchup => {
      // Track affected players
      if (matchup.playerAId) affectedPlayerIds.add(matchup.playerAId);
      if (matchup.playerBId) affectedPlayerIds.add(matchup.playerBId);

      if (matchup.id) {
        // Clear scorecard data from backend
        this.scorecardService.deleteScorecard(matchup.id).subscribe({
          next: () => {
            // Clear cache
            this.holeScoresCache.delete(matchup.id!);
            // Clear cached hole scores and total scores
            matchup.holeScores = [];
            matchup.playerAScore = undefined;
            matchup.playerBScore = undefined;
            this.onScoreChange(matchup);

            clearedCount++;
            if (clearedCount === totalMatchups) {
              // Refresh averages and handicaps for all affected players
              const playerIds = Array.from(affectedPlayerIds);
              this.refreshPlayerAverages(playerIds);
              this.refreshPlayerHandicaps(playerIds).then(() => {
                this.isLoading = false;
                console.log('All scorecards cleared successfully');
              });
            }
          },
          error: (error) => {
            // Even if backend delete fails, clear the frontend data
            console.warn(`Failed to clear scorecard for matchup ${matchup.id}, clearing frontend data only:`, error);
            this.holeScoresCache.delete(matchup.id!);
            matchup.holeScores = [];
            matchup.playerAScore = undefined;
            matchup.playerBScore = undefined;
            this.onScoreChange(matchup);

            clearedCount++;
            if (clearedCount === totalMatchups) {
              // Refresh averages and handicaps even on errors
              const playerIds = Array.from(affectedPlayerIds);
              this.refreshPlayerAverages(playerIds);
              this.refreshPlayerHandicaps(playerIds).then(() => {
                this.isLoading = false;
              });
            }
          }
        });
      } else {
        // If no matchup ID, just clear frontend data and cache
        this.holeScoresCache.delete(matchup.id!);
        matchup.holeScores = [];
        matchup.playerAScore = undefined;
        matchup.playerBScore = undefined;
        this.onScoreChange(matchup);

        clearedCount++;
        if (clearedCount === totalMatchups) {
          // Refresh averages and handicaps for all affected players
          const playerIds = Array.from(affectedPlayerIds);
          this.refreshPlayerAverages(playerIds);
          this.refreshPlayerHandicaps(playerIds).then(() => {
            this.isLoading = false;
          });
        }
      }
    });
  }

  getWeekDisplayName(week: Week): string {
    const weekDate = this.dateUtil.formatDateShort(week.date);
    return `${week.name} (${weekDate})`;
  }

  trackByMatchupId(index: number, matchup: MatchupWithDetails): string {
    return matchup.id || index.toString();
  }

  // Scorecard modal methods
  openScorecardModal(matchup: MatchupWithDetails) {
    if (!matchup.playerAName || !matchup.playerBName) {
      console.error('Matchup is missing player names');
      return;
    }

    // Fetch player handicaps before opening modal
    this.scoringService.getPlayers().subscribe({
      next: (players) => {
        const playerA = players.find(p => p.id === matchup.playerAId);
        const playerB = players.find(p => p.id === matchup.playerBId);

        this.currentScorecardData = {
          matchupId: matchup.id || '',
          playerAId: matchup.playerAId || '',
          playerBId: matchup.playerBId || '',
          playerAName: matchup.playerAName || '',
          playerBName: matchup.playerBName || '',
          flightName: matchup.flightName || 'Unknown Flight',
          holes: [], // Will be initialized by the modal
          playerATotalScore: matchup.playerAScore || 0,
          playerBTotalScore: matchup.playerBScore || 0,
          playerAHolesWon: 0,
          playerBHolesWon: 0,
          holesHalved: 0,
          // Use the new handicap cache
          playerAHandicap: this.playerHandicapsCache.get(matchup.playerAId) || playerA?.initialHandicap || 0,
          playerBHandicap: this.playerHandicapsCache.get(matchup.playerBId) || playerB?.initialHandicap || 0
        };

        this.showScorecardModal = true;

        // Manually trigger scorecard initialization after the view is rendered
        setTimeout(() => {
          if (this.scorecardModal) {
            console.log('Manually triggering scorecard initialization');
            this.scorecardModal.initializeScorecard();
          }
        }, 0);
      },
      error: (error) => {
        console.error('Error fetching player handicaps:', error);
        // Open modal without handicaps as fallback
        this.currentScorecardData = {
          matchupId: matchup.id || '',
          playerAId: matchup.playerAId || '',
          playerBId: matchup.playerBId || '',
          playerAName: matchup.playerAName || '',
          playerBName: matchup.playerBName || '',
          flightName: matchup.flightName || 'Unknown Flight',
          holes: [], // Will be initialized by the modal
          playerATotalScore: matchup.playerAScore || 0,
          playerBTotalScore: matchup.playerBScore || 0,
          playerAHolesWon: 0,
          playerBHolesWon: 0,
          holesHalved: 0,
          playerAHandicap: 0,
          playerBHandicap: 0
        };

        this.showScorecardModal = true;

        // Manually trigger scorecard initialization after the view is rendered
        setTimeout(() => {
          if (this.scorecardModal) {
            console.log('Manually triggering scorecard initialization (fallback)');
            this.scorecardModal.initializeScorecard();
          }
        }, 0);
      }
    });
  }

  onScorecardSave(scorecardData: ScorecardData) {
    // Find the matching matchup and update scores
    const matchup = this.matchups.find(m => m.id === scorecardData.matchupId);
    if (matchup) {
      matchup.playerAScore = scorecardData.playerATotalScore;
      matchup.playerBScore = scorecardData.playerBTotalScore;
      this.onScoreChange(matchup);

      const playerIds = [matchup.playerAId, matchup.playerBId].filter(id => id) as string[];

      // Refresh both player averages and handicaps since scores have changed
      this.refreshPlayerAverages(playerIds);
      this.refreshPlayerHandicaps(playerIds);

      // Refresh hole scores for this matchup using cached method
      this.refreshMatchupHoleScores(scorecardData.matchupId).then(() => {
        console.log('Hole scores refreshed for matchup:', scorecardData.matchupId);
      });
    }

    // Show success message - the scorecard has been saved to the backend
    console.log('Scorecard saved successfully:', scorecardData);

    this.closeScorecardModal();
  }

  closeScorecardModal() {
    this.showScorecardModal = false;
    this.currentScorecardData = null;

    // Note: We don't need to reload all data here since onScorecardSave already
    // refreshes the specific player averages and handicaps, and refreshes hole scores
    // Only refresh matchup data to get updated points (without full reload)
    if (this.selectedWeekId) {
      this.matchupService.getMatchupsByWeek(this.selectedWeekId).subscribe({
        next: (matchups) => {
          // Update matchups while preserving enriched player details
          this.matchups = this.sortMatchupsByFlight(matchups.map(matchup => this.enrichMatchupWithDetails(matchup)));
        },
        error: (error) => console.error('Error refreshing matchups:', error)
      });
    }
  }

  formatWeekDate(week: Week): string {
    return this.dateUtil.formatDateOnly(week.date);
  }

  getPlayerMatchPoints(matchup: MatchupWithDetails, player: 'A' | 'B'): string {
    // Return match play points if available
    const points = player === 'A' ? matchup.playerAPoints : matchup.playerBPoints;
    return points !== null && points !== undefined ? points.toString() : '-';
  }

  getPlayerNetScore(matchup: MatchupWithDetails, player: 'A' | 'B'): string {
    // If no scores available, return dash
    if (!matchup.playerAScore || !matchup.playerBScore) {
      return '-';
    }

    // If we don't have player handicap information, fall back to gross score
    const playerAHandicap = this.getPlayerHandicap(matchup.playerAId);
    const playerBHandicap = this.getPlayerHandicap(matchup.playerBId);

    if (playerAHandicap === null || playerBHandicap === null) {
      // Return gross score if handicaps not available
      return player === 'A' ? (matchup.playerAScore?.toString() || '-') : (matchup.playerBScore?.toString() || '-');
    }

    const grossScore = player === 'A' ? matchup.playerAScore : matchup.playerBScore;
    if (!grossScore) return '-';

    // Use backend logic for net score calculation
    const playerHandicap = player === 'A' ? playerAHandicap : playerBHandicap;
    const opponentHandicap = player === 'A' ? playerBHandicap : playerAHandicap;

    // --- 9-hole stroke allocation fix ---
    // Only allocate strokes to the hardest holes within the 9 being played
    // For score-entry, assume holes 1-9 (adjust if back 9 is supported)
    const holesInPlay = Array.from({ length: 9 }, (_, i) => ({ number: i + 1, handicap: i + 1 })); // Replace with real hole data if available
    const holesWithHandicap = holesInPlay.filter(h => typeof h.handicap === 'number');
    const handicapDifference = Math.abs(playerHandicap - opponentHandicap);
    const playerReceivesStrokes = playerHandicap > opponentHandicap;
    let strokesOnThisHole = 0;
    if (playerReceivesStrokes) {
      const hardestHoles = [...holesWithHandicap]
        .sort((a, b) => (a.handicap ?? 99) - (b.handicap ?? 99))
        .slice(0, Math.round(handicapDifference))
        .map(h => h.number);
      // Find the current hole number (if available)
      const currentHoleNumber = matchup.holeScores && matchup.holeScores.length === 9 ? matchup.holeScores.find((h, idx) => (player === 'A' ? h.playerAScore : h.playerBScore) === grossScore)?.holeNumber : null;
      if (currentHoleNumber && hardestHoles.includes(currentHoleNumber)) {
        strokesOnThisHole = 1;
      }
    }
    const netScore = grossScore - strokesOnThisHole;
    return netScore.toString();
  }

  getPlayerAverageScore(playerId: string | undefined): string {
    if (!playerId) return '-';

    // Use pre-loaded average score from playerAverages map
    const average = this.playerAverages.get(playerId);
    if (average !== undefined) {
      return average.toFixed(2);
    }

    // Return placeholder if not yet loaded
    return '...';
  }

<<<<<<< Updated upstream

=======
  /**
   * Pre-load player averages for all players in the current matchups to avoid excessive API calls
   */
>>>>>>> Stashed changes
  private preLoadPlayerAverages(): void {
    if (!this.selectedWeek || this.matchups.length === 0) return;

    // Get unique player IDs from all matchups
    const playerIds = new Set<string>();
    this.matchups.forEach(matchup => {
      if (matchup.playerAId) playerIds.add(matchup.playerAId);
      if (matchup.playerBId) playerIds.add(matchup.playerBId);
    });

    // Fetch both average scores and handicaps for all players
    const averageRequests = Array.from(playerIds).map(playerId => {
      // Determine if this player has completed scores for the current week
      const playerMatchups = this.matchups.filter(m => 
        m.playerAId === playerId || m.playerBId === playerId
      );
      
      const hasCompletedScores = playerMatchups.some(matchup => {
        const playerAComplete = matchup.playerAId === playerId && matchup.playerAScore;
        const playerBComplete = matchup.playerBId === playerId && matchup.playerBScore;
        return playerAComplete || playerBComplete;
      });

      // If player has completed scores, include current week (weekNumber)
      // If not, exclude current week (weekNumber - 1)
      const weekNumber = hasCompletedScores ? this.selectedWeek!.weekNumber : this.selectedWeek!.weekNumber - 1;
      const cacheKey = `${playerId}-${this.selectedWeek!.id}-${hasCompletedScores}`;

      // Check if already cached with the current completion status
      if (this.weekAverageCache.has(cacheKey)) {
        this.playerAverages.set(playerId, this.weekAverageCache.get(cacheKey)!);
        return Promise.resolve(this.weekAverageCache.get(cacheKey)!);
      }

<<<<<<< Updated upstream
      // Fetch from AverageScoreService for average score up to week
      return this.averageScoreService.getPlayerAverageScoreUpToWeek(playerId, this.selectedWeek!.seasonId, this.selectedWeek!.weekNumber)
        .toPromise().then((average: number | undefined) => {
          const avgValue = average || 0;
          this.weekAverageCache.set(cacheKey, avgValue);
          this.playerAverages.set(playerId, avgValue);
          return avgValue;
        }).catch((error: any) => {
          console.error('Error fetching player week average:', error);
          this.weekAverageCache.set(cacheKey, 0);
          this.playerAverages.set(playerId, 0);
          return 0;
        });
    });

    const handicapRequests = Array.from(playerIds).map(playerId => {
      const cacheKey = `${playerId}-${this.selectedWeek!.id}`;

      // Check if already cached
      if (this.weekHandicapCache.has(cacheKey)) {
        this.playerHandicaps.set(playerId, this.weekHandicapCache.get(cacheKey)!);
        return Promise.resolve(this.weekHandicapCache.get(cacheKey)!);
      }

      // Fetch from HandicapService for handicap up to week
      return this.handicapService.getPlayerHandicapUpToWeek(playerId, this.selectedWeek!.seasonId, this.selectedWeek!.weekNumber)
        .toPromise().then((handicap: number | undefined) => {
          const hcpValue = handicap || 0;
          this.weekHandicapCache.set(cacheKey, hcpValue);
          this.playerHandicaps.set(playerId, hcpValue);
          return hcpValue;
        }).catch((error: any) => {
          console.error('Error fetching player week handicap:', error);
          this.weekHandicapCache.set(cacheKey, 0);
          this.playerHandicaps.set(playerId, 0);
          return 0;
        });
=======
      const url = `/api/averagescore/player/${playerId}/season/${this.selectedWeek!.seasonId}/uptoweek/${weekNumber}`;
      return this.http.get<number>(url).toPromise().then(average => {
        this.weekAverageCache.set(cacheKey, average!);
        this.playerAverages.set(playerId, average!);
        return average!;
      }).catch(error => {
        console.error('Error fetching player week average:', error);
        this.weekAverageCache.set(cacheKey, 0);
        this.playerAverages.set(playerId, 0);
        return 0;
      });
>>>>>>> Stashed changes
    });

    // Wait for all requests to complete
    Promise.all([...averageRequests, ...handicapRequests]).then(() => {
      console.log(`Pre-loaded averages and handicaps for ${playerIds.size} players`);
    });
  }

  showPlayerStats(matchup: MatchupWithDetails): boolean {
    // Show stats if we have player information
    return !!matchup.playerAId && !!matchup.playerBId;
  }

  public getPlayerHandicapForDisplay(playerId: string | undefined): string {
    const handicap = this.getPlayerHandicap(playerId);
    return handicap !== null ? handicap.toString() : '-';
  }

  private getPlayerHandicap(playerId: string | undefined): number | null {
    if (!playerId) return null;

<<<<<<< Updated upstream
    // Use the pre-loaded handicap from playerHandicaps map
    const handicap = this.playerHandicaps.get(playerId);
    if (handicap !== undefined) {
      return handicap;
    }

    // Fallback to initial handicap if not yet loaded
=======
    // Use the new player handicaps cache
    if (this.playerHandicapsCache.has(playerId)) {
      return this.playerHandicapsCache.get(playerId)!;
    }

    // Fallback to initial handicap if not in cache
>>>>>>> Stashed changes
    const player = this.players.find(p => p.id === playerId);
    return player?.initialHandicap || null;
  }

  getPlayerFirstName(fullName: string | undefined): string {
    if (!fullName) return '';
    return fullName.split(' ')[0] || '';
  }

  downloadWeekScorecardPdf(): void {
    if (!this.selectedWeekId) return;
    const url = `/api/pdf/scorecard/week/${this.selectedWeekId}`;
    this.isLoading = true;
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const weekNumber = this.selectedWeek?.weekNumber || 'Unknown';
        const fileName = `Scorecard_Week_${weekNumber}.pdf`;
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName;
        link.click();
        window.URL.revokeObjectURL(link.href);
        this.isLoading = false;
      },
      error: (err) => {
        alert('Failed to generate PDF: ' + (err?.error || err));
        this.isLoading = false;
      }
    });
  }

  downloadWeekSummaryPdf(): void {
    if (!this.selectedWeekId) return;
    const url = `/api/pdf/summary/week/${this.selectedWeekId}`;
    this.isLoading = true;
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const weekNumber = this.selectedWeek?.weekNumber || 'Unknown';
        const fileName = `Summary_Week_${weekNumber}.pdf`;
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName;
        link.click();
        window.URL.revokeObjectURL(link.href);
        this.isLoading = false;
      },
      error: (err) => {
        alert('Failed to generate summary PDF: ' + (err?.error || err));
        this.isLoading = false;
      }
    });
  }

  private async loadPlayerHandicaps() {
    if (!this.selectedSeasonId || !this.selectedWeek) {
      return;
    }

    // Prevent multiple simultaneous handicap loads
    if (this.isLoadingHandicaps) {
      console.log('Already loading handicaps, skipping duplicate call');
      return;
    }

    this.isLoadingHandicaps = true;
    console.log('Loading player handicaps...');

    try {
      // Get unique player IDs from all matchups
      const playerIds = new Set<string>();
      this.matchups.forEach(matchup => {
        if (matchup.playerAId) playerIds.add(matchup.playerAId);
        if (matchup.playerBId) playerIds.add(matchup.playerBId);
      });

      // Check if any players have completed scores for the current week
      const hasAnyCompletedScores = Array.from(playerIds).some(playerId => {
        const playerMatchups = this.matchups.filter(m => 
          m.playerAId === playerId || m.playerBId === playerId
        );
        
        return playerMatchups.some(matchup => {
          const playerAComplete = matchup.playerAId === playerId && matchup.playerAScore;
          const playerBComplete = matchup.playerBId === playerId && matchup.playerBScore;
          return playerAComplete || playerBComplete;
        });
      });

      // If any player has completed scores, include current week
      // If not, exclude current week (use previous week)
      const weekNumber = hasAnyCompletedScores ? this.selectedWeek.weekNumber : this.selectedWeek.weekNumber - 1;
      
      // Use bulk API to get all handicaps at once
      const handicapsDict = await firstValueFrom(this.handicapService.getAllPlayerUptoWeekHandicaps(this.selectedSeasonId, weekNumber));
      
      // Clear and populate the handicaps cache
      this.playerHandicapsCache.clear();
      
      // Convert dictionary to cache
      Object.entries(handicapsDict).forEach(([playerId, handicap]) => {
        this.playerHandicapsCache.set(playerId, handicap);
      });
      
      console.log(`Loaded handicaps for ${this.playerHandicapsCache.size} players using bulk API`);
    } catch (error) {
      console.error('Error loading player handicaps:', error);
    } finally {
      this.isLoadingHandicaps = false;
    }
  }

  /**
   * Refresh player averages for specific players after scores have been updated
   */
  private refreshPlayerAverages(playerIds: string[]): void {
    if (!this.selectedWeek || playerIds.length === 0) return;

    // Clear old cache entries for these players
    playerIds.forEach(playerId => {
      // Remove both completion states from cache
      this.weekAverageCache.delete(`${playerId}-${this.selectedWeek!.id}-true`);
      this.weekAverageCache.delete(`${playerId}-${this.selectedWeek!.id}-false`);
    });

    // Fetch fresh averages for these players
    const averageRequests = playerIds.map(playerId => {
      // Determine if this player has completed scores for the current week
      const playerMatchups = this.matchups.filter(m => 
        m.playerAId === playerId || m.playerBId === playerId
      );
      
      const hasCompletedScores = playerMatchups.some(matchup => {
        const playerAComplete = matchup.playerAId === playerId && matchup.playerAScore;
        const playerBComplete = matchup.playerBId === playerId && matchup.playerBScore;
        return playerAComplete || playerBComplete;
      });

      // If player has completed scores, include current week (weekNumber)
      // If not, exclude current week (weekNumber - 1)
      const weekNumber = hasCompletedScores ? this.selectedWeek!.weekNumber : this.selectedWeek!.weekNumber - 1;
      const cacheKey = `${playerId}-${this.selectedWeek!.id}-${hasCompletedScores}`;
      const url = `/api/averagescore/player/${playerId}/season/${this.selectedWeek!.seasonId}/uptoweek/${weekNumber}`;
      
      return this.http.get<number>(url).toPromise().then(average => {
        this.weekAverageCache.set(cacheKey, average!);
        this.playerAverages.set(playerId, average!);
        return average!;
      }).catch(error => {
        console.error('Error refreshing player average:', error);
        return 0;
      });
    });

    Promise.all(averageRequests).then(() => {
      console.log(`Refreshed averages for ${playerIds.length} players`);
    });
  }

  /**
   * Refresh player handicaps for specific players after scores have been updated
   */
  private async refreshPlayerHandicaps(playerIds: string[]): Promise<void> {
    if (!this.selectedSeasonId || !this.selectedWeek || playerIds.length === 0) return;

    console.log(`Refreshing handicaps for ${playerIds.length} players...`);

    try {
      // For small numbers of players (1-2), use individual calls to avoid over-fetching
      // For larger numbers, use bulk API
      if (playerIds.length <= 2) {
        // Use individual calls for small updates
        const handicapRequests = playerIds.map(async (playerId) => {
          // Determine if this player has completed scores for the current week
          const playerMatchups = this.matchups.filter(m => 
            m.playerAId === playerId || m.playerBId === playerId
          );
          
          const hasCompletedScores = playerMatchups.some(matchup => {
            const playerAComplete = matchup.playerAId === playerId && matchup.playerAScore;
            const playerBComplete = matchup.playerBId === playerId && matchup.playerBScore;
            return playerAComplete || playerBComplete;
          });

          // If player has completed scores, include current week
          // If not, exclude current week (use previous week)
          const weekNumber = hasCompletedScores ? this.selectedWeek!.weekNumber : this.selectedWeek!.weekNumber - 1;
          
          try {
            const handicap = await firstValueFrom(this.handicapService.getPlayerSessionHandicap(playerId, this.selectedSeasonId, weekNumber));
            return { playerId, handicap: handicap as number };
          } catch (error) {
            console.error(`Error refreshing handicap for player ${playerId}:`, error);
            return { playerId, handicap: 0 };
          }
        });

        // Wait for all handicap requests to complete
        const results = await Promise.all(handicapRequests);
        
        // Update the handicaps cache
        results.forEach(({ playerId, handicap }) => {
          this.playerHandicapsCache.set(playerId, handicap);
        });
      } else {
        // Use bulk API for larger updates
        // Check if any of the affected players have completed scores for the current week
        const hasAnyCompletedScores = playerIds.some(playerId => {
          const playerMatchups = this.matchups.filter(m => 
            m.playerAId === playerId || m.playerBId === playerId
          );
          
          return playerMatchups.some(matchup => {
            const playerAComplete = matchup.playerAId === playerId && matchup.playerAScore;
            const playerBComplete = matchup.playerBId === playerId && matchup.playerBScore;
            return playerAComplete || playerBComplete;
          });
        });

        const weekNumber = hasAnyCompletedScores ? this.selectedWeek.weekNumber : this.selectedWeek.weekNumber - 1;
        
        // Use bulk API to get all handicaps at once
        const handicapsDict = await firstValueFrom(this.handicapService.getAllPlayerUptoWeekHandicaps(this.selectedSeasonId, weekNumber));
        
        // Update only the requested players in the cache
        playerIds.forEach(playerId => {
          if (handicapsDict[playerId] !== undefined) {
            this.playerHandicapsCache.set(playerId, handicapsDict[playerId]);
          }
        });
      }

      console.log(`Refreshed handicaps for ${playerIds.length} players`);
    } catch (error) {
      console.error('Error refreshing player handicaps:', error);
    }
  }
}
