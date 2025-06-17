import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ScoringService } from '../../services/scoring.service';
import { ScorecardService } from '../../services/scorecard.service';
import { MatchupService } from '../../../settings/services/matchup.service';
import { Season, Week, ScoreEntry, Player, PlayerWithFlight } from '../../models/week.model';
import { Matchup } from '../../../settings/services/matchup.service';
import { ScorecardModalComponent } from '../scorecard-modal/scorecard-modal.component';
import { ScorecardData } from '../../models/scorecard.model';
import { HoleScoreBackend } from '../../services/scorecard.service';

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

  constructor(
    private scoringService: ScoringService,
    private scorecardService: ScorecardService,
    private matchupService: MatchupService,
    private route: ActivatedRoute,
    private router: Router
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
      // Load both weeks and players, but load players first
      this.loadPlayers();
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
        this.loadPlayers();
        this.onWeekChange();
      },
      error: (error) => console.error('Error loading week:', error)
    });
  }

  onWeekChange() {
    if (this.selectedWeekId) {
      this.selectedWeek = this.weeks.find(w => w.id === this.selectedWeekId) || null;
      this.loadMatchupsForWeek();
    } else {
      this.selectedWeek = null;
      this.matchups = [];
    }
  }

  loadPlayers() {
    if (!this.selectedSeasonId) return;
    
    this.scoringService.getPlayersInFlights(this.selectedSeasonId).subscribe({
      next: (players: PlayerWithFlight[]) => {
        this.players = players;
        // Re-enrich matchups with player details after players are loaded
        if (this.matchups.length > 0) {
          this.matchups = this.matchups.map(matchup => this.enrichMatchupWithDetails(matchup));
        }
      },
      error: (error) => console.error('Error loading players:', error)
    });
  }

  loadMatchupsForWeek() {
    if (!this.selectedWeekId) return;

    this.isLoading = true;
    this.matchupService.getMatchupsByWeek(this.selectedWeekId).subscribe({
      next: (matchups) => {
        this.matchups = matchups.map(matchup => this.enrichMatchupWithDetails(matchup));
        this.loadHoleScoresForMatchups();
        this.isLoading = false;
        // If players aren't loaded yet, re-enrich after players are loaded
        if (this.players.length === 0) {
          this.loadPlayers();
        }
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
    
    // Debug logging to help identify the issue
    if (!playerA || !playerB) {
      console.log('Player not found for matchup:', {
        matchupId: matchup.id,
        playerAId: matchup.playerAId,
        playerBId: matchup.playerBId,
        playerAFound: !!playerA,
        playerBFound: !!playerB,
        totalPlayers: this.players.length,
        playerIds: this.players.map(p => p.id)
      });
    }
    
    return {
      ...matchup,
      playerAName: playerA ? `${playerA.firstName} ${playerA.lastName}` : `Unknown Player (${matchup.playerAId})`,
      playerBName: playerB ? `${playerB.firstName} ${playerB.lastName}` : `Unknown Player (${matchup.playerBId})`,
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
    const weekDate = new Date(week.date).toLocaleDateString('en-US', { 
      weekday: 'long',
      month: 'short', 
      day: 'numeric' 
    });
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

    this.currentScorecardData = {
      matchupId: matchup.id || '',
      playerAId: matchup.playerAId || '',
      playerBId: matchup.playerBId || '',
      playerAName: matchup.playerAName,
      playerBName: matchup.playerBName,
      flightName: matchup.flightName || 'Unknown Flight',
      holes: [], // Will be initialized by the modal
      playerATotalScore: matchup.playerAScore || 0,
      playerBTotalScore: matchup.playerBScore || 0,
      playerAHolesWon: 0,
      playerBHolesWon: 0,
      holesHalved: 0
    };

    this.showScorecardModal = true;
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
}