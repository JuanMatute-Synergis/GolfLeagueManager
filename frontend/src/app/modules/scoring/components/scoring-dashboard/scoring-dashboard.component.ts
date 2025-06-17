import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScoringService } from '../../services/scoring.service';
import { Season, Week, PlayerSeasonStats } from '../../models/week.model';

@Component({
  selector: 'app-scoring-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col">
      <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center mb-8 flex-shrink-0 gap-4">
        <div>
          <h1 class="text-3xl font-semibold text-foreground">Scoring System</h1>
          <p class="mt-1 text-sm sm:text-base text-muted-foreground">Manage scores, view leaderboards, and track season standings</p>
        </div>
        <div class="flex items-center space-x-4">
          <select 
            [(ngModel)]="selectedSeasonId"
            (change)="onSeasonChange()"
            class="px-4 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
            <option value="">Select Season</option>
            <option *ngFor="let season of seasons" [value]="season.id">{{ season.name }}</option>
          </select>
        </div>
      </div>

      <div *ngIf="selectedSeasonId" class="flex-1 flex flex-col">
        <!-- Stats Cards -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8 flex-shrink-0">
          <!-- Current Week -->
          <div class="bg-card border border-border rounded-lg shadow-sm p-6">
            <div class="text-center">
              <div class="text-4xl mb-3">📅</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">Current Week</h3>
              <p class="text-2xl font-bold text-foreground mb-1">{{ currentWeek?.name || 'No Active Week' }}</p>
              <p class="text-sm text-muted-foreground">
                {{ currentWeek ? (currentWeek.startDate | date:'shortDate') + ' - ' + (currentWeek.endDate | date:'shortDate') : '' }}
              </p>
            </div>
          </div>

          <!-- Total Weeks -->
          <div class="bg-card border border-border rounded-lg shadow-sm p-6">
            <div class="text-center">
              <div class="text-4xl mb-3">📋</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">Total Weeks</h3>
              <p class="text-2xl font-bold text-foreground mb-1">{{ weeks.length }}</p>
              <p class="text-sm text-muted-foreground">in season</p>
            </div>
          </div>

          <!-- Leader -->
          <div class="bg-card border border-border rounded-lg shadow-sm p-6">
            <div class="text-center">
              <div class="text-4xl mb-3">🏆</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">Leader</h3>
              <p class="text-lg font-bold text-foreground mb-1">{{ seasonLeader?.playerName || 'No Scores Yet' }}</p>
              <p class="text-sm text-muted-foreground">{{ seasonLeader?.totalPoints || 0 }} points</p>
            </div>
          </div>

          <!-- Active Players -->
          <div class="bg-card border border-border rounded-lg shadow-sm p-6">
            <div class="text-center">
              <div class="text-4xl mb-3">👥</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">Active Players</h3>
              <p class="text-2xl font-bold text-foreground mb-1">{{ seasonStandings.length }}</p>
              <p class="text-sm text-muted-foreground">with scores</p>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="bg-card border border-border rounded-lg shadow-sm p-6 mb-8 flex-shrink-0">
          <h2 class="text-xl font-semibold text-foreground mb-4">Quick Actions</h2>
          <div class="flex flex-wrap gap-3">
            <button 
              (click)="navigateTo('/settings?tab=weeks')"
              class="bg-primary text-primary-foreground px-4 py-2 rounded-md font-medium transition-colors hover:bg-primary/90 flex items-center gap-2">
              📅 Manage Weeks
            </button>
            
            <button 
              (click)="navigateTo('/settings?tab=scores')"
              [disabled]="!currentWeek"
              class="bg-primary text-primary-foreground px-4 py-2 rounded-md font-medium transition-colors hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2">
              ✏️ Enter Scores
            </button>
            
            <button 
              (click)="navigateTo('/scoring/leaderboard')"
              class="bg-primary text-primary-foreground px-4 py-2 rounded-md font-medium transition-colors hover:bg-primary/90 flex items-center gap-2">
              📊 View Leaderboard
            </button>
            
            <button 
              (click)="navigateTo('/scoring/standings')"
              class="bg-primary text-primary-foreground px-4 py-2 rounded-md font-medium transition-colors hover:bg-primary/90 flex items-center gap-2">
              🏆 Season Standings
            </button>
          </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 flex-1">
          <!-- Season Weeks -->
          <div class="lg:col-span-2">
            <div class="bg-card border border-border rounded-lg shadow-sm h-full flex flex-col">
              <div class="p-4 border-b border-border bg-muted/20 flex-shrink-0">
                <h2 class="text-lg font-semibold text-foreground">Season Weeks</h2>
              </div>
              <div class="flex-1 overflow-hidden">
                <div class="h-full overflow-y-auto">
                  <table class="w-full border-collapse">
                    <thead class="bg-muted/50 text-xs uppercase text-muted-foreground sticky top-0">
                      <tr>
                        <th class="text-left font-semibold text-foreground p-4 border-b border-border">Week</th>
                        <th class="text-left font-semibold text-foreground p-4 border-b border-border">Name</th>
                        <th class="text-left font-semibold text-foreground p-4 border-b border-border">Date Range</th>
                        <th class="text-left font-semibold text-foreground p-4 border-b border-border">Status</th>
                        <th class="text-left font-semibold text-foreground p-4 border-b border-border">Scores</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngFor="let week of weeks" class="hover:bg-muted/20 transition-colors">
                        <td class="p-4 border-b border-border text-foreground">{{ week.weekNumber }}</td>
                        <td class="p-4 border-b border-border text-foreground font-medium">{{ week.name }}</td>
                        <td class="p-4 border-b border-border text-foreground">{{ week.startDate | date:'shortDate' }} - {{ week.endDate | date:'shortDate' }}</td>
                        <td class="p-4 border-b border-border">
                          <span [class]="'px-2 py-1 text-xs rounded-full ' + getWeekStatusClass(week)">
                            {{ getWeekStatus(week) }}
                          </span>
                        </td>
                        <td class="p-4 border-b border-border text-foreground">{{ week.scoreEntries?.length || 0 }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>

          <!-- Top Players -->
          <div class="lg:col-span-1">
            <div class="bg-card border border-border rounded-lg shadow-sm h-full flex flex-col">
              <div class="p-4 border-b border-border bg-muted/20 flex-shrink-0">
                <h2 class="text-lg font-semibold text-foreground">Top 5 Players</h2>
              </div>
              <div class="flex-1 overflow-hidden">
                <div class="h-full overflow-y-auto p-4">
                  <div class="space-y-3">
                    <div 
                      *ngFor="let player of seasonStandings.slice(0, 5); let i = index"
                      class="flex items-center justify-between p-3 border border-border rounded-lg bg-background">
                      <div class="flex items-center space-x-3">
                        <span 
                          class="flex items-center justify-center w-8 h-8 rounded-full text-white text-sm font-bold"
                          [style.background-color]="getRankColor(i + 1)">
                          {{ i + 1 }}
                        </span>
                        <div>
                          <div class="font-semibold text-foreground">{{ player.playerName }}</div>
                          <div class="text-sm text-muted-foreground">{{ player.roundsPlayed }} rounds</div>
                        </div>
                      </div>
                      <div class="text-right">
                        <div class="font-bold text-lg text-foreground">{{ player.totalPoints }}</div>
                        <div class="text-sm text-muted-foreground">{{ player.averageScore | number:'1.1-1' }} avg</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Select Season Message -->
      <div *ngIf="!selectedSeasonId" class="text-center mt-8 flex-1 flex items-center justify-center">
        <div>
          <div class="text-6xl mb-4">ℹ️</div>
          <h3 class="text-xl font-semibold text-foreground mb-2">Select a Season</h3>
          <p class="text-muted-foreground">Choose a season from the dropdown above to view scoring information.</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    /* Additional custom styles if needed */
  `]
})
export class ScoringDashboardComponent implements OnInit {
  seasons: Season[] = [];
  selectedSeasonId: string = '';
  weeks: Week[] = [];
  currentWeek: Week | null = null;
  seasonStandings: PlayerSeasonStats[] = [];
  seasonLeader: PlayerSeasonStats | null = null;

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
      this.loadSeasonData();
    }
  }

  loadSeasonData() {
    // Load weeks
    this.scoringService.getWeeksBySeason(this.selectedSeasonId).subscribe({
      next: (weeks) => this.weeks = weeks,
      error: (error) => console.error('Error loading weeks:', error)
    });

    // Load current week
    this.scoringService.getCurrentWeek(this.selectedSeasonId).subscribe({
      next: (week) => this.currentWeek = week,
      error: (error) => this.currentWeek = null
    });

    // Load season standings
    this.scoringService.getSeasonStandings(this.selectedSeasonId).subscribe({
      next: (standings) => {
        this.seasonStandings = standings;
        this.seasonLeader = standings.length > 0 ? standings[0] : null;
      },
      error: (error) => console.error('Error loading standings:', error)
    });
  }

  getWeekStatus(week: Week): string {
    const now = new Date();
    const start = new Date(week.startDate);
    const end = new Date(week.endDate);

    if (now < start) return 'Upcoming';
    if (now > end) return 'Completed';
    return 'Active';
  }

  getWeekStatusClass(week: Week): string {
    const status = this.getWeekStatus(week);
    switch (status) {
      case 'Active': return 'bg-primary/10 text-primary border border-primary/20';
      case 'Completed': return 'bg-muted text-muted-foreground border border-border';
      case 'Upcoming': return 'bg-secondary/10 text-secondary-foreground border border-secondary/20';
      default: return 'bg-muted text-muted-foreground border border-border';
    }
  }

  getRankColor(rank: number): string {
    switch (rank) {
      case 1: return '#FFD700'; // Gold
      case 2: return '#C0C0C0'; // Silver
      case 3: return '#CD7F32'; // Bronze
      default: return '#6366f1'; // Indigo
    }
  }

  navigateTo(route: string) {
    this.router.navigate([route]);
  }
}
