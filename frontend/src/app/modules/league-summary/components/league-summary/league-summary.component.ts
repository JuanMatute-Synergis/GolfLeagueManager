import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { forkJoin, Observable } from 'rxjs';

interface Player {
  id: string;
  firstName: string;
  lastName: string;
  displayName: string;
  handicap: number;
  averageScore: number;
  grossScore: number;
  thisWeekPoints: number;
  sessionTotal: number;
  isAbsent: boolean;
}

interface Flight {
  id: string;
  name: string;
  players: Player[];
}

interface NextWeekMatchup {
  playerAName: string;
  playerBName: string;
  flightName: string;
}

interface Season {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
}

interface Week {
  id: string;
  weekNumber: number;
  name: string;
  date: string;
  seasonId: string;
}

interface Session {
  number: number;
  startWeekNumber: number;
  currentWeekNumber: number;
}

interface LeagueSummaryData {
  week: Week;
  session: Session;
  flights: Flight[];
  nextWeekMatchups: NextWeekMatchup[];
}

@Component({
  selector: 'app-league-summary',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './league-summary.component.html',
  styleUrls: ['./league-summary.component.css']
})
export class LeagueSummaryComponent implements OnInit {
  // Data
  seasons: Season[] = [];
  weeks: Week[] = [];
  summaryData: LeagueSummaryData | null = null;

  // Selected values
  selectedSeasonId: string = '';
  selectedWeekId: string = '';

  // UI state
  isLoading = false;
  error: string | null = null;

  private readonly apiUrl = '/api';

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.loadSeasons();
  }

  loadSeasons(): void {
    this.isLoading = true;
    this.error = null;

    this.http.get<Season[]>(`${this.apiUrl}/seasons`).subscribe({
      next: (seasons) => {
        this.seasons = seasons;
        this.isLoading = false;

        // Auto-select current season - first try by isCurrent flag, then by date
        let currentSeason = seasons.find(s => s.isCurrent);

        if (!currentSeason && seasons.length > 0) {
          // Fallback: find season that contains today's date
          const today = new Date();
          currentSeason = seasons.find(season => {
            const startDate = new Date(season.startDate);
            const endDate = new Date(season.endDate);
            return today >= startDate && today <= endDate;
          });

          // If still no current season, use the most recent one
          if (!currentSeason) {
            currentSeason = seasons.sort((a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime())[0];
          }
        }

        if (currentSeason) {
          this.selectedSeasonId = currentSeason.id;
          this.onSeasonChange();
        }
      },
      error: (error) => {
        console.error('Error loading seasons:', error);
        this.error = 'Failed to load seasons';
        this.isLoading = false;
      }
    });
  }

  onSeasonChange(): void {
    this.weeks = [];
    this.selectedWeekId = '';
    this.summaryData = null;

    if (!this.selectedSeasonId) return;

    this.isLoading = true;
    this.http.get<Week[]>(`${this.apiUrl}/weeks/season/${this.selectedSeasonId}`).subscribe({
      next: (weeks) => {
        this.weeks = weeks.sort((a, b) => b.weekNumber - a.weekNumber); // Most recent first
        this.isLoading = false;

        // Auto-select current week based on today's date
        if (weeks.length > 0) {
          const today = new Date();

          // Find the week that contains today's date or the closest past week
          const currentWeek = weeks
            .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()) // Sort by date ascending
            .find(week => {
              const weekDate = new Date(week.date);
              // Check if today is within 7 days after the week date (assuming each week spans 7 days)
              const weekEndDate = new Date(weekDate.getTime() + (7 * 24 * 60 * 60 * 1000));
              return today >= weekDate && today <= weekEndDate;
            });

          // If no current week found, find the most recent past week
          const selectedWeek = currentWeek || weeks
            .filter(week => new Date(week.date) <= today)
            .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())[0];

          if (selectedWeek) {
            this.selectedWeekId = selectedWeek.id;
            this.onWeekChange();
          }
        }
      },
      error: (error) => {
        console.error('Error loading weeks:', error);
        this.error = 'Failed to load weeks';
        this.isLoading = false;
      }
    });
  }

  onWeekChange(): void {
    this.summaryData = null;
    if (!this.selectedWeekId) return;

    this.loadSummaryData();
  }

  private loadSummaryData(): void {
    this.isLoading = true;
    this.error = null;

    // Use the session-based standings endpoint
    this.http.get<any>(`${this.apiUrl}/standings/session?seasonId=${this.selectedSeasonId}&weekId=${this.selectedWeekId}`).subscribe({
      next: (response) => {
        // The session endpoint returns the week data directly
        this.summaryData = {
          week: response.week,
          session: response.session,
          flights: response.flights.map((flight: any) => ({
            id: flight.id,
            name: flight.name,
            players: flight.players.map((player: any, index: number) => ({
              id: player.id,
              displayName: player.displayName,
              handicap: player.handicap || 0,
              averageScore: player.averageScore || 0,
              grossScore: player.grossScore || 0,
              thisWeekPoints: player.thisWeekPoints || 0,
              sessionTotal: player.sessionTotal || 0,
              isAbsent: player.isAbsent,
              rank: index + 1
            }))
          })),
          nextWeekMatchups: [] // Can be added later if needed
        };
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading summary data:', error);
        this.error = 'Failed to load league summary data. Please try again.';
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

  getSessionDisplayText(): string {
    if (!this.summaryData?.session) return '';

    const sessionNumber = this.summaryData.session.number;
    const startWeek = this.summaryData.session.startWeekNumber;
    const currentWeek = this.summaryData.session.currentWeekNumber;

    if (startWeek === currentWeek) {
      return `Session ${sessionNumber} (Week ${startWeek})`;
    } else {
      return `Session ${sessionNumber} (Weeks ${startWeek}-${currentWeek})`;
    }
  }

  formatWeekDate(week: Week): string {
    return new Date(week.date).toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  exportToPdf(): void {
    if (!this.selectedWeekId) return;

    this.isLoading = true;
    this.http.get(`${this.apiUrl}/pdf/summary/week/${this.selectedWeekId}`, { responseType: 'blob' }).subscribe({
      next: (response) => {
        const blob = new Blob([response], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        const week = this.weeks.find(w => w.id === this.selectedWeekId);
        const weekNumber = week?.weekNumber || 'Unknown';
        link.download = `week_${weekNumber}_summary.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error exporting PDF:', error);
        this.error = 'Failed to export PDF';
        this.isLoading = false;
      }
    });
  }

  // TrackBy functions for performance
  trackByFlightId(index: number, flight: Flight): string {
    return flight.id;
  }

  trackByPlayerId(index: number, player: Player): string {
    return player.id;
  }
}
