import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatchupService, Matchup } from '../../services/matchup.service';
import { ScoringService } from '../../../scoring/services/scoring.service';
import { PlayerFlightAssignmentService } from '../../services/player-flight-assignment.service';
import { Season, Week, PlayerWithFlight } from '../../../scoring/models/week.model';

@Component({
  selector: 'app-scheduling-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="h-full flex flex-col">
      <!-- Header -->
      <div class="mb-8 flex-shrink-0">
        <h2 class="text-3xl font-semibold text-foreground mb-2">Scheduling Management</h2>
        <p class="text-muted-foreground">Create and manage matchups between players in the same flights</p>
      </div>

      <!-- Error Message -->
      <div class="bg-destructive/10 border border-destructive/20 text-destructive p-4 rounded-md mb-6 flex justify-between items-center flex-shrink-0" *ngIf="error">
        {{ error }}
        <button class="bg-muted text-muted-foreground px-3 py-1 rounded text-sm hover:bg-muted/80" (click)="error = null">Dismiss</button>
      </div>

      <!-- Season and Week Selection -->
      <div class="bg-card border border-border rounded-lg shadow-sm p-6 mb-6 flex-shrink-0">
        <div class="flex flex-col lg:flex-row lg:items-center gap-4">
          <div class="flex-1">
            <label for="season-select" class="block text-sm font-medium text-foreground mb-2">Season</label>
            <select 
              id="season-select"
              [(ngModel)]="selectedSeasonId"
              (change)="onSeasonChange()"
              class="w-full px-4 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
              <option value="">Select Season</option>
              <option *ngFor="let season of seasons" [value]="season.id">{{ season.name }}</option>
            </select>
          </div>
          
          <div class="flex-1">
            <label for="week-select" class="block text-sm font-medium text-foreground mb-2">Week</label>
            <select 
              id="week-select"
              [(ngModel)]="selectedWeekId"
              (change)="onWeekChange()"
              [disabled]="!selectedSeasonId || weeks.length === 0"
              class="w-full px-4 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20 disabled:bg-muted disabled:cursor-not-allowed">
              <option value="">Select Week</option>
              <option *ngFor="let week of weeks" [value]="week.id">{{ getWeekDisplayName(week) }}</option>
            </select>
          </div>
          
          <!-- Week Stats -->
          <div *ngIf="selectedWeekId" class="text-xs text-muted-foreground bg-muted/20 rounded-md p-2">
            <div class="flex items-center space-x-3">
              <span>👥 {{ availablePlayers.length }} total</span>
              <span>⚡ {{ matchups.length }} matchups</span>
              <span class="font-medium text-foreground">🆓 {{ getAvailablePlayersForWeek().length }} available</span>
            </div>
          </div>

          <div class="flex items-end gap-2">
            <button 
              *ngIf="selectedWeekId && selectedSeasonId"
              (click)="generateMatchupsForWeek()"
              [disabled]="isLoading"
              class="bg-primary text-primary-foreground px-4 py-2 rounded-md text-sm font-medium transition-colors hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed"
              title="Randomly pair players in each flight (one matchup per player)">
              <span *ngIf="isLoading" class="animate-spin mr-2">⟳</span>
              Auto-Generate Pairings
            </button>
          </div>
        </div>
      </div>

      <!-- Matchups Section -->
      <div *ngIf="selectedWeekId" class="flex-1 flex flex-col min-h-0">
        <div class="bg-card border border-border rounded-lg shadow-sm overflow-hidden flex flex-col flex-1">
          <div class="px-6 py-4 border-b border-border bg-muted/20 flex-shrink-0">
            <div class="flex justify-between items-center">
              <h3 class="text-lg font-semibold text-foreground">
                Matchups for {{ getSelectedWeekName() }}
              </h3>
              <button 
                (click)="showCreateMatchupForm = !showCreateMatchupForm"
                class="bg-primary text-primary-foreground px-3 py-1.5 rounded text-sm font-medium transition-colors hover:bg-primary/90">
                {{ showCreateMatchupForm ? 'Cancel' : 'Add Matchup' }}
              </button>
            </div>
          </div>

          <!-- Create Matchup Form -->
          <div *ngIf="showCreateMatchupForm" class="p-6 border-b border-border bg-background flex-shrink-0">
            <h4 class="text-md font-medium text-foreground mb-2">Create New Matchup</h4>
            <p class="text-sm text-muted-foreground mb-4">Each player can only have one matchup per week. Players already matched will not appear in the dropdown.</p>
            
            <div *ngIf="getEligiblePlayersForPlayerA().length === 0" class="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-md p-3 mb-4">
              <p class="text-sm text-yellow-800 dark:text-yellow-200">
                ⚠️ All players already have matchups for this week. Delete existing matchups to create new ones.
              </p>
            </div>

            <form [formGroup]="matchupForm" (ngSubmit)="createMatchup()" class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label for="playerA" class="block text-sm font-medium text-foreground mb-1">Player A</label>
                <select 
                  id="playerA"
                  formControlName="playerAId" 
                  class="w-full p-2 border border-border rounded text-sm"
                  [disabled]="getEligiblePlayersForPlayerA().length === 0">
                  <option value="">Select Player A</option>
                  <option *ngFor="let player of getEligiblePlayersForPlayerA()" [value]="player.id">
                    {{ player.firstName }} {{ player.lastName }} ({{ player.flightName || 'Unknown Flight' }})
                  </option>
                </select>
              </div>
              
              <div>
                <label for="playerB" class="block text-sm font-medium text-foreground mb-1">Player B</label>
                <select 
                  id="playerB"
                  formControlName="playerBId" 
                  class="w-full p-2 border border-border rounded text-sm">
                  <option value="">Select Player B</option>
                  <option *ngFor="let player of getEligiblePlayersForPlayerB()" [value]="player.id">
                    {{ player.firstName }} {{ player.lastName }} ({{ player.flightName || 'Unknown Flight' }})
                  </option>
                </select>
              </div>
              
              <div class="flex items-end">
                <button 
                  type="submit" 
                  [disabled]="matchupForm.invalid || isLoading"
                  class="w-full bg-primary text-primary-foreground px-4 py-2 rounded text-sm font-medium transition-colors hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed">
                  Create Matchup
                </button>
              </div>
            </form>
          </div>

          <!-- Empty State -->
          <div *ngIf="matchups.length === 0 && !isLoading" class="p-8 text-center flex-1 flex items-center justify-center">
            <div>
              <div class="text-6xl mb-4">⚡</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">No matchups scheduled</h3>
              <p class="text-muted-foreground mb-4">Create matchups between players in the same flight for this week.</p>
              <button 
                (click)="showCreateMatchupForm = true"
                class="bg-primary text-primary-foreground px-4 py-2 rounded text-sm font-medium transition-colors hover:bg-primary/90">
                Add First Matchup
              </button>
            </div>
          </div>

          <!-- Matchups List -->
          <div *ngIf="matchups.length > 0" class="flex-1 min-h-0 overflow-hidden">
            <div class="h-full overflow-y-auto max-h-[60vh]">
              <div class="p-6 space-y-4">
                <div *ngFor="let matchup of matchups; trackBy: trackByMatchupId" 
                     class="bg-background border border-border rounded-lg p-4 hover:bg-muted/20 transition-colors">
                  <div class="flex justify-between items-start">
                    <div class="flex-1">
                      <div class="flex items-center space-x-4 mb-2">
                        <div class="flex items-center space-x-2">
                          <span class="font-semibold text-foreground">{{ getPlayerName(matchup.playerAId) }}</span>
                          <span class="text-muted-foreground">vs</span>
                          <span class="font-semibold text-foreground">{{ getPlayerName(matchup.playerBId) }}</span>
                        </div>
                        <span class="text-xs px-2 py-1 bg-primary/10 text-primary border border-primary/20 rounded-full">
                          {{ getPlayerFlight(matchup.playerAId) }}
                        </span>
                      </div>
                      
                      <div class="text-sm text-muted-foreground">
                        <div *ngIf="matchup.playerAScore !== null && matchup.playerBScore !== null">
                          Score: {{ matchup.playerAScore }} - {{ matchup.playerBScore }} | 
                          Points: {{ matchup.playerAPoints || 0 }} - {{ matchup.playerBPoints || 0 }}
                        </div>
                        <div *ngIf="matchup.playerAScore === null || matchup.playerBScore === null">
                          Scheduled - No scores entered yet
                        </div>
                      </div>
                    </div>
                    
                    <div class="flex items-center space-x-2">
                      <button 
                        (click)="deleteMatchup(matchup.id!)"
                        [disabled]="isLoading"
                        class="text-destructive hover:text-destructive/80 text-sm underline disabled:text-muted-foreground">
                        Delete
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    /* Additional custom styles if needed */
  `]
})
export class SchedulingSettingsComponent implements OnInit {
  // Form
  matchupForm: FormGroup;
  
  // Data
  seasons: Season[] = [];
  weeks: Week[] = [];
  matchups: Matchup[] = [];
  availablePlayers: PlayerWithFlight[] = [];
  
  // Selected data
  selectedSeasonId: string = '';
  selectedWeekId: string = '';
  
  // UI state
  showCreateMatchupForm = false;
  isLoading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private matchupService: MatchupService,
    private scoringService: ScoringService,
    private playerFlightAssignmentService: PlayerFlightAssignmentService
  ) {
    this.matchupForm = this.fb.group({
      playerAId: ['', Validators.required],
      playerBId: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.loadSeasons();
  }

  loadSeasons() {
    this.isLoading = true;
    this.error = null;
    
    this.scoringService.getSeasons().subscribe({
      next: (seasons) => {
        this.seasons = seasons;
        // Auto-select the first season if available
        if (seasons.length > 0 && !this.selectedSeasonId) {
          this.selectedSeasonId = seasons[0].id!;
          this.onSeasonChange();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading seasons:', error);
        this.error = 'Failed to load seasons. Please try again.';
        this.isLoading = false;
      }
    });
  }

  onSeasonChange() {
    if (this.selectedSeasonId) {
      this.loadWeeks();
      this.loadAvailablePlayers();
      this.selectedWeekId = '';
      this.matchups = [];
    }
  }

  onWeekChange() {
    if (this.selectedWeekId) {
      this.loadMatchupsForWeek();
    } else {
      this.matchups = [];
    }
  }

  loadWeeks() {
    if (!this.selectedSeasonId) return;
    
    this.isLoading = true;
    this.error = null;
    
    this.scoringService.getWeeksBySeason(this.selectedSeasonId).subscribe({
      next: (weeks) => {
        this.weeks = weeks;
        this.isLoading = false;
        // Auto-select current week if not already selected
        if (weeks.length > 0 && !this.selectedWeekId) {
          this.selectCurrentWeek();
        }
      },
      error: (error) => {
        console.error('Error loading weeks:', error);
        this.error = 'Failed to load weeks. Please try again.';
        this.isLoading = false;
      }
    });
  }

  loadAvailablePlayers() {
    if (!this.selectedSeasonId) return;
    
    this.isLoading = true;
    this.error = null;
    
    this.scoringService.getPlayersInFlights(this.selectedSeasonId).subscribe({
      next: (players: PlayerWithFlight[]) => {
        this.availablePlayers = players;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading players:', error);
        this.error = 'Failed to load players. Please try again.';
        this.isLoading = false;
      }
    });
  }

  loadMatchupsForWeek() {
    if (!this.selectedWeekId) return;
    
    this.isLoading = true;
    this.error = null;
    
    this.matchupService.getMatchupsByWeek(this.selectedWeekId).subscribe({
      next: (matchups) => {
        this.matchups = matchups;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading matchups:', error);
        this.error = 'Failed to load matchups. Please try again.';
        this.isLoading = false;
      }
    });
  }

  createMatchup() {
    if (this.matchupForm.valid) {
      const formValue = this.matchupForm.value;
      const matchup: Partial<Matchup> = {
        weekId: this.selectedWeekId,
        playerAId: formValue.playerAId,
        playerBId: formValue.playerBId
      };
      
      this.isLoading = true;
      this.error = null;
      
      this.matchupService.createMatchup(matchup).subscribe({
        next: (createdMatchup: Matchup) => {
          this.matchups.push(createdMatchup);
          this.matchupForm.reset();
          this.showCreateMatchupForm = false;
          this.isLoading = false;
          // Form will automatically update available players due to getEligiblePlayersForPlayerA/B methods
        },
        error: (error: any) => {
          console.error('Error creating matchup:', error);
          this.error = error.error?.message || 'Failed to create matchup. Please try again.';
          this.isLoading = false;
        }
      });
    }
  }

  generateMatchupsForWeek() {
    if (!this.selectedWeekId || !this.selectedSeasonId) return;
    
    this.isLoading = true;
    this.error = null;
    
    this.matchupService.generateRoundRobinMatchups(this.selectedWeekId, this.selectedSeasonId).subscribe({
      next: (generatedMatchups: Matchup[]) => {
        this.matchups = generatedMatchups;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error generating matchups:', error);
        this.error = error.error?.message || 'Failed to generate matchups. Please try again.';
        this.isLoading = false;
      }
    });
  }

  deleteMatchup(matchupId: string) {
    if (confirm('Are you sure you want to delete this matchup?')) {
      this.isLoading = true;
      this.error = null;
      
      this.matchupService.deleteMatchup(matchupId).subscribe({
        next: () => {
          this.matchups = this.matchups.filter(m => m.id !== matchupId);
          this.isLoading = false;
          // Form will automatically update available players due to getEligiblePlayersForPlayerA/B methods
        },
        error: (error: any) => {
          console.error('Error deleting matchup:', error);
          this.error = error.error?.message || 'Failed to delete matchup. Please try again.';
          this.isLoading = false;
        }
      });
    }
  }

  selectCurrentWeek() {
    if (!this.selectedSeasonId || this.weeks.length === 0) return;
    
    this.scoringService.getCurrentWeek(this.selectedSeasonId).subscribe({
      next: (currentWeek) => {
        // Check if the current week exists in our weeks list
        const weekExists = this.weeks.find(w => w.id === currentWeek.id);
        if (weekExists) {
          this.selectedWeekId = currentWeek.id;
          this.onWeekChange();
        } else {
          // If current week doesn't exist, fall back to the first week
          this.selectFirstWeek();
        }
      },
      error: (error) => {
        console.error('Error loading current week:', error);
        // Fall back to selecting the first week if current week API fails
        this.selectFirstWeek();
      }
    });
  }

  selectFirstWeek() {
    if (this.weeks.length > 0) {
      // Select the first week (sorted by week number)
      const sortedWeeks = this.weeks.sort((a, b) => a.weekNumber - b.weekNumber);
      this.selectedWeekId = sortedWeeks[0].id;
      this.onWeekChange();
    }
  }

  getSelectedWeekName(): string {
    const week = this.weeks.find(w => w.id === this.selectedWeekId);
    return week ? week.name : '';
  }

  getEligiblePlayersForPlayerB(): any[] {
    const playerAId = this.matchupForm.get('playerAId')?.value;
    if (!playerAId) return this.getAvailablePlayersForWeek();
    
    // Get the flight of Player A
    const playerA = this.availablePlayers.find(p => p.id === playerAId);
    if (!playerA) return [];
    
    // Return only players in the same flight, excluding Player A, and excluding players already matched
    const availableForWeek = this.getAvailablePlayersForWeek();
    return availableForWeek.filter(p => 
      p.id !== playerAId && p.flightId === playerA.flightId
    );
  }

  // Get players who don't already have a matchup for the selected week
  getAvailablePlayersForWeek(): any[] {
    if (!this.selectedWeekId || !this.matchups.length) {
      return this.availablePlayers;
    }

    // Get list of player IDs who already have matchups this week
    const playersWithMatchups = new Set<string>();
    this.matchups.forEach(matchup => {
      playersWithMatchups.add(matchup.playerAId);
      playersWithMatchups.add(matchup.playerBId);
    });

    // Filter out players who already have matchups
    return this.availablePlayers.filter(player => 
      !playersWithMatchups.has(player.id)
    );
  }

  // Get players eligible for Player A (those without existing matchups)
  getEligiblePlayersForPlayerA(): any[] {
    return this.getAvailablePlayersForWeek();
  }

  getPlayerName(playerId: string): string {
    const player = this.availablePlayers.find(p => p.id === playerId);
    return player ? `${player.firstName} ${player.lastName}` : 'Unknown Player';
  }

  getPlayerFlight(playerId: string): string {
    const player = this.availablePlayers.find(p => p.id === playerId);
    return player ? player.flightName || 'Unknown Flight' : 'Unknown Flight';
  }

  getWeekDisplayName(week: Week): string {
    const startDate = new Date(week.startDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    const endDate = new Date(week.endDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    return `${week.name} (${startDate} - ${endDate})`;
  }

  trackByMatchupId(index: number, matchup: Matchup): string {
    return matchup.id || index.toString();
  }
}
