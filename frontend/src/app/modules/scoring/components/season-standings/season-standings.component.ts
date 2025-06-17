import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ScoringService } from '../../services/scoring.service';
import { Season, PlayerSeasonStats } from '../../models/week.model';

@Component({
  selector: 'app-season-standings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col">
      <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center mb-8 flex-shrink-0 gap-4">
        <div>
          <h1 class="text-3xl font-semibold text-foreground">Season Standings</h1>
          <p class="mt-1 text-sm sm:text-base text-muted-foreground">View complete season rankings and statistics</p>
        </div>
        <div class="flex items-center space-x-4">
          <select 
            [(ngModel)]="selectedSeasonId"
            (change)="onSeasonChange()"
            class="px-4 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
            <option value="">Select Season</option>
            <option *ngFor="let season of seasons" [value]="season.id">{{ season.name }}</option>
          </select>
          <button 
            (click)="loadStandings()"
            [disabled]="!selectedSeasonId || loading"
            class="bg-primary text-primary-foreground px-4 py-2 rounded-md font-medium transition-colors hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2">
            <span class="animate-spin" *ngIf="loading">⟳</span>
            🔄 Refresh
          </button>
        </div>
      </div>

      <div *ngIf="selectedSeasonId" class="flex-1 flex flex-col">
        <!-- Season Stats Overview -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8 flex-shrink-0" *ngIf="standings.length > 0">
          <!-- Season Leader -->
          <div class="bg-card border border-border rounded-lg shadow-sm p-6">
            <div class="text-center">
              <div class="text-4xl mb-3">🏆</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">Season Leader</h3>
              <p class="text-xl font-bold text-foreground mb-1">{{ standings[0].playerName }}</p>
              <p class="text-sm text-primary font-medium">{{ standings[0].totalPoints }} points</p>
            </div>
          </div>

          <!-- Active Players -->
          <div class="bg-card border border-border rounded-lg shadow-sm p-6">
            <div class="text-center">
              <div class="text-4xl mb-3">👥</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">Active Players</h3>
              <p class="text-2xl font-bold text-foreground mb-1">{{ standings.length }}</p>
              <p class="text-sm text-muted-foreground">participating</p>
            </div>
          </div>

          <!-- Best Average -->
          <div class="bg-card border border-border rounded-lg shadow-sm p-6">
            <div class="text-center">
              <div class="text-4xl mb-3">📈</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">Best Average</h3>
              <p class="text-2xl font-bold text-foreground mb-1">{{ getBestAverage() | number:'1.1-1' }}</p>
              <p class="text-sm text-muted-foreground">strokes</p>
            </div>
          </div>

          <!-- Best Score -->
          <div class="bg-card border border-border rounded-lg shadow-sm p-6">
            <div class="text-center">
              <div class="text-4xl mb-3">⭐</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">Best Score</h3>
              <p class="text-2xl font-bold text-foreground mb-1">{{ getBestScore() }}</p>
              <p class="text-sm text-muted-foreground">single round</p>
            </div>
          </div>
        </div>

        <!-- Standings Table -->
        <div class="bg-card border border-border rounded-lg shadow-sm flex-1 flex flex-col min-h-0">
          <div class="px-6 py-4 border-b border-border bg-muted/20 flex-shrink-0">
            <div class="flex justify-between items-center">
              <h2 class="text-xl font-semibold text-foreground">Season Rankings</h2>
              <div class="flex items-center space-x-2">
                <span class="text-sm text-muted-foreground">Sort by:</span>
                <select 
                  [(ngModel)]="selectedSort"
                  (change)="onSortChange()"
                  class="px-3 py-1 border border-border rounded text-sm bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
                  <option *ngFor="let option of sortOptions" [value]="option.value">{{ option.label }}</option>
                </select>
              </div>
            </div>
          </div>

          <div *ngIf="standings.length > 0" class="flex-1 overflow-hidden">
            <div class="h-full overflow-y-auto">
              <table class="w-full border-collapse">
                <thead class="bg-muted/50 text-xs uppercase text-muted-foreground sticky top-0">
                  <tr>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Rank</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Player</th>
                    <th class="text-center font-semibold text-foreground p-4 border-b border-border">Total Points</th>
                    <th class="text-center font-semibold text-foreground p-4 border-b border-border">Rounds</th>
                    <th class="text-center font-semibold text-foreground p-4 border-b border-border">Avg Score</th>
                    <th class="text-center font-semibold text-foreground p-4 border-b border-border">Best Score</th>
                    <th class="text-center font-semibold text-foreground p-4 border-b border-border">Worst Score</th>
                    <th class="text-center font-semibold text-foreground p-4 border-b border-border">Form</th>
                  </tr>
                </thead>
                <tbody [class.animate-pulse]="loading">
                  <tr *ngFor="let player of sortedStandings; let i = index" 
                      [class]="getRowClass(i + 1)" 
                      class="hover:bg-muted/20 transition-colors duration-150">
                    <td class="p-4 border-b border-border">
                      <div class="flex items-center space-x-2">
                        <span class="rank-badge" [class]="getRankBadgeClass(i + 1)">
                          {{ i + 1 }}
                        </span>
                        <span *ngIf="i < 3" class="text-lg">{{ getRankIcon(i + 1) }}</span>
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="font-semibold text-foreground">{{ player.playerName }}</div>
                    </td>
                    <td class="p-4 border-b border-border text-center">
                      <span class="text-xl font-bold text-primary">{{ player.totalPoints }}</span>
                    </td>
                    <td class="p-4 border-b border-border text-center">
                      <span class="font-medium text-foreground">{{ player.roundsPlayed }}</span>
                    </td>
                    <td class="p-4 border-b border-border text-center">
                      <span class="font-bold" [class]="getAverageScoreClass(player.averageScore)">
                        {{ player.averageScore | number:'1.1-1' }}
                      </span>
                    </td>
                    <td class="p-4 border-b border-border text-center">
                      <span class="font-bold text-green-600">{{ player.bestScore }}</span>
                    </td>
                    <td class="p-4 border-b border-border text-center">
                      <span class="font-bold text-red-600">{{ player.worstScore }}</span>
                    </td>
                    <td class="p-4 border-b border-border text-center">
                      <span 
                        class="px-2 py-1 rounded-full text-xs font-bold"
                        [class]="getFormClass(player)">
                        {{ getPlayerForm(player) }}
                      </span>
                    </td>
                  </tr>
              </tbody>
            </table>

            <!-- Pagination for large datasets -->
            <div *ngIf="sortedStandings.length > 25" class="px-6 py-4 border-t border-border bg-muted/20">
              <div class="flex items-center justify-between">
                <div class="text-sm text-muted-foreground">
                  Showing {{ ((currentPage - 1) * pageSize) + 1 }} to {{ Math.min(currentPage * pageSize, sortedStandings.length) }} of {{ sortedStandings.length }} players
                </div>
                <div class="flex space-x-2">
                  <button 
                    (click)="previousPage()"
                    [disabled]="currentPage === 1"
                    class="px-3 py-1 border border-border rounded text-sm bg-background text-foreground hover:bg-muted/50 disabled:bg-muted disabled:text-muted-foreground disabled:cursor-not-allowed transition-colors">
                    Previous
                  </button>
                  <button 
                    (click)="nextPage()"
                    [disabled]="currentPage === totalPages"
                    class="px-3 py-1 border border-border rounded text-sm bg-background text-foreground hover:bg-muted/50 disabled:bg-muted disabled:text-muted-foreground disabled:cursor-not-allowed transition-colors">
                    Next
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Detailed Statistics -->
          <div *ngIf="standings.length > 0" class="px-6 py-6 border-t border-border bg-muted/20">
            <h3 class="text-lg font-semibold mb-4 text-foreground">Season Statistics</h3>
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <!-- Points Distribution -->
              <div class="bg-card rounded-lg p-4 shadow-sm border border-border">
                <h4 class="font-semibold mb-3 text-foreground">Points Distribution</h4>
                <div class="space-y-3">
                  <div class="flex justify-between items-center">
                    <span class="text-muted-foreground">Average Points per Player:</span>
                    <span class="font-bold text-foreground">{{ getAveragePoints() | number:'1.0-0' }}</span>
                  </div>
                  <div class="flex justify-between items-center">
                    <span class="text-muted-foreground">Highest Single Week:</span>
                    <span class="font-bold text-foreground">{{ getHighestPoints() }}</span>
                  </div>
                  <div class="flex justify-between items-center">
                    <span class="text-muted-foreground">Total Points Awarded:</span>
                    <span class="font-bold text-foreground">{{ getTotalPoints() }}</span>
                  </div>
                </div>
              </div>

              <!-- Score Statistics -->
              <div class="bg-card rounded-lg p-4 shadow-sm border border-border">
                <h4 class="font-semibold mb-3 text-foreground">Score Statistics</h4>
                <div class="space-y-3">
                  <div class="flex justify-between items-center">
                    <span class="text-muted-foreground">Season Average:</span>
                    <span class="font-bold text-foreground">{{ getOverallAverage() | number:'1.1-1' }}</span>
                  </div>
                  <div class="flex justify-between items-center">
                    <span class="text-muted-foreground">Best Single Round:</span>
                    <span class="font-bold text-green-600">{{ getBestScore() }}</span>
                  </div>
                  <div class="flex justify-between items-center">
                    <span class="text-muted-foreground">Worst Single Round:</span>
                    <span class="font-bold text-red-600">{{ getWorstScore() }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- No Standings Message -->
          <div *ngIf="standings.length === 0" class="text-center py-12 flex-1 flex items-center justify-center">
            <div>
              <div class="text-6xl mb-4">📊</div>
              <h3 class="text-xl font-semibold mb-2 text-foreground">No Standings Available</h3>
              <p class="text-muted-foreground">No scores have been entered for this season yet.</p>
            </div>
          </div>
        </div>
      </div>

      <!-- No Selection Message -->
      <div *ngIf="!selectedSeasonId" class="text-center mt-12">
        <div class="text-6xl mb-4">ℹ️</div>
        <h3 class="text-xl font-semibold mb-2 text-foreground">Select a Season</h3>
        <p class="text-muted-foreground">Choose a season from the dropdown above to view the standings.</p>
      </div>
    </div>
  `,
  styles: [`
    .rank-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 2rem;
      height: 2rem;
      border-radius: 50%;
      font-weight: bold;
      font-size: 0.875rem;
    }

    .rank-1 {
      background: linear-gradient(45deg, #ffd700, #ffed4e);
      color: #1a1a1a;
      box-shadow: 0 2px 8px rgba(255, 215, 0, 0.3);
    }

    .rank-2 {
      background: linear-gradient(45deg, #c0c0c0, #e5e5e5);
      color: #1a1a1a;
      box-shadow: 0 2px 8px rgba(192, 192, 192, 0.3);
    }

    .rank-3 {
      background: linear-gradient(45deg, #cd7f32, #d2941d);
      color: white;
      box-shadow: 0 2px 8px rgba(205, 127, 50, 0.3);
    }

    .rank-other {
      background: #6366f1;
      color: white;
    }

    .row-winner {
      background: linear-gradient(90deg, rgba(255, 215, 0, 0.1), transparent);
    }

    .row-podium {
      background: linear-gradient(90deg, rgba(156, 163, 175, 0.1), transparent);
    }

    .avg-excellent { color: #10b981; }
    .avg-good { color: #3b82f6; }
    .avg-average { color: #f59e0b; }
    .avg-poor { color: #ef4444; }

    .form-hot { background: #10b981; color: white; }
    .form-warm { background: #f59e0b; color: white; }
    .form-cold { background: #ef4444; color: white; }
    .form-neutral { background: #6b7280; color: white; }

    .space-y-3 > * + * {
      margin-top: 0.75rem;
    }
  `]
})
export class SeasonStandingsComponent implements OnInit {
  seasons: Season[] = [];
  selectedSeasonId: string = '';
  standings: PlayerSeasonStats[] = [];
  sortedStandings: PlayerSeasonStats[] = [];
  loading = false;
  selectedSort = 'points';
  
  // Pagination properties
  currentPage = 1;
  pageSize = 25;
  Math = Math;

  sortOptions = [
    { label: 'Total Points', value: 'points' },
    { label: 'Average Score', value: 'average' },
    { label: 'Rounds Played', value: 'rounds' },
    { label: 'Best Score', value: 'best' },
    { label: 'Player Name', value: 'name' }
  ];

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
      error: (error) => console.error('Error loading seasons:', error)
    });
  }

  onSeasonChange() {
    if (this.selectedSeasonId) {
      this.loadStandings();
    }
  }

  loadStandings() {
    if (this.selectedSeasonId) {
      this.loading = true;
      this.scoringService.getSeasonStandings(this.selectedSeasonId).subscribe({
        next: (standings) => {
          this.standings = standings;
          this.applySorting();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading standings:', error);
          this.standings = [];
          this.sortedStandings = [];
          this.loading = false;
        }
      });
    }
  }

  onSortChange() {
    this.applySorting();
  }

  applySorting() {
    let sorted = [...this.standings];
    
    switch (this.selectedSort) {
      case 'points':
        sorted.sort((a, b) => b.totalPoints - a.totalPoints);
        break;
      case 'average':
        sorted.sort((a, b) => a.averageScore - b.averageScore);
        break;
      case 'rounds':
        sorted.sort((a, b) => b.roundsPlayed - a.roundsPlayed);
        break;
      case 'best':
        sorted.sort((a, b) => a.bestScore - b.bestScore);
        break;
      case 'name':
        sorted.sort((a, b) => a.playerName.localeCompare(b.playerName));
        break;
    }

    this.sortedStandings = sorted;
    this.currentPage = 1; // Reset to first page when sorting
  }

  // Pagination methods
  get totalPages(): number {
    return Math.ceil(this.sortedStandings.length / this.pageSize);
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  // Display helper methods
  getRankIcon(rank: number): string {
    switch (rank) {
      case 1: return '🏆';
      case 2: return '🥈';
      case 3: return '🥉';
      default: return '';
    }
  }

  getRankBadgeClass(rank: number): string {
    switch (rank) {
      case 1: return 'rank-badge rank-1';
      case 2: return 'rank-badge rank-2';
      case 3: return 'rank-badge rank-3';
      default: return 'rank-badge rank-other';
    }
  }

  getRowClass(rank: number): string {
    if (rank === 1) return 'row-winner';
    if (rank <= 3) return 'row-podium';
    return '';
  }

  getAverageScoreClass(average: number): string {
    if (average <= 72) return 'avg-excellent';
    if (average <= 76) return 'avg-good';
    if (average <= 82) return 'avg-average';
    return 'avg-poor';
  }

  getPlayerForm(player: PlayerSeasonStats): string {
    const avgScore = player.averageScore;
    const roundsPlayed = player.roundsPlayed;
    
    if (roundsPlayed < 3) return 'NEW';
    if (avgScore <= 74) return 'HOT';
    if (avgScore <= 78) return 'WARM';
    if (avgScore <= 85) return 'COOL';
    return 'COLD';
  }

  getFormClass(player: PlayerSeasonStats): string {
    const form = this.getPlayerForm(player);
    switch (form) {
      case 'HOT': return 'form-hot';
      case 'WARM': return 'form-warm';
      case 'COOL': return 'form-neutral';
      case 'COLD': return 'form-cold';
      default: return 'form-neutral';
    }
  }

  // Statistics calculations
  getBestAverage(): number {
    if (this.standings.length === 0) return 0;
    return Math.min(...this.standings.map(s => s.averageScore));
  }

  getBestScore(): number {
    if (this.standings.length === 0) return 0;
    return Math.min(...this.standings.map(s => s.bestScore));
  }

  getWorstScore(): number {
    if (this.standings.length === 0) return 0;
    return Math.max(...this.standings.map(s => s.worstScore));
  }

  getAveragePoints(): number {
    if (this.standings.length === 0) return 0;
    const total = this.standings.reduce((sum, s) => sum + s.totalPoints, 0);
    return total / this.standings.length;
  }

  getHighestPoints(): number {
    if (this.standings.length === 0) return 0;
    return Math.max(...this.standings.map(s => s.totalPoints));
  }

  getTotalPoints(): number {
    return this.standings.reduce((sum, s) => sum + s.totalPoints, 0);
  }

  getOverallAverage(): number {
    if (this.standings.length === 0) return 0;
    const totalStrokes = this.standings.reduce((sum, s) => sum + (s.averageScore * s.roundsPlayed), 0);
    const totalRounds = this.standings.reduce((sum, s) => sum + s.roundsPlayed, 0);
    return totalRounds > 0 ? totalStrokes / totalRounds : 0;
  }
}
