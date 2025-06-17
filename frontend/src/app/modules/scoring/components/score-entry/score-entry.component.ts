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

interface MatchupWithDetails extends Matchup {
  playerAName?: string;
  playerBName?: string;
  flightName?: string;
  hasChanged?: boolean;
  originalPlayerAScore?: number;
  originalPlayerBScore?: number;
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
    if (matchup.playerAScore && matchup.playerBScore) {
      return 'Completed';
    } else if (matchup.playerAScore || matchup.playerBScore) {
      return 'Partial';
    } else {
      return 'Pending';
    }
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
    matchup.playerAScore = undefined;
    matchup.playerBScore = undefined;
    this.onScoreChange(matchup);
  }

  clearAllScores() {
    this.matchups.forEach(matchup => {
      matchup.playerAScore = undefined;
      matchup.playerBScore = undefined;
      this.onScoreChange(matchup);
    });
  }

  getWeekDisplayName(week: Week): string {
    const startDate = new Date(week.startDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    const endDate = new Date(week.endDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    return `${week.name} (${startDate} - ${endDate})`;
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