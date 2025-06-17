import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ScoringService } from '../../services/scoring.service';
import { Season, Week } from '../../models/week.model';

@Component({
  selector: 'app-week-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col">
      <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center mb-8 flex-shrink-0 gap-4">
        <div>
          <h1 class="text-3xl font-semibold text-foreground">Week Management</h1>
          <p class="mt-1 text-sm sm:text-base text-muted-foreground">Create and manage weeks for the golf season</p>
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

      <!-- Create/Edit Week Form -->
      <div *ngIf="showCreateForm || editingWeek" class="bg-card border border-border rounded-lg shadow-sm p-6 mb-6 flex-shrink-0">
        <h2 class="text-xl font-semibold text-foreground mb-4">{{ editingWeek ? 'Edit' : 'Create' }} Week</h2>
        <form (ngSubmit)="saveWeek()" #weekForm="ngForm">
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <div>
              <label class="block text-sm font-medium text-foreground mb-2">Week Number</label>
              <input 
                type="number"
                [(ngModel)]="weekForm_weekNumber"
                name="weekNumber"
                required
                min="1"
                class="w-full px-3 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
            </div>
            
            <div>
              <label class="block text-sm font-medium text-foreground mb-2">Week Name</label>
              <input 
                type="text"
                [(ngModel)]="weekForm_name"
                name="name"
                required
                placeholder="e.g., Week 1"
                class="w-full px-3 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
            </div>
            
            <div>
              <label class="block text-sm font-medium text-foreground mb-2">Start Date</label>
              <input 
                type="date"
                [(ngModel)]="weekForm_startDate"
                name="startDate"
                required
                class="w-full px-3 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
            </div>
            
            <div>
              <label class="block text-sm font-medium text-foreground mb-2">End Date</label>
              <input 
                type="date"
                [(ngModel)]="weekForm_endDate"
                name="endDate"
                required
                class="w-full px-3 py-2 border border-border rounded-md bg-background text-foreground focus:border-primary focus:ring-2 focus:ring-primary/20">
            </div>
          </div>
          
          <div class="flex justify-end space-x-3 mt-6">
            <button 
              type="button"
              (click)="cancelEdit()"
              class="bg-muted text-muted-foreground px-4 py-2 rounded-md font-medium transition-colors hover:bg-muted/80">
              Cancel
            </button>
            <button 
              type="submit"
              [disabled]="!weekForm.form.valid || isLoading"
              class="bg-primary text-primary-foreground px-4 py-2 rounded-md font-medium transition-colors hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2">
              <span *ngIf="isLoading" class="animate-spin">⟳</span>
              {{ editingWeek ? 'Update' : 'Create' }} Week
            </button>
          </div>
        </form>
      </div>

      <div *ngIf="selectedSeasonId" class="flex-1 flex flex-col min-h-0">
        <!-- Weeks List -->
        <div class="bg-card border border-border rounded-lg shadow-sm flex-1 flex flex-col min-h-0">
          <div class="px-6 py-4 border-b border-border bg-muted/20 flex-shrink-0">
            <h2 class="text-xl font-semibold text-foreground">Season Weeks ({{ weeks.length }})</h2>
          </div>
          
          <div *ngIf="weeks.length === 0" class="p-8 text-center flex-1 flex items-center justify-center">
            <div>
              <div class="text-6xl mb-4">📅</div>
              <h3 class="text-lg font-semibold text-foreground mb-2">No weeks available</h3>
              <p class="text-muted-foreground">Weeks are automatically generated when a season is created.</p>
            </div>
          </div>

          <div *ngIf="weeks.length > 0" class="flex-1 overflow-hidden">
            <div class="h-full overflow-y-auto">
              <table class="w-full border-collapse">
                <thead class="bg-muted/50 text-xs uppercase text-muted-foreground sticky top-0">
                  <tr>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Week</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Name</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Date Range</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Status</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Scores</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let week of weeks" class="hover:bg-muted/20 transition-colors">
                    <td class="p-4 border-b border-border text-foreground font-medium">{{ week.weekNumber }}</td>
                    <td class="p-4 border-b border-border">
                      <div class="font-semibold text-foreground">{{ week.name }}</div>
                    </td>
                    <td class="p-4 border-b border-border text-foreground">
                      <div>{{ week.startDate | date:'mediumDate' }}</div>
                      <div class="text-muted-foreground">{{ week.endDate | date:'mediumDate' }}</div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <span [class]="'px-2 py-1 text-xs rounded-full ' + getWeekStatusClass(week)">
                        {{ getWeekStatus(week) }}
                      </span>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="flex items-center space-x-2">
                        <span class="font-semibold text-foreground">{{ week.scoreEntries?.length || 0 }}</span>
                        <button 
                          *ngIf="getWeekStatus(week) === 'Active'"
                          (click)="navigateToScoreEntry(week)"
                          class="text-primary hover:text-primary/80 text-xs underline">
                          Enter Scores
                        </button>
                      </div>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="flex space-x-2">
                        <button 
                          (click)="editWeek(week)"
                          class="text-primary hover:text-primary/80 p-1"
                          title="Edit week">
                          ✏️
                        </button>
                        <button 
                          (click)="deleteWeek(week)"
                          [disabled]="(week.scoreEntries?.length || 0) > 0"
                          class="text-destructive hover:text-destructive/80 disabled:text-muted-foreground disabled:cursor-not-allowed p-1"
                          [title]="(week.scoreEntries?.length || 0) > 0 ? 'Cannot delete week with scores' : 'Delete week'">
                          🗑️
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

      <!-- Select Season Message -->
      <div *ngIf="!selectedSeasonId" class="text-center mt-8 flex-1 flex items-center justify-center">
        <div>
          <div class="text-6xl mb-4">📋</div>
          <h3 class="text-xl font-semibold text-foreground mb-2">Select a Season</h3>
          <p class="text-muted-foreground">Choose a season from the dropdown above to manage weeks.</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    /* Additional custom styles if needed */
  `]
})
export class WeekManagementComponent implements OnInit {
  seasons: Season[] = [];
  selectedSeasonId: string = '';
  weeks: Week[] = [];
  showCreateForm: boolean = false;
  editingWeek: Week | null = null;
  isLoading: boolean = false;

  // Form fields
  weekForm_weekNumber: number = 1;
  weekForm_name: string = '';
  weekForm_startDate: string = '';
  weekForm_endDate: string = '';

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
      this.loadWeeks();
      this.resetForm();
    }
  }

  loadWeeks() {
    this.scoringService.getWeeksBySeason(this.selectedSeasonId).subscribe({
      next: (weeks) => {
        this.weeks = weeks.sort((a, b) => a.weekNumber - b.weekNumber);
      },
      error: (error) => console.error('Error loading weeks:', error)
    });
  }

  saveWeek() {
    if (!this.selectedSeasonId) return;

    this.isLoading = true;
    
    const weekData: Week = {
      id: this.editingWeek?.id || '',
      seasonId: this.selectedSeasonId,
      weekNumber: this.weekForm_weekNumber,
      name: this.weekForm_name,
      startDate: this.weekForm_startDate,
      endDate: this.weekForm_endDate,
      isActive: true
    };

    const operation = this.editingWeek 
      ? this.scoringService.updateWeek(weekData.id, weekData)
      : this.scoringService.createWeek(weekData);

    operation.subscribe({
      next: () => {
        this.loadWeeks();
        this.resetForm();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error saving week:', error);
        this.isLoading = false;
      }
    });
  }

  editWeek(week: Week) {
    this.editingWeek = week;
    this.weekForm_weekNumber = week.weekNumber;
    this.weekForm_name = week.name;
    this.weekForm_startDate = week.startDate;
    this.weekForm_endDate = week.endDate;
    this.showCreateForm = false;
  }

  deleteWeek(week: Week) {
    if ((week.scoreEntries?.length || 0) > 0) {
      alert('Cannot delete a week that has score entries.');
      return;
    }

    if (confirm(`Are you sure you want to delete "${week.name}"?`)) {
      this.scoringService.deleteWeek(week.id).subscribe({
        next: () => this.loadWeeks(),
        error: (error) => console.error('Error deleting week:', error)
      });
    }
  }

  cancelEdit() {
    this.resetForm();
  }

  resetForm() {
    this.showCreateForm = false;
    this.editingWeek = null;
    this.weekForm_weekNumber = this.weeks.length + 1;
    this.weekForm_name = `Week ${this.weeks.length + 1}`;
    this.weekForm_startDate = '';
    this.weekForm_endDate = '';
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
      case 'Active': return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200 border border-green-200 dark:border-green-700';
      case 'Completed': return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400 border border-gray-200 dark:border-gray-600';
      case 'Upcoming': return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 border border-blue-200 dark:border-blue-700';
      default: return 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400 border border-gray-200 dark:border-gray-600';
    }
  }

  navigateToScoreEntry(week: Week) {
    this.router.navigate(['/scoring/score-entry'], { queryParams: { weekId: week.id } });
  }

  private formatDateForInput(date: string): string {
    return date.split('T')[0];
  }
}
