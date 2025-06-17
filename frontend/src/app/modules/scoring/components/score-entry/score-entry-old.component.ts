import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ScoringService } from '../../services/scoring.service';
import { MatchupService } from '../../../settings/services/matchup.service';
import { Season, Week, ScoreEntry, Player, PlayerWithFlight, BulkScoreEntry } from '../../models/week.model';
import { Matchup } from '../../../settings/services/matchup.service';

@Component({
  selector: 'app-score-entry',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col">
      <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center mb-8 flex-shrink-0 gap-4">
        <div>
          <h1 class="text-3xl font-semibold text-foreground">Score Entry</h1>
          <p class="mt-1 text-sm sm:text-base text-muted-foreground">Enter golf scores for players</p>
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
        <!-- Consolidated Week Info & Actions -->
        <div class="bg-card border border-border rounded-lg shadow-sm p-4 mb-6 flex-shrink-0">
          <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
            <!-- Week Info & Stats -->
            <div class="flex flex-col sm:flex-row sm:items-center gap-4">
              <div class="flex-shrink-0">
                <h3 class="text-lg font-semibold text-foreground">{{ selectedWeek.name }}</h3>
                <p class="text-sm text-muted-foreground">{{ selectedWeek.startDate | date:'shortDate' }} - {{ selectedWeek.endDate | date:'shortDate' }}</p>
              </div>
              <div class="flex items-center gap-6 text-sm">
                <div class="flex items-center gap-1">
                  <span class="text-muted-foreground">📊</span>
                  <span class="font-medium text-foreground">{{ playerScores.length }}</span>
                  <span class="text-muted-foreground">players</span>
                </div>
                <div class="flex items-center gap-1">
                  <span class="text-muted-foreground">⚡</span>
                  <span class="font-medium text-foreground">{{ getChangedScoresCount() }}</span>
                  <span class="text-muted-foreground">unsaved</span>
                </div>
                <div class="flex items-center gap-1">
                  <span class="text-muted-foreground">✅</span>
                  <span class="font-medium text-foreground">{{ existingScores.length }}</span>
                  <span class="text-muted-foreground">saved</span>
                </div>
              </div>
            </div>
            
            <!-- Quick Actions -->
            <div class="flex flex-col sm:flex-row gap-2 flex-shrink-0">
              <button 
                (click)="saveAllScores()"
                [disabled]="!hasUnsavedChanges() || isLoading"
                class="bg-primary text-primary-foreground px-3 py-2 rounded-md text-sm font-medium transition-colors hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2">
                <span *ngIf="isLoading" class="animate-spin">⟳</span>
                💾 Save All
              </button>
              
              <button 
                (click)="clearAllScores()"
                [disabled]="playerScores.length === 0"
                class="bg-destructive text-destructive-foreground px-3 py-2 rounded-md text-sm font-medium transition-colors hover:bg-destructive/90 disabled:opacity-50 disabled:cursor-not-allowed">
                🗑️ Clear All
              </button>
            </div>
          </div>
        </div>

        <!-- Score Entry Table -->
        <div class="bg-card border border-border rounded-lg shadow-sm overflow-hidden flex flex-col flex-1 min-h-0">
          <div class="px-6 py-4 border-b border-border bg-muted/20 flex-shrink-0">
            <h2 class="text-xl font-semibold text-foreground">Player Scores</h2>
          </div>
          
          <div *ngIf="players.length === 0" class="p-8 text-center flex-1 flex items-center justify-center">
            <div>
              <div class="text-6xl mb-4">⛳</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">No players assigned to flights</h3>
              <p class="text-muted-foreground">Players must be assigned to flights before scores can be entered.</p>
              <p class="text-sm text-muted-foreground mt-2">Go to Season Management to assign players to flights.</p>
            </div>
          </div>

          <div *ngIf="players.length > 0" class="flex-1 min-h-0 overflow-hidden">
            <div class="h-full overflow-y-auto max-h-[60vh]">
              <table class="w-full border-collapse">
                <thead class="bg-muted/50 text-xs uppercase text-muted-foreground sticky top-0 z-10">
                  <tr>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Player</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Flight</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Score</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Points</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Status</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let player of players; trackBy: trackByPlayerId" 
                      [class]="'hover:bg-muted/20 transition-colors ' + (hasPlayerScoreChanged(player.id) ? 'bg-primary/5' : '')">
                    <td class="p-4 border-b border-border">
                      <div class="font-semibold text-foreground">{{ player.firstName }} {{ player.lastName }}</div>
                      <div class="text-sm text-muted-foreground">{{ player.email }}</div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="text-sm text-foreground">{{ player.flightName || 'No Flight' }}</div>
                      <div class="text-xs text-muted-foreground" *ngIf="player.handicapAtAssignment">
                        Handicap: {{ player.handicapAtAssignment }}
                      </div>
                      <div class="text-xs text-primary" *ngIf="player.isFlightLeader">
                        ⭐ Flight Leader
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <input 
                        type="number"
                        [(ngModel)]="playerScores[getPlayerScoreIndex(player.id)].score"
                        (ngModelChange)="onScoreChange(player.id, $event)"
                        [placeholder]="'Enter score'"
                        min="50"
                        max="150"
                        class="w-20 px-3 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="flex items-center space-x-2">
                        <span class="font-semibold text-lg text-foreground">
                          {{ getPlayerScore(player.id)?.pointsEarned || 0 }}
                        </span>
                        <span *ngIf="getPlayerScore(player.id)?.score" class="text-sm text-muted-foreground">
                          pts
                        </span>
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <span [class]="'px-2 py-1 text-xs rounded-full ' + getPlayerStatusClass(player.id)">
                        {{ getPlayerStatus(player.id) }}
                      </span>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="flex space-x-2">
                        <button 
                          *ngIf="hasPlayerScoreChanged(player.id)"
                          (click)="savePlayerScore(player.id)"
                          [disabled]="isLoading"
                          class="text-primary hover:text-primary/80 text-sm underline disabled:text-muted-foreground">
                          Save
                        </button>
                        <button 
                          *ngIf="getPlayerScore(player.id)?.score"
                          (click)="clearPlayerScore(player.id)"
                          class="text-destructive hover:text-destructive/80 text-sm underline">
                          Clear
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- Leaderboard Preview -->
        <div *ngIf="existingScores.length > 0" class="bg-card border border-border rounded-lg shadow-sm mt-6 flex-shrink-0">
          <div class="px-6 py-4 border-b border-border bg-muted/20">
            <h2 class="text-xl font-semibold text-foreground">Current Leaderboard</h2>
          </div>
          <div class="p-6">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              <div *ngFor="let score of getLeaderboard().slice(0, 6); let i = index" 
                   class="flex items-center justify-between p-3 border border-border rounded-lg bg-background">
                <div class="flex items-center space-x-3">
                  <span 
                    class="flex items-center justify-center w-8 h-8 rounded-full text-white text-sm font-bold"
                    [style.background-color]="getRankColor(i + 1)">
                    {{ i + 1 }}
                  </span>
                  <div>
                    <div class="font-semibold text-foreground">{{ score.player?.firstName }} {{ score.player?.lastName }}</div>
                    <div class="text-sm text-muted-foreground">{{ score.pointsEarned }} points</div>
                  </div>
                </div>
                <div class="text-right">
                  <div class="font-bold text-lg text-foreground">{{ score.score }}</div>
                  <div class="text-sm text-muted-foreground">strokes</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Selection Message -->
      <div *ngIf="!selectedWeekId" class="text-center mt-8 flex-1 flex items-center justify-center">
        <div>
          <div class="text-6xl mb-4">📝</div>
          <h3 class="text-xl font-semibold text-foreground mb-2">Select a Week</h3>
          <p class="text-muted-foreground">Choose a season and week from the dropdowns above to enter scores.</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    /* Additional custom styles if needed */
  `]
})
export class ScoreEntryComponent implements OnInit {
  seasons: Season[] = [];
  weeks: Week[] = [];
  matchups: Matchup[] = [];
  selectedSeasonId: string = '';
  selectedWeekId: string = '';
  selectedWeek: Week | null = null;
  existingScores: ScoreEntry[] = [];
  isLoading: boolean = false;

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
        this.selectedWeekId = params['weekId'];
        this.loadWeekById(params['weekId']);
      }
    });
  }

  loadSeasons() {
    this.scoringService.getSeasons().subscribe({
      next: (seasons) => {
        this.seasons = seasons;
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
      // Clear current week selection when changing seasons
      this.selectedWeekId = '';
      this.selectedWeek = null;
      
      this.loadWeeks();
      this.loadPlayers();
      this.clearScoreData();
    }
  }

  loadWeeks() {
    this.scoringService.getWeeksBySeason(this.selectedSeasonId).subscribe({
      next: (weeks) => {
        this.weeks = weeks.sort((a, b) => a.weekNumber - b.weekNumber);
        
        // Auto-select current week if no week is selected and weeks are available
        if (weeks.length > 0 && !this.selectedWeekId) {
          this.loadCurrentWeek();
        }
      },
      error: (error) => console.error('Error loading weeks:', error)
    });
  }

  loadCurrentWeek() {
    if (!this.selectedSeasonId) return;
    
    this.scoringService.getCurrentWeek(this.selectedSeasonId).subscribe({
      next: (currentWeek) => {
        // Check if the current week exists in our loaded weeks
        const weekExists = this.weeks.find(w => w.id === currentWeek.id);
        if (weekExists) {
          this.selectedWeekId = currentWeek.id;
          this.selectedWeek = currentWeek;
          this.loadExistingScores();
          this.initializePlayerScores();
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
      this.loadExistingScores();
      this.initializePlayerScores();
    }
  }

  loadWeekById(weekId: string) {
    this.scoringService.getWeekById(weekId).subscribe({
      next: (week) => {
        this.selectedWeek = week;
        this.selectedSeasonId = week.seasonId;
        this.selectedWeekId = week.id;
        this.loadSeasons();
        this.loadWeeks();
        this.loadPlayers();
        this.loadExistingScores();
      },
      error: (error) => console.error('Error loading week:', error)
    });
  }

  onWeekChange() {
    if (this.selectedWeekId) {
      this.selectedWeek = this.weeks.find(w => w.id === this.selectedWeekId) || null;
      this.loadExistingScores();
      this.initializePlayerScores();
    } else {
      this.clearScoreData();
    }
  }

  loadPlayers() {
    if (!this.selectedSeasonId) return;
    
    this.scoringService.getPlayersInFlights(this.selectedSeasonId).subscribe({
      next: (players) => {
        this.players = players;
        this.initializePlayerScores();
      },
      error: (error) => console.error('Error loading players in flights:', error)
    });
  }

  loadExistingScores() {
    if (!this.selectedWeekId) return;
    
    this.scoringService.getScoresByWeek(this.selectedWeekId).subscribe({
      next: (scores) => {
        this.existingScores = scores;
        this.initializePlayerScores();
      },
      error: (error) => console.error('Error loading scores:', error)
    });
  }

  initializePlayerScores() {
    if (!this.players.length) return;

    this.playerScores = this.players.map(player => {
      const existingScore = this.existingScores.find(s => s.playerId === player.id);
      const score = existingScore?.score || null;
      
      if (score !== null) {
        this.originalScores.set(player.id, score);
      }

      return {
        playerId: player.id,
        weekId: this.selectedWeekId,
        score: score,
        pointsEarned: existingScore?.pointsEarned || 0
      };
    });
  }

  onScoreChange(playerId: string, score: number) {
    const playerScore = this.getPlayerScore(playerId);
    if (playerScore) {
      playerScore.score = score;
      
      // Calculate points automatically (this is a simple calculation)
      // Points = total players - ranking + 1 (will be calculated properly on backend)
      const allScores = this.playerScores
        .filter(ps => ps.score !== null && ps.score !== undefined)
        .sort((a, b) => a.score! - b.score!);
      
      const position = allScores.findIndex(ps => ps.playerId === playerId) + 1;
      if (position > 0 && playerScore.pointsEarned !== undefined) {
        playerScore.pointsEarned = allScores.length - position + 1;
      }
    }
  }

  savePlayerScore(playerId: string) {
    const playerScore = this.getPlayerScore(playerId);
    if (!playerScore || !playerScore.score) return;

    this.isLoading = true;
    
    const scoreEntry = {
      playerId: playerId,
      weekId: this.selectedWeekId,
      score: playerScore.score
    };

    this.scoringService.enterScore(scoreEntry).subscribe({
      next: () => {
        this.originalScores.set(playerId, playerScore.score!);
        this.loadExistingScores();
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error saving score:', error);
        this.isLoading = false;
      }
    });
  }

  saveAllScores() {
    const scoresToSave = this.playerScores
      .filter(ps => ps.score !== null && ps.score !== undefined && this.hasPlayerScoreChanged(ps.playerId))
      .map(ps => ({
        playerId: ps.playerId,
        weekId: this.selectedWeekId,
        score: ps.score!
      }));

    if (scoresToSave.length === 0) return;

    this.isLoading = true;

    this.scoringService.enterBulkScores(scoresToSave).subscribe({
      next: () => {
        // Update original scores
        scoresToSave.forEach(score => {
          this.originalScores.set(score.playerId, score.score);
        });
        this.loadExistingScores();
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error saving scores:', error);
        this.isLoading = false;
      }
    });
  }

  clearPlayerScore(playerId: string) {
    const playerScore = this.getPlayerScore(playerId);
    if (playerScore) {
      playerScore.score = null;
      if (playerScore.pointsEarned !== undefined) {
        playerScore.pointsEarned = 0;
      }
    }
  }

  clearAllScores() {
    if (confirm('Are you sure you want to clear all unsaved scores?')) {
      this.playerScores.forEach(ps => {
        if (!this.originalScores.has(ps.playerId)) {
          ps.score = null;
          if (ps.pointsEarned !== undefined) {
            ps.pointsEarned = 0;
          }
        } else {
          ps.score = this.originalScores.get(ps.playerId)!;
        }
      });
    }
  }

  clearScoreData() {
    this.selectedWeek = null;
    this.existingScores = [];
    this.playerScores = [];
    this.originalScores.clear();
  }

  getPlayerScore(playerId: string): BulkScoreEntry | undefined {
    return this.playerScores.find(ps => ps.playerId === playerId);
  }

  getPlayerScoreIndex(playerId: string): number {
    return this.playerScores.findIndex(ps => ps.playerId === playerId);
  }

  hasPlayerScoreChanged(playerId: string): boolean {
    const currentScore = this.getPlayerScore(playerId)?.score;
    const originalScore = this.originalScores.get(playerId);
    
    if (currentScore === null || currentScore === undefined) {
      return originalScore !== undefined;
    }
    
    return currentScore !== originalScore;
  }

  hasUnsavedChanges(): boolean {
    return this.playerScores.some(ps => this.hasPlayerScoreChanged(ps.playerId));
  }

  getChangedScoresCount(): number {
    return this.playerScores.filter(ps => this.hasPlayerScoreChanged(ps.playerId)).length;
  }

  getPlayerStatus(playerId: string): string {
    const playerScore = this.getPlayerScore(playerId);
    const hasScore = playerScore?.score !== null && playerScore?.score !== undefined;
    const hasChanged = this.hasPlayerScoreChanged(playerId);
    
    if (!hasScore) return 'No Score';
    if (hasChanged) return 'Unsaved';
    return 'Saved';
  }

  getPlayerStatusClass(playerId: string): string {
    const status = this.getPlayerStatus(playerId);
    switch (status) {
      case 'Saved': return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200 border border-green-200 dark:border-green-700';
      case 'Unsaved': return 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200 border border-orange-200 dark:border-orange-700';
      case 'No Score': return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400 border border-gray-200 dark:border-gray-600';
      default: return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400 border border-gray-200 dark:border-gray-600';
    }
  }

  getLeaderboard(): ScoreEntry[] {
    return this.existingScores
      .filter(score => score.score !== null)
      .sort((a, b) => b.pointsEarned - a.pointsEarned);
  }

  getRankColor(rank: number): string {
    switch (rank) {
      case 1: return '#FFD700'; // Gold
      case 2: return '#C0C0C0'; // Silver
      case 3: return '#CD7F32'; // Bronze
      default: return '#6366f1'; // Indigo
    }
  }

  getWeekDisplayName(week: Week): string {
    const startDate = new Date(week.startDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    const endDate = new Date(week.endDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    return `${week.name} (${startDate} - ${endDate})`;
  }

  trackByPlayerId(index: number, player: Player): string {
    return player.id;
  }
}
