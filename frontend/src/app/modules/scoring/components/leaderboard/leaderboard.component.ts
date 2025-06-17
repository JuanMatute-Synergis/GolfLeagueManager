import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ScoringService } from '../../services/scoring.service';
import { Season, Week, ScoreEntry, Player } from '../../models/week.model';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col">
      <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center mb-8 flex-shrink-0 gap-4">
        <div>
          <h1 class="text-3xl font-semibold text-foreground">Weekly Leaderboard</h1>
          <p class="mt-1 text-sm sm:text-base text-muted-foreground">View weekly golf results and rankings</p>
        </div>
        <div class="flex items-center space-x-4">
          <select 
            [(ngModel)]="selectedSeasonId"
            (change)="onSeasonChange()"
            class="px-4 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
            <option value="">Select Season</option>
            <option *ngFor="let season of seasons" [value]="season.id">{{ season.name }}</option>
          </select>
          
          <select 
            [(ngModel)]="selectedWeekId"
            (change)="onWeekChange()"
            [disabled]="!selectedSeasonId || weeks.length === 0"
            class="px-4 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20 disabled:bg-muted disabled:cursor-not-allowed">
            <option value="">Select Week</option>
            <option *ngFor="let week of weeks" [value]="week.id">{{ getWeekDisplayName(week) }}</option>
          </select>
        </div>
      </div>

      <div *ngIf="selectedWeekId && selectedWeek" class="flex-1 flex flex-col">
        <!-- Week Info Banner -->
        <div class="bg-gradient-to-r from-primary to-primary/80 text-primary-foreground rounded-lg p-6 mb-6 flex-shrink-0">
          <div class="flex items-center justify-between">
            <div>
              <h2 class="text-2xl font-bold mb-2">{{ selectedWeek.name }}</h2>
              <p class="opacity-90">{{ selectedWeek.startDate | date:'mediumDate' }} - {{ selectedWeek.endDate | date:'mediumDate' }}</p>
            </div>
            <div class="text-right">
              <div class="text-3xl font-bold">{{ leaderboard.length }}</div>
              <div class="opacity-90">players competed</div>
            </div>
          </div>
        </div>

        <!-- Top 3 Podium -->
        <div *ngIf="leaderboard.length >= 3" class="mb-8">
          <h2 class="text-2xl font-bold text-gray-900 mb-6 text-center">🏆 Top 3 Finishers</h2>
          <div class="flex justify-center items-end space-x-4 mb-8">
            <!-- 2nd Place -->
            <div class="bg-gradient-to-t from-gray-300 to-gray-400 text-white p-6 rounded-lg text-center transform scale-90">
              <div class="text-4xl mb-2">🥈</div>
              <div class="font-bold text-lg">{{ leaderboard[1].player?.firstName }} {{ leaderboard[1].player?.lastName }}</div>
              <div class="text-sm opacity-90 mb-2">{{ leaderboard[1].score }} strokes</div>
              <div class="text-xl font-bold">{{ leaderboard[1].pointsEarned }} pts</div>
              <div class="text-xs mt-1">2nd Place</div>
            </div>
            
            <!-- 1st Place -->
            <div class="bg-gradient-to-t from-yellow-400 to-yellow-500 text-white p-8 rounded-lg text-center transform scale-100">
              <div class="text-5xl mb-3">🏆</div>
              <div class="font-bold text-xl">{{ leaderboard[0].player?.firstName }} {{ leaderboard[0].player?.lastName }}</div>
              <div class="text-sm opacity-90 mb-3">{{ leaderboard[0].score }} strokes</div>
              <div class="text-2xl font-bold">{{ leaderboard[0].pointsEarned }} pts</div>
              <div class="text-sm mt-2">🎉 WINNER 🎉</div>
            </div>
            
            <!-- 3rd Place -->
            <div class="bg-gradient-to-t from-amber-600 to-amber-700 text-white p-6 rounded-lg text-center transform scale-90">
              <div class="text-4xl mb-2">🥉</div>
              <div class="font-bold text-lg">{{ leaderboard[2].player?.firstName }} {{ leaderboard[2].player?.lastName }}</div>
              <div class="text-sm opacity-90 mb-2">{{ leaderboard[2].score }} strokes</div>
              <div class="text-xl font-bold">{{ leaderboard[2].pointsEarned }} pts</div>
              <div class="text-xs mt-1">3rd Place</div>
            </div>
          </div>
        </div>

        <!-- Full Leaderboard -->
        <div class="bg-card border border-border rounded-lg shadow-sm flex-1 flex flex-col min-h-0">
          <div class="px-6 py-4 border-b border-border bg-muted/20 flex-shrink-0">
            <h2 class="text-xl font-semibold text-foreground">Complete Results</h2>
          </div>
          
          <div *ngIf="leaderboard.length === 0" class="p-8 text-center flex-1 flex items-center justify-center">
            <div>
              <div class="text-6xl mb-4">🏌️‍♂️</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">No scores yet</h3>
              <p class="text-muted-foreground mb-4">Scores haven't been entered for this week yet.</p>
              <button 
                (click)="navigateToScoreEntry()"
                class="bg-primary text-primary-foreground px-4 py-2 rounded-md font-medium transition-colors hover:bg-primary/90">
                Enter Scores
              </button>
            </div>
          </div>

          <div *ngIf="leaderboard.length > 0" class="flex-1 overflow-hidden">
            <div class="h-full overflow-y-auto">
              <table class="w-full border-collapse">
                <thead class="bg-muted/50 text-xs uppercase text-muted-foreground sticky top-0">
                  <tr>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Rank</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Player</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Score</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Points</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Performance</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let score of leaderboard; let i = index"
                       [class]="'hover:bg-muted/20 transition-colors ' + getRankRowClass(i + 1)">
                    <td class="p-4 border-b border-border">
                      <div class="flex items-center space-x-3">
                        <span
                          class="flex items-center justify-center w-8 h-8 rounded-full text-white text-sm font-bold"
                          [style.background-color]="getRankColor(i + 1)">
                          {{ i + 1 }}
                        </span>
                        <span *ngIf="i < 3" class="text-lg">{{ getRankEmoji(i + 1) }}</span>
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="font-semibold text-foreground">
                        {{ score.player?.firstName }} {{ score.player?.lastName }}
                      </div>
                      <div class="text-sm text-muted-foreground">{{ score.player?.email }}</div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="flex items-center space-x-2">
                        <span class="text-2xl font-bold text-foreground">{{ score.score }}</span>
                        <span class="text-sm text-muted-foreground">strokes</span>
                      </div>
                      <div class="text-sm text-muted-foreground">
                        {{ getScoreRelativeToPar(score.score) }}
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="flex items-center space-x-2">
                        <span class="text-xl font-bold text-primary">{{ score.pointsEarned }}</span>
                        <span class="text-sm text-muted-foreground">points</span>
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="flex items-center space-x-2">
                        <span class="text-2xl">{{ getPerformanceEmoji(i + 1, leaderboard.length) }}</span>
                        <span class="text-sm font-medium text-foreground">{{ getPerformanceText(i + 1, leaderboard.length) }}</span>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- Week Statistics -->
        <div *ngIf="leaderboard.length > 0" class="grid grid-cols-1 md:grid-cols-4 gap-6 mt-6 flex-shrink-0">
          <div class="bg-card border border-border rounded-lg shadow-sm p-6 text-center">
            <div class="text-3xl mb-2">🎯</div>
            <div class="text-2xl font-bold text-foreground">{{ getLowestScore() }}</div>
            <div class="text-sm text-muted-foreground">Lowest Score</div>
          </div>
          
          <div class="bg-card border border-border rounded-lg shadow-sm p-6 text-center">
            <div class="text-3xl mb-2">📊</div>
            <div class="text-2xl font-bold text-foreground">{{ getAverageScore() | number:'1.1-1' }}</div>
            <div class="text-sm text-muted-foreground">Average Score</div>
          </div>
          
          <div class="bg-card border border-border rounded-lg shadow-sm p-6 text-center">
            <div class="text-3xl mb-2">🏆</div>
            <div class="text-2xl font-bold text-foreground">{{ getHighestPoints() }}</div>
            <div class="text-sm text-muted-foreground">Most Points</div>
          </div>
          
          <div class="bg-card border border-border rounded-lg shadow-sm p-6 text-center">
            <div class="text-3xl mb-2">👥</div>
            <div class="text-2xl font-bold text-foreground">{{ leaderboard.length }}</div>
            <div class="text-sm text-muted-foreground">Total Players</div>
          </div>
        </div>
      </div>

      <!-- Selection Message -->
      <div *ngIf="!selectedWeekId" class="text-center mt-8 flex-1 flex items-center justify-center">
        <div>
          <div class="text-6xl mb-4">🏆</div>
          <h3 class="text-xl font-semibold text-foreground mb-2">Select a Week</h3>
          <p class="text-muted-foreground">Choose a season and week from the dropdowns above to view the leaderboard.</p>
        </div>
      </div>
    </div>
  `,
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
    }
  }

  loadWeeks() {
    this.scoringService.getWeeksBySeason(this.selectedSeasonId).subscribe({
      next: (weeks) => {
        this.weeks = weeks.sort((a, b) => b.weekNumber - a.weekNumber); // Latest first
        
        // Auto-select the most recent week with scores
        if (weeks.length > 0) {
          this.selectedWeekId = weeks[0].id;
          this.onWeekChange();
        }
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

  loadLeaderboard() {
    if (!this.selectedWeekId) return;
    
    this.scoringService.getWeeklyLeaderboard(this.selectedWeekId).subscribe({
      next: (scores) => {
        this.leaderboard = scores;
      },
      error: (error: any) => console.error('Error loading leaderboard:', error)
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
    const par = 72; // Standard par
    const difference = score - par;
    if (difference === 0) return 'Even par';
    if (difference > 0) return `+${difference} over par`;
    return `${difference} under par`;
  }

  getLowestScore(): number {
    if (this.leaderboard.length === 0) return 0;
    return Math.min(...this.leaderboard.map(s => s.score));
  }

  getAverageScore(): number {
    if (this.leaderboard.length === 0) return 0;
    const total = this.leaderboard.reduce((sum, s) => sum + s.score, 0);
    return total / this.leaderboard.length;
  }

  getHighestPoints(): number {
    if (this.leaderboard.length === 0) return 0;
    return Math.max(...this.leaderboard.map(s => s.pointsEarned));
  }

  getWeekDisplayName(week: Week): string {
    const startDate = new Date(week.startDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    const endDate = new Date(week.endDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    return `${week.name} (${startDate} - ${endDate})`;
  }

  navigateToScoreEntry() {
    this.router.navigate(['/scoring/score-entry'], { 
      queryParams: { weekId: this.selectedWeekId } 
    });
  }
}