import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ScoringService } from '../../services/scoring.service';
import { Season, Week, ScoreEntry, Player, PlayerWithFlight } from '../../models/week.model';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './leaderboard.component.html',
  styles: [`
    /* Additional custom styles if needed */
  `]
})
export class LeaderboardComponent implements OnInit {
  seasons: Season[] = [];
  weeks: Week[] = [];
  selectedSeasonId: string = '';
  selectedWeekId: string = '';
  selectedWeek: Week | null = null;
  leaderboard: ScoreEntry[] = [];

  // Flight filtering additions
  flights: { id: string, name: string }[] = [];
  selectedFlight: string = '';
  playerFlights: PlayerWithFlight[] = [];
  filteredLeaderboard: ScoreEntry[] = [];

  // Course information - could be made dynamic in the future
  private readonly coursePar = 36; // 9-hole course: 4+3+4+5+4+3+4+4+5 = 36

  // Sorting additions
  selectedSort: string = 'rank';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private scoringService: ScoringService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadSeasons();
  }

  loadSeasons() {
    this.scoringService.getSeasons().subscribe({
      next: (seasons) => {
        this.seasons = seasons;
        if (seasons.length > 0) {
          this.selectedSeasonId = seasons[0].id;
          this.onSeasonChange();
        }
      },
      error: (error: any) => console.error('Error loading seasons:', error)
    });
  }

  onSeasonChange() {
    if (this.selectedSeasonId) {
      this.loadWeeks();
      this.selectedWeekId = '';
      this.leaderboard = [];
      this.loadFlightsAndAssignments(); // Load flights for the selected season
    }
  }

  loadWeeks() {
    this.scoringService.getWeeksBySeason(this.selectedSeasonId).subscribe({
      next: (weeks) => {
        // Sort by weekNumber ascending
        this.weeks = weeks.sort((a, b) => a.weekNumber - b.weekNumber);
        // Auto-select the latest week (<= today)
        const today = new Date();
        const pastOrCurrentWeeks = this.weeks.filter(w => new Date(w.date) <= today);
        let currentWeek: Week | undefined;
        if (pastOrCurrentWeeks.length > 0) {
          // Pick the one with the highest weekNumber
          currentWeek = pastOrCurrentWeeks.reduce((latest, w) => w.weekNumber > latest.weekNumber ? w : latest, pastOrCurrentWeeks[0]);
        } else if (this.weeks.length > 0) {
          // If all weeks are in the future, select the first
          currentWeek = this.weeks[0];
        }
        this.selectedWeekId = currentWeek ? currentWeek.id : '';
        this.onWeekChange();
      },
      error: (error: any) => console.error('Error loading weeks:', error)
    });
  }

  onWeekChange() {
    if (this.selectedWeekId) {
      this.selectedWeek = this.weeks.find(w => w.id === this.selectedWeekId) || null;
      this.loadLeaderboard();
    } else {
      this.leaderboard = [];
    }
  }

  loadFlightsAndAssignments() {
    if (!this.selectedSeasonId) return;
    this.scoringService.getPlayersInFlights(this.selectedSeasonId).subscribe({
      next: (playerFlights) => {
        this.playerFlights = playerFlights;
        // Extract unique flights
        const uniqueFlights: { [id: string]: string } = {};
        playerFlights.forEach((pf) => {
          if (pf.flightId && pf.flightName) {
            uniqueFlights[pf.flightId] = pf.flightName;
          }
        });
        this.flights = Object.entries(uniqueFlights).map(([id, name]) => ({ id, name }));
        this.selectedFlight = '';
        this.applyFlightFilter();
      },
      error: (error: any) => console.error('Error loading player flights:', error)
    });
  }

  onFlightChange() {
    this.applyFlightFilter();
  }

  loadLeaderboard() {
    if (!this.selectedWeekId) return;

    this.scoringService.getWeeklyLeaderboard(this.selectedWeekId).subscribe({
      next: (scores) => {
        console.log('Leaderboard data:', scores); // Debug log
        console.log('Sample score entry:', scores[0]); // Debug first entry
        this.leaderboard = scores;
        this.applyFlightFilter();
      },
      error: (error: any) => console.error('Error loading leaderboard:', error)
    });
  }

  applyFlightFilter() {
    if (!this.selectedFlight) {
      this.filteredLeaderboard = [...this.leaderboard];
    } else {
      // Build a set of playerIds in the selected flight
      const playerIdsInFlight = new Set(
        this.playerFlights.filter(pf => pf.flightId === this.selectedFlight).map(pf => pf.id)
      );
      this.filteredLeaderboard = this.leaderboard.filter(score => {
        const pid = score.playerId || (score.player && score.player.id);
        return pid && playerIdsInFlight.has(pid);
      });
    }
    this.sortLeaderboard();
  }

  sortLeaderboard(sortBy?: string) {
    if (sortBy) {
      if (this.selectedSort === sortBy) {
        // Toggle direction
        this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
      } else {
        this.selectedSort = sortBy;
        this.sortDirection = 'asc';
      }
    }
    const dir = this.sortDirection === 'asc' ? 1 : -1;
    this.filteredLeaderboard.sort((a, b) => {
      switch (this.selectedSort) {
        case 'player': {
          const nameA = this.getPlayerDisplayName(a).toLowerCase();
          const nameB = this.getPlayerDisplayName(b).toLowerCase();
          return nameA.localeCompare(nameB) * dir;
        }
        case 'handicap': {
          const hA = typeof this.getHandicap(a) === 'number' ? (this.getHandicap(a) as number) : 999;
          const hB = typeof this.getHandicap(b) === 'number' ? (this.getHandicap(b) as number) : 999;
          return (hA - hB) * dir;
        }
        case 'score':
          return ((a.score ?? 999) - (b.score ?? 999)) * dir;
        case 'points':
          return ((a.pointsEarned ?? 0) - (b.pointsEarned ?? 0)) * dir;
        case 'performance': {
          // Use points then score as tiebreaker
          const perfA = (a.pointsEarned ?? 0) * 1000 - (a.score ?? 999);
          const perfB = (b.pointsEarned ?? 0) * 1000 - (b.score ?? 999);
          return (perfA - perfB) * dir;
        }
        case 'rank':
        default:
          // Default: sort by points desc, then score asc
          return (b.pointsEarned - a.pointsEarned) * dir || (a.score - b.score) * dir;
      }
    });
  }

  getRankColor(rank: number): string {
    switch (rank) {
      case 1: return '#FFD700'; // Gold
      case 2: return '#C0C0C0'; // Silver
      case 3: return '#CD7F32'; // Bronze
      case 4: case 5: return '#4F46E5'; // Indigo
      default: return '#6B7280'; // Gray
    }
  }

  getRankEmoji(rank: number): string {
    switch (rank) {
      case 1: return '🏆';
      case 2: return '🥈';
      case 3: return '🥉';
      default: return '';
    }
  }

  getRankRowClass(rank: number): string {
    switch (rank) {
      case 1: return 'bg-yellow-50 border-l-4 border-yellow-400';
      case 2: return 'bg-gray-50 border-l-4 border-gray-400';
      case 3: return 'bg-orange-50 border-l-4 border-orange-400';
      default: return '';
    }
  }

  getPerformanceEmoji(rank: number, totalPlayers: number): string {
    const percentage = rank / totalPlayers;
    if (percentage <= 0.1) return '🔥'; // Top 10%
    if (percentage <= 0.25) return '⭐'; // Top 25%
    if (percentage <= 0.5) return '👍'; // Top 50%
    if (percentage <= 0.75) return '😊'; // Top 75%
    return '💪'; // Bottom 25%
  }

  getPerformanceText(rank: number, totalPlayers: number): string {
    const percentage = rank / totalPlayers;
    if (percentage <= 0.1) return 'Exceptional';
    if (percentage <= 0.25) return 'Excellent';
    if (percentage <= 0.5) return 'Good';
    if (percentage <= 0.75) return 'Average';
    return 'Keep Going';
  }

  getScoreRelativeToPar(score: number): string {
    const difference = score - this.coursePar;
    if (difference === 0) return 'Even par';
    if (difference > 0) return `+${difference} over par`;
    return `${difference} under par`;
  }

  getLowestScore(entries?: ScoreEntry[]): number {
    const arr = entries ?? this.leaderboard;
    if (arr.length === 0) return 0;
    return Math.min(...arr.map(s => s.score));
  }

  getAverageScore(entries?: ScoreEntry[]): number {
    const arr = entries ?? this.leaderboard;
    if (arr.length === 0) return 0;
    const total = arr.reduce((sum, s) => sum + s.score, 0);
    return total / arr.length;
  }

  getHighestPoints(entries?: ScoreEntry[]): number {
    const arr = entries ?? this.leaderboard;
    if (arr.length === 0) return 0;
    return Math.max(...arr.map(s => s.pointsEarned));
  }

  getWeekDisplayName(week: Week): string {
    // Parse date as local date to avoid timezone shift
    let weekDate: Date;
    if (typeof week.date === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(week.date)) {
      // Parse as local date (YYYY-MM-DD)
      const [year, month, day] = week.date.split('-').map(Number);
      weekDate = new Date(year, month - 1, day);
    } else {
      weekDate = new Date(week.date);
    }
    const weekDay = weekDate.toLocaleDateString('en-US', { weekday: 'long' });
    const monthDay = weekDate.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    return `${week.name} (${weekDay}, ${monthDay})`;
  }

  navigateToScoreEntry() {
    this.router.navigate(['/scoring/score-entry'], {
      queryParams: { weekId: this.selectedWeekId }
    });
  }

  getPlayerDisplayName(score: any): string {
    // Check if we have a player object with firstName and lastName
    if (score.player && score.player.firstName && score.player.lastName) {
      return `${score.player.firstName} ${score.player.lastName}`;
    }

    // Check if firstName/lastName are directly on the score object
    if (score.firstName && score.lastName) {
      return `${score.firstName} ${score.lastName}`;
    }

    // Check if we have a player.name field
    if (score.player && score.player.name) {
      return score.player.name;
    }

    // Check if name is directly on the score object
    if (score.name) {
      return score.name;
    }

    // Check if we have player email
    if (score.player && score.player.email) {
      return score.player.email;
    }

    // Check if email is directly on the score object
    if (score.email) {
      return score.email;
    }

    // Fallback to playerId if player object is missing (backend issue)
    if (score.playerId) {
      const shortId = score.playerId.substring(0, 8);
      return `Player ${shortId}`;
    }

    return 'Unknown Player';
  }

  getHandicap(score: any): number | string {
    if (score.player && typeof score.player.currentHandicap === 'number') {
      return score.player.currentHandicap;
    }
    if (typeof score.currentHandicap === 'number') {
      return score.currentHandicap;
    }
    return '-';
  }
}
