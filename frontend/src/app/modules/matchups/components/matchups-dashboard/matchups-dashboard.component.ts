import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  MatchupsService,
  Matchup,
  MatchupWithFlightInfo,
  Season,
  Week,
  Flight,
  PlayerFlightAssignment,
  Player
} from '../../services/matchups.service';
import { ScoreCalculationService } from '../../../scoring/services/score-calculation.service';
import { ScorecardModalComponent } from '../../../scoring/components/scorecard-modal/scorecard-modal.component';
import { DateUtilService } from '../../../../core/services/date-util.service';
import { HandicapService } from '../../../../core/services/handicap.service';
import { NineHoles } from '../../../scoring/models/week.model';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-matchups-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, ScorecardModalComponent],
  templateUrl: './matchups-dashboard.component.html',
  styleUrls: ['./matchups-dashboard.component.css']
})
export class MatchupsDashboardComponent implements OnInit {
  // Data
  seasons: Season[] = [];
  weeks: Week[] = [];
  flights: Flight[] = [];
  matchups: Matchup[] = [];
  playerFlightAssignments: PlayerFlightAssignment[] = [];
  players: Player[] = [];

  // Processed data
  matchupsByFlight: { [flightId: string]: MatchupWithFlightInfo[] } = {};
  flightOrder: Flight[] = [];

  // Selected values
  selectedSeasonId: string = '';
  selectedWeekId: string = '';
  selectedFlightId: string = '';

  // UI state
  isLoading = false;
  error: string | null = null;

  // Scorecard viewer state
  showScorecardViewer = false;
  selectedMatchupForScorecard: MatchupWithFlightInfo | null = null;

  // Session handicaps cache
  sessionHandicaps: { [playerId: string]: number } = {};

  constructor(
    private matchupsService: MatchupsService,
    private scoreCalculationService: ScoreCalculationService,
    private dateUtil: DateUtilService,
    private handicapService: HandicapService,
  ) {}

  ngOnInit(): void {
    this.loadSeasons();
  }

  loadSeasons(): void {
    this.isLoading = true;
    this.error = null;

    this.matchupsService.getSeasons().subscribe({
      next: (seasons) => {
        this.seasons = seasons.sort((a, b) => b.year - a.year || b.seasonNumber - a.seasonNumber);
        this.isLoading = false;

        // Auto-select the most recent season
        if (this.seasons.length > 0) {
          this.selectedSeasonId = this.seasons[0].id;
          this.onSeasonChange();
        }
      },
      error: (error) => {
        console.error('Error loading seasons:', error);
        this.error = 'Failed to load seasons. Please try again.';
        this.isLoading = false;
      }
    });
  }

  onSeasonChange(): void {
    if (!this.selectedSeasonId) {
      this.weeks = [];
      this.flights = [];
      this.matchups = [];
      this.matchupsByFlight = {};
      this.flightOrder = [];
      return;
    }

    this.isLoading = true;
    this.error = null;
    this.selectedWeekId = '';

    // Load weeks, flights, and players for the selected season
    forkJoin({
      weeks: this.matchupsService.getWeeksBySeason(this.selectedSeasonId),
      flights: this.matchupsService.getFlightsBySeason(this.selectedSeasonId),
      playerAssignments: this.matchupsService.getPlayerFlightAssignments(),
      players: this.matchupsService.getPlayersInFlights(this.selectedSeasonId)
    }).subscribe({
      next: (result) => {
        this.weeks = result.weeks.sort((a, b) => a.weekNumber - b.weekNumber);
        this.flights = result.flights.sort((a, b) => a.name.localeCompare(b.name));
        this.playerFlightAssignments = result.playerAssignments;
        this.players = result.players;

        // Filter assignments for current season flights
        const seasonFlightIds = this.flights.map(f => f.id);
        this.playerFlightAssignments = this.playerFlightAssignments.filter(
          assignment => seasonFlightIds.includes(assignment.flightId)
        );

        this.isLoading = false;

        // Auto-select current week or first week
        if (this.weeks.length > 0) {
          // Try to find current week based on date
          const today = new Date();
          const todayString = today.getFullYear() + '-' +
            String(today.getMonth() + 1).padStart(2, '0') + '-' +
            String(today.getDate()).padStart(2, '0');

          const currentWeek = this.weeks.find(week => {
            // Compare date strings directly to avoid timezone issues
            const weekDateString = week.date.split('T')[0]; // Get YYYY-MM-DD part
            const weekDate = new Date(weekDateString + 'T12:00:00'); // Add noon time to avoid timezone issues
            const todayDate = new Date(todayString + 'T12:00:00');
            const daysDiff = Math.abs(todayDate.getTime() - weekDate.getTime()) / (1000 * 3600 * 24);
            return daysDiff <= 3; // Within 3 days of the week
          });

          this.selectedWeekId = currentWeek ? currentWeek.id : this.weeks[0].id;
          this.onWeekChange();
        }
      },
      error: (error) => {
        console.error('Error loading season data:', error);
        this.error = 'Failed to load season data. Please try again.';
        this.isLoading = false;
      }
    });
  }

  onWeekChange(): void {
    if (!this.selectedWeekId) {
      this.matchups = [];
      this.matchupsByFlight = {};
      this.flightOrder = [];
      this.sessionHandicaps = {};
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.matchupsService.getMatchupsByWeek(this.selectedWeekId).subscribe({
      next: (matchups) => {
        this.matchups = matchups;
        this.processMatchupsByFlight();
        this.loadSessionHandicaps();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading matchups:', error);
        this.error = 'Failed to load matchups. Please try again.';
        this.isLoading = false;
      }
    });
  }

  private processMatchupsByFlight(): void {
    this.matchupsByFlight = {};
    this.flightOrder = [];

    // Group matchups by flight
    const flightGroups: { [flightId: string]: MatchupWithFlightInfo[] } = {};

    this.matchups.forEach(matchup => {
      const playerAFlightAssignment = this.playerFlightAssignments.find(
        assignment => assignment.playerId === matchup.playerAId
      );
      const playerBFlightAssignment = this.playerFlightAssignments.find(
        assignment => assignment.playerId === matchup.playerBId
      );

      // Both players should be in the same flight
      const flightId = playerAFlightAssignment?.flightId || playerBFlightAssignment?.flightId;

      if (flightId) {
        const flight = this.flights.find(f => f.id === flightId);
        const playerA = this.players.find(p => p.id === matchup.playerAId);
        const playerB = this.players.find(p => p.id === matchup.playerBId);

        if (flight && playerA && playerB) {
          const matchupWithFlightInfo: MatchupWithFlightInfo = {
            ...matchup,
            playerAName: `${playerA.firstName} ${playerA.lastName}`,
            playerBName: `${playerB.firstName} ${playerB.lastName}`,
            flightName: flight.name,
            flightId: flight.id
          };

          if (!flightGroups[flightId]) {
            flightGroups[flightId] = [];
          }
          flightGroups[flightId].push(matchupWithFlightInfo);
        }
      }
    });

    // Sort matchups within each flight by player names
    Object.keys(flightGroups).forEach(flightId => {
      flightGroups[flightId].sort((a, b) => a.playerAName.localeCompare(b.playerAName));
    });

    this.matchupsByFlight = flightGroups;

    // Create ordered list of flights that have matchups
    this.flightOrder = this.flights.filter(flight =>
      this.matchupsByFlight[flight.id] && this.matchupsByFlight[flight.id].length > 0
    );

    // Auto-select first flight if none selected
    if (this.flightOrder.length > 0 && !this.selectedFlightId) {
      this.selectedFlightId = this.flightOrder[0].id;
    }
  }

  downloadWeekScorecardPdf(): void {
    if (!this.selectedWeekId) return;
    this.isLoading = true;
    this.matchupsService.downloadWeekScorecardPdf(this.selectedWeekId).subscribe({
      next: (blob) => {
        const fileName = `Scorecard_Week_${this.selectedWeekId}.pdf`;
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

  getSelectedSeasonName(): string {
    const season = this.seasons.find(s => s.id === this.selectedSeasonId);
    return season ? season.name : '';
  }

  getSelectedWeekName(): string {
    const week = this.weeks.find(w => w.id === this.selectedWeekId);
    return week ? week.name : '';
  }

  getSelectedWeek(): Week | undefined {
    return this.weeks.find(w => w.id === this.selectedWeekId);
  }

  getSelectedWeekForScorecard(): import('../../../scoring/models/week.model').Week | undefined {
    const week = this.getSelectedWeek();
    if (!week) return undefined;
    
    // Convert matchups service Week to scoring module Week
    return {
      ...week,
      nineHoles: week.nineHoles || NineHoles.Front, // Default to Front if not set
      matchups: week.matchups?.map(m => ({
        ...m,
        id: m.id || '' // Ensure id is string, not string | undefined
      })) || []
    } as import('../../../scoring/models/week.model').Week;
  }

  getWeekDateRange(week: Week): string {
    return this.dateUtil.formatDateShort(week.date);
  }

  hasScores(matchup: MatchupWithFlightInfo): boolean {
    // Consider matchup complete if either player has scored OR if any player is marked absent
    return (matchup.playerAScore !== null && matchup.playerAScore !== undefined) ||
           (matchup.playerBScore !== null && matchup.playerBScore !== undefined) ||
           (matchup.playerAAbsent === true) || (matchup.playerBAbsent === true);
  }
  getMatchupStatus(matchup: MatchupWithFlightInfo): string {
    // Handle absence scenarios first
    if (matchup.playerAAbsent === true && matchup.playerBAbsent === true) {
      return 'Both Absent';
    }
    if (matchup.playerAAbsent === true) {
      return `${matchup.playerBName} Wins (A Absent)`;
    }
    if (matchup.playerBAbsent === true) {
      return `${matchup.playerAName} Wins (B Absent)`;
    }

    // Handle normal scoring scenarios
    if (!this.hasScores(matchup)) {
      return 'Pending';
    }

    // Use match play points if available (this is the correct way for match play scoring)
    if (matchup.playerAPoints !== null && matchup.playerAPoints !== undefined &&
        matchup.playerBPoints !== null && matchup.playerBPoints !== undefined) {

      if (matchup.playerAPoints > matchup.playerBPoints) {
        return `${matchup.playerAName} Wins`;
      } else if (matchup.playerBPoints > matchup.playerAPoints) {
        return `${matchup.playerBName} Wins`;
      } else {
        return 'Tied';
      }
    }

    // Fall back to gross scores if match play points aren't available
    const scoreA = matchup.playerAScore || 0;
    const scoreB = matchup.playerBScore || 0;

    if (scoreA === scoreB) {
      return 'Tied';
    }

    return scoreA < scoreB ? `${matchup.playerAName} Wins` : `${matchup.playerBName} Wins`;
  }

  getScoreDisplay(score: number | null | undefined): string {
    return score !== null && score !== undefined ? score.toString() : '--';
  }

  // Generate avatar initials from full name
  getPlayerInitials(playerName: string): string {
    return playerName
      .split(' ')
      .map(name => name.charAt(0))
      .join('')
      .toUpperCase();
  }

  // Get first name from full name
  getFirstName(fullName: string): string {
    return fullName.split(' ')[0];
  }

  // Flight tab selection
  selectFlight(flightId: string): void {
    this.selectedFlightId = flightId;
  }

  getSelectedFlightMatchups(): MatchupWithFlightInfo[] {
    return this.selectedFlightId ? (this.matchupsByFlight[this.selectedFlightId] || []) : [];
  }

  getSelectedFlightName(): string {
    const flight = this.flightOrder.find(f => f.id === this.selectedFlightId);
    return flight ? flight.name : '';
  }

  // Open scorecard viewer
  openScorecardViewer(matchup: MatchupWithFlightInfo): void {
    this.selectedMatchupForScorecard = matchup;
    this.showScorecardViewer = true;
  }

  // Close scorecard viewer
  closeScorecardViewer(): void {
    this.showScorecardViewer = false;
    this.selectedMatchupForScorecard = null;

    // Refresh matchup data to get updated points after scorecard save
    if (this.selectedWeekId) {
      this.onWeekChange();
    }
  }

  // TrackBy functions for performance
  trackByFlightId(index: number, flight: Flight): string {
    return flight.id;
  }

  trackByMatchupId(index: number, matchup: MatchupWithFlightInfo): string {
    return matchup.id || index.toString();
  }

  getPlayerMatchPoints(matchup: MatchupWithFlightInfo, player: 'A' | 'B'): string {
    // Handle absence scenarios - show points if they're calculated
    if (matchup.playerAAbsent === true || matchup.playerBAbsent === true) {
      const points = player === 'A' ? matchup.playerAPoints : matchup.playerBPoints;
      return points !== null && points !== undefined ? points.toString() : '0';
    }

    // Return match play points if available
    const points = player === 'A' ? matchup.playerAPoints : matchup.playerBPoints;
    return points !== null && points !== undefined ? points.toString() : '-';
  }

  getPlayerNetScore(matchup: MatchupWithFlightInfo, player: 'A' | 'B'): string {
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

    // Fallback synchronous calculation to maintain UI responsiveness
    // This should match the backend logic for overall net scores
    if (playerHandicap <= opponentHandicap) {
      return grossScore.toString(); // No strokes for equal or lower handicap
    }

    const handicapDifference = playerHandicap - opponentHandicap;
    const netScore = grossScore - handicapDifference;

    return netScore.toString();
  }

  private getPlayerHandicap(playerId: string | undefined): number | null {
    if (!playerId) return null;

    // First try to get session handicap
    if (this.sessionHandicaps[playerId] !== undefined) {
      return this.sessionHandicaps[playerId];
    }

    // Fallback to current handicap
    const player = this.players.find(p => p.id === playerId);
    return player?.handicap || null;
  }

  getPlayerHandicapForDisplay(playerId: string | undefined): string {
    const handicap = this.getPlayerHandicap(playerId);
    return handicap !== null ? handicap.toString() : '-';
  }

  getPlayerAverageScore(playerId: string | undefined): string {
    if (!playerId) return '-';

    const player = this.players.find(p => p.id === playerId);
    return player?.currentAverageScore?.toString() || '-';
  }

  getPlayerFirstName(fullName: string | undefined): string {
    if (!fullName) return '';
    return fullName.split(' ')[0] || '';
  }

  getPlayerScoreDisplay(matchup: MatchupWithFlightInfo, player: 'A' | 'B'): string {
    const isAbsent = player === 'A' ? (matchup.playerAAbsent === true) : (matchup.playerBAbsent === true);
    if (isAbsent) {
      return 'ABSENT';
    }

    const score = player === 'A' ? matchup.playerAScore : matchup.playerBScore;
    return score !== null && score !== undefined ? score.toString() : '--';
  }

  private loadSessionHandicaps(): void {
    if (!this.selectedSeasonId || !this.selectedWeekId) {
      return;
    }

    // Clear existing session handicaps
    this.sessionHandicaps = {};

    // Get the selected week to find the week number
    const selectedWeek = this.weeks.find(w => w.id === this.selectedWeekId);
    if (!selectedWeek) {
      return;
    }

    // Collect all unique player IDs from matchups
    const playerIds = new Set<string>();
    this.matchups.forEach(matchup => {
      if (matchup.playerAId) playerIds.add(matchup.playerAId);
      if (matchup.playerBId) playerIds.add(matchup.playerBId);
    });

    // Load session handicaps for all players
    const handicapRequests = Array.from(playerIds).map(playerId =>
      this.handicapService.getPlayerSessionHandicap(playerId, this.selectedSeasonId, selectedWeek.weekNumber)
    );

    if (handicapRequests.length > 0) {
      forkJoin(handicapRequests).subscribe({
        next: (handicaps) => {
          Array.from(playerIds).forEach((playerId, index) => {
            this.sessionHandicaps[playerId] = handicaps[index];
          });
        },
        error: (error) => {
          console.error('Error loading session handicaps:', error);
          // Fallback to current handicaps if session handicaps fail
          Array.from(playerIds).forEach(playerId => {
            const player = this.players.find(p => p.id === playerId);
            this.sessionHandicaps[playerId] = player?.handicap || 0;
          });
        }
      });
    }
  }
}
