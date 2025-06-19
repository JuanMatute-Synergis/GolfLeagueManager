import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ScoringService } from '../../services/scoring.service';
import { ScorecardService } from '../../services/scorecard.service';
import { MatchupService } from '../../../settings/services/matchup.service';
import { Season, Week, ScoreEntry, Player, PlayerWithFlight } from '../../models/week.model';
import { Matchup } from '../../../settings/services/matchup.service';
import { ScorecardModalComponent } from '../scorecard-modal/scorecard-modal.component';
import { ScorecardData } from '../../models/scorecard.model';
import { HoleScoreBackend } from '../../services/scorecard.service';
import { DateUtilService } from '../../../../core/services/date-util.service';

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
  styleUrls: ['./score-entry.component.css']
})
export class ScoreEntryComponent implements OnInit {
  seasons: Season[] = [];
  weeks: Week[] = [];
  matchups: MatchupWithDetails[] = [];
  players: PlayerWithFlight[] = [];
  selectedSeasonId: string = '';
  selectedWeekId: string = '';
  selectedWeek: Week | null = null;
  isLoading: boolean = false;

  // Scorecard modal properties
  showScorecardModal: boolean = false;
  currentScorecardData: ScorecardData | null = null;
  @ViewChild(ScorecardModalComponent) scorecardModal!: ScorecardModalComponent;

  constructor(
    private scoringService: ScoringService,
    private scorecardService: ScorecardService,
    private matchupService: MatchupService,
    private route: ActivatedRoute,
    private router: Router,
    private dateUtil: DateUtilService
  ) {}

  ngOnInit() {
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
      // Use the robust loading method to avoid timing issues
      this.loadPlayersAndMatchups();
    } else {
      this.selectedWeek = null;
      this.matchups = [];
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
  loadPlayersAndMatchups() {
    if (!this.selectedSeasonId || !this.selectedWeekId) return;
    
    this.isLoading = true;
    console.log('Loading players and matchups simultaneously');
    
    forkJoin({
      players: this.scoringService.getPlayersInFlights(this.selectedSeasonId),
      matchups: this.matchupService.getMatchupsByWeek(this.selectedWeekId)
    }).subscribe({
      next: ({ players, matchups }) => {
        console.log('Loaded:', players.length, 'players and', matchups.length, 'matchups');
        
        // Set players first
        this.players = players;
        
        // Then enrich matchups with player details
        this.matchups = matchups.map(matchup => this.enrichMatchupWithDetails(matchup));
        
        // Load hole scores
        this.loadHoleScoresForMatchups();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading players and matchups:', error);
        this.isLoading = false;
      }
    });
  }

  loadMatchupsForWeek() {
    if (!this.selectedWeekId) return;

    this.isLoading = true;
    console.log('Loading matchups for week:', this.selectedWeekId);
    
    this.matchupService.getMatchupsByWeek(this.selectedWeekId).subscribe({
      next: (matchups) => {
        console.log('Matchups loaded:', matchups.length, 'matchups');
        console.log('Players available:', this.players.length, 'players');
        
        // Only enrich if we have players, otherwise store raw matchups
        if (this.players.length > 0) {
          this.matchups = matchups.map(matchup => this.enrichMatchupWithDetails(matchup));
        } else {
          // Store raw matchups and enrich later when players are loaded
          this.matchups = matchups.map(matchup => ({
            ...matchup,
            playerAName: `Loading... (${matchup.playerAId})`,
            playerBName: `Loading... (${matchup.playerBId})`,
            flightName: 'Loading...',
            hasChanged: false,
            originalPlayerAScore: matchup.playerAScore,
            originalPlayerBScore: matchup.playerBScore
          }));
          // Load players if not already loaded
          this.loadPlayers();
        }
        
        this.loadHoleScoresForMatchups();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading matchups:', error);
        this.isLoading = false;
      }
    });
  }

  loadHoleScoresForMatchups() {
    // Load hole scores for all matchups
    this.matchups.forEach(matchup => {
      if (matchup.id) {
        this.scorecardService.getScorecard(matchup.id).subscribe({
          next: (holeScores) => {
            matchup.holeScores = holeScores;
          },
          error: (error) => {
            // Silently handle errors - matchup may not have hole scores yet
            matchup.holeScores = [];
          }
        });
      }
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

  onScoreChange(matchup: MatchupWithDetails) {
    matchup.hasChanged = (
      matchup.playerAScore !== matchup.originalPlayerAScore ||
      matchup.playerBScore !== matchup.originalPlayerBScore
    );
  }

  getMatchupWinner(matchup: MatchupWithDetails): string {
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
      case 'Pending': return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
      default: return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400';
    }
  }

  getCompletedMatchupsCount(): number {
    return this.matchups.filter(m => m.playerAScore && m.playerBScore).length;
  }

  hasUnsavedChanges(): boolean {
    return this.matchups.some(m => m.hasChanged);
  }

  saveMatchupScore(matchup: MatchupWithDetails) {
    if (!matchup.id) return;

    this.isLoading = true;
    this.matchupService.updateMatchup(matchup).subscribe({
      next: () => {
        matchup.originalPlayerAScore = matchup.playerAScore;
        matchup.originalPlayerBScore = matchup.playerBScore;
        matchup.hasChanged = false;
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

    matchupsToSave.forEach(matchup => {
      if (matchup.id) {
        this.matchupService.updateMatchup(matchup).subscribe({
          next: () => {
            matchup.originalPlayerAScore = matchup.playerAScore;
            matchup.originalPlayerBScore = matchup.playerBScore;
            matchup.hasChanged = false;
            savedCount++;
            
            if (savedCount === matchupsToSave.length) {
              this.isLoading = false;
            }
          },
          error: (error) => {
            console.error('Error saving matchup score:', error);
            savedCount++;
            
            if (savedCount === matchupsToSave.length) {
              this.isLoading = false;
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
      next: () => {
        // Clear cached hole scores
        matchup.holeScores = [];
        // Clear total scores
        matchup.playerAScore = undefined;
        matchup.playerBScore = undefined;
        this.onScoreChange(matchup);
        this.isLoading = false;
        console.log('Scorecard cleared successfully');
      },
      error: (error) => {
        // Even if backend delete fails, clear the frontend data
        console.warn('Failed to clear scorecard from backend, clearing frontend data only:', error);
        matchup.holeScores = [];
        matchup.playerAScore = undefined;
        matchup.playerBScore = undefined;
        this.onScoreChange(matchup);
        this.isLoading = false;
      }
    });
  }

  clearAllScores() {
    if (this.matchups.length === 0) return;

    this.isLoading = true;
    let clearedCount = 0;
    const totalMatchups = this.matchups.length;

    this.matchups.forEach(matchup => {
      if (matchup.id) {
        // Clear scorecard data from backend
        this.scorecardService.deleteScorecard(matchup.id).subscribe({
          next: () => {
            // Clear cached hole scores and total scores
            matchup.holeScores = [];
            matchup.playerAScore = undefined;
            matchup.playerBScore = undefined;
            this.onScoreChange(matchup);
            
            clearedCount++;
            if (clearedCount === totalMatchups) {
              this.isLoading = false;
              console.log('All scorecards cleared successfully');
            }
          },
          error: (error) => {
            // Even if backend delete fails, clear the frontend data
            console.warn(`Failed to clear scorecard for matchup ${matchup.id}, clearing frontend data only:`, error);
            matchup.holeScores = [];
            matchup.playerAScore = undefined;
            matchup.playerBScore = undefined;
            this.onScoreChange(matchup);
            
            clearedCount++;
            if (clearedCount === totalMatchups) {
              this.isLoading = false;
            }
          }
        });
      } else {
        // If no matchup ID, just clear frontend data
        matchup.holeScores = [];
        matchup.playerAScore = undefined;
        matchup.playerBScore = undefined;
        this.onScoreChange(matchup);
        
        clearedCount++;
        if (clearedCount === totalMatchups) {
          this.isLoading = false;
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
          // Include player handicaps
          playerAHandicap: playerA?.currentHandicap || 0,
          playerBHandicap: playerB?.currentHandicap || 0
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
      
      // Reload hole scores for this matchup to update status
      this.scorecardService.getScorecard(scorecardData.matchupId).subscribe({
        next: (holeScores) => {
          matchup.holeScores = holeScores;
        },
        error: (error) => console.error('Error reloading hole scores:', error)
      });
    }

    // Show success message - the scorecard has been saved to the backend
    console.log('Scorecard saved successfully:', scorecardData);
    
    this.closeScorecardModal();
  }

  closeScorecardModal() {
    this.showScorecardModal = false;
    this.currentScorecardData = null;
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

    // Calculate net score using handicap difference logic (like in scorecard component)
    const handicapDifference = Math.abs(playerAHandicap - playerBHandicap);
    
    if (handicapDifference === 0) {
      return grossScore.toString(); // No handicap adjustment
    }

    // Determine who gets strokes (higher handicap player)
    const playerHandicap = player === 'A' ? playerAHandicap : playerBHandicap;
    const playerAReceivesStrokes = playerAHandicap > playerBHandicap;
    const playerBReceivesStrokes = playerBHandicap > playerAHandicap;
    
    const playerReceivesStrokes = (player === 'A' && playerAReceivesStrokes) || 
                                 (player === 'B' && playerBReceivesStrokes);

    if (!playerReceivesStrokes) {
      return grossScore.toString(); // No strokes for this player
    }

    // For simplicity in the list view, approximate strokes as handicap difference
    // (The exact hole-by-hole calculation is done in the scorecard component)
    const approximateStrokes = Math.round(handicapDifference);
    const netScore = grossScore - approximateStrokes;
    
    return netScore.toString();
  }

  getPlayerAverageScore(playerId: string | undefined): string {
    if (!playerId) return '-';
    
    const player = this.players.find(p => p.id === playerId);
    return player?.currentAverageScore?.toString() || '-';
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
    
    const player = this.players.find(p => p.id === playerId);
    return player?.currentHandicap || null;
  }

  getPlayerFirstName(fullName: string | undefined): string {
    if (!fullName) return '';
    return fullName.split(' ')[0] || '';
  }
}