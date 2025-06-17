import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ScoringService } from '../../services/scoring.service';
import { MatchupService } from '../../../settings/services/matchup.service';
import { Season, Week, ScoreEntry, Player, PlayerWithFlight } from '../../models/week.model';
import { Matchup } from '../../../settings/services/matchup.service';

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
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col">
      <!-- Header -->
      <div class="mb-8 flex-shrink-0">
        <h2 class="text-3xl font-semibold text-foreground mb-2">Score Entry</h2>
        <p class="text-muted-foreground">Enter scores for each matchup in the selected week</p>
      </div>

      <!-- Season and Week Selection -->
      <div class="bg-card border border-border rounded-lg shadow-sm p-4 mb-6 flex-shrink-0">
        <div class="flex flex-col sm:flex-row gap-4">
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
        <!-- Week Info & Actions -->
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
                  <span class="text-muted-foreground">⚡</span>
                  <span class="font-medium text-foreground">{{ matchups.length }}</span>
                  <span class="text-muted-foreground">matchups</span>
                </div>
                <div class="flex items-center gap-1">
                  <span class="text-muted-foreground">✅</span>
                  <span class="font-medium text-foreground">{{ getCompletedMatchupsCount() }}</span>
                  <span class="text-muted-foreground">scored</span>
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
                [disabled]="matchups.length === 0"
                class="bg-destructive text-destructive-foreground px-3 py-2 rounded-md text-sm font-medium transition-colors hover:bg-destructive/90 disabled:opacity-50 disabled:cursor-not-allowed">
                🗑️ Clear All
              </button>
            </div>
          </div>
        </div>

        <!-- Matchups Table -->
        <div class="bg-card border border-border rounded-lg shadow-sm overflow-hidden flex flex-col flex-1 min-h-0">
          <div class="px-6 py-4 border-b border-border bg-muted/20 flex-shrink-0">
            <h2 class="text-xl font-semibold text-foreground">Matchup Scores</h2>
          </div>
          
          <div *ngIf="matchups.length === 0" class="p-8 text-center flex-1 flex items-center justify-center">
            <div>
              <div class="text-6xl mb-4">⛳</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">No matchups scheduled</h3>
              <p class="text-muted-foreground">No matchups have been created for this week yet.</p>
              <p class="text-sm text-muted-foreground mt-2">Go to Scheduling Management to create matchups.</p>
            </div>
          </div>

          <div *ngIf="matchups.length > 0" class="flex-1 min-h-0 overflow-hidden">
            <div class="h-full overflow-y-auto max-h-[60vh]">
              <table class="w-full border-collapse">
                <thead class="bg-muted/50 text-xs uppercase text-muted-foreground sticky top-0 z-10">
                  <tr>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Matchup</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Player A Score</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Player B Score</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Winner</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Status</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border bg-muted/50">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let matchup of matchups; trackBy: trackByMatchupId" 
                      [class]="'hover:bg-muted/20 transition-colors ' + (matchup.hasChanged ? 'bg-primary/5' : '')">
                    <td class="p-4 border-b border-border">
                      <div class="space-y-2">
                        <div class="font-semibold text-foreground">{{ matchup.playerAName }}</div>
                        <div class="text-sm text-muted-foreground">vs</div>
                        <div class="font-semibold text-foreground">{{ matchup.playerBName }}</div>
                      </div>
                      <div class="mt-2 text-xs text-muted-foreground">
                        Flight: {{ matchup.flightName }}
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="space-y-1">
                        <div class="text-sm font-medium text-foreground">{{ matchup.playerAName }}</div>
                        <input 
                          type="number"
                          [(ngModel)]="matchup.playerAScore"
                          (ngModelChange)="onScoreChange(matchup)"
                          [placeholder]="'Enter score'"
                          min="50"
                          max="150"
                          class="w-20 px-3 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="space-y-1">
                        <div class="text-sm font-medium text-foreground">{{ matchup.playerBName }}</div>
                        <input 
                          type="number"
                          [(ngModel)]="matchup.playerBScore"
                          (ngModelChange)="onScoreChange(matchup)"
                          [placeholder]="'Enter score'"
                          min="50"
                          max="150"
                          class="w-20 px-3 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div *ngIf="matchup.playerAScore && matchup.playerBScore" class="flex items-center space-x-2">
                        <span class="font-semibold text-lg text-foreground">
                          {{ getMatchupWinner(matchup) }}
                        </span>
                        <span class="text-sm text-muted-foreground">
                          wins
                        </span>
                      </div>
                      <div *ngIf="!matchup.playerAScore || !matchup.playerBScore" class="text-sm text-muted-foreground">
                        Pending scores
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <span [class]="'px-2 py-1 text-xs rounded-full ' + getMatchupStatusClass(matchup)">
                        {{ getMatchupStatus(matchup) }}
                      </span>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="flex space-x-2">
                        <button 
                          *ngIf="matchup.hasChanged"
                          (click)="saveMatchupScore(matchup)"
                          [disabled]="isLoading"
                          class="text-primary hover:text-primary/80 text-sm underline disabled:text-muted-foreground">
                          Save
                        </button>
                        <button 
                          *ngIf="matchup.playerAScore || matchup.playerBScore"
                          (click)="clearMatchupScores(matchup)"
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
      </div>

      <!-- Select Week Message -->
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
  matchups: MatchupWithDetails[] = [];
  players: PlayerWithFlight[] = [];
  selectedSeasonId: string = '';
  selectedWeekId: string = '';
  selectedWeek: Week | null = null;
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
      this.loadWeeks();
      this.loadPlayers();
      this.selectedWeekId = '';
      this.selectedWeek = null;
      this.matchups = [];
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
    
    return {
      ...matchup,
      playerAName: playerA ? `${playerA.firstName} ${playerA.lastName}` : 'Unknown Player',
      playerBName: playerB ? `${playerB.firstName} ${playerB.lastName}` : 'Unknown Player',
      flightName: playerA?.flightName || 'Unknown Flight',
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
}
