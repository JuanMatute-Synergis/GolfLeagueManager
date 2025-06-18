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
              <label class="block text-sm font-medium text-foreground mb-2">Date (Wednesday)</label>
              <input 
                type="date"
                [(ngModel)]="weekForm_date"
                name="date"
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
        <!-- Holiday Week Management Section -->
        <div class="bg-card border border-border rounded-lg shadow-sm mb-4 p-4">
          <h3 class="text-lg font-semibold text-foreground mb-2">Holiday Week Management</h3>
          <p class="text-sm text-muted-foreground mb-3">
            Easily identify and remove weeks that fall on or near holidays. Week numbers will automatically renumber after deletion.
          </p>
          
          <div *ngIf="getHolidayWeeks().length > 0" class="space-y-2">
            <h4 class="font-medium text-foreground">Potential Holiday Weeks:</h4>
            <div class="flex flex-wrap gap-2">
              <div *ngFor="let holidayWeek of getHolidayWeeks()" 
                   class="flex items-center space-x-2 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-md px-3 py-2">
                <span class="text-sm">
                  <span class="text-foreground">Week {{ holidayWeek.week.weekNumber }}</span> <span class="text-foreground"> - {{ holidayWeek.week.date | date:'MMM d' }}</span>
                  <span class="text-yellow-700 dark:text-yellow-300 ml-1">({{ holidayWeek.reason }})</span>
                </span>
                <button 
                  (click)="deleteWeek(holidayWeek.week)"
                  [disabled]="(holidayWeek.week.scoreEntries?.length || 0) > 0"
                  class="text-red-600 hover:text-red-800 disabled:text-gray-400 disabled:cursor-not-allowed text-xs"
                  [title]="(holidayWeek.week.scoreEntries?.length || 0) > 0 ? 'Cannot delete week with scores' : 'Delete holiday week'">
                  Remove
                </button>
              </div>
            </div>
          </div>
          
          <div *ngIf="getHolidayWeeks().length === 0" class="text-sm text-muted-foreground">
            No potential holiday weeks detected in the current schedule.
          </div>
        </div>

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
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Date</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Status</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Scores</th>
                    <th class="text-left font-semibold text-foreground p-4 border-b border-border">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let week of weeks" 
                      [class]="'hover:bg-muted/20 transition-colors ' + (isHolidayWeek(week) ? 'bg-yellow-50 dark:bg-yellow-900/10 border-l-4 border-yellow-400' : '')">
                    <td class="p-4 border-b border-border text-foreground font-medium">
                      {{ week.weekNumber }}
                      <span *ngIf="isHolidayWeek(week)" class="ml-2 text-xs text-yellow-600 dark:text-yellow-400">🎄</span>
                    </td>
                    <td class="p-4 border-b border-border">
                      <div class="font-semibold text-foreground">{{ week.name }}</div>
                      <div *ngIf="isHolidayWeek(week)" class="text-xs text-yellow-600 dark:text-yellow-400 mt-1">
                        {{ getHolidayReason(week) }}
                      </div>
                    </td>
                    <td class="p-4 border-b border-border text-foreground">
                      <div>{{ week.date | date:'fullDate' }}</div>
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
  weekForm_date: string = '';

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
      date: this.weekForm_date,
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
    this.weekForm_date = this.formatDateForInput(week.date);
    this.showCreateForm = false;
  }

  deleteWeek(week: Week) {
    if ((week.scoreEntries?.length || 0) > 0) {
      alert('Cannot delete a week that has score entries.');
      return;
    }

    // Count how many weeks will be renumbered
    const subsequentWeeks = this.weeks.filter(w => w.weekNumber > week.weekNumber);
    const renumberMessage = subsequentWeeks.length > 0 
      ? `\n\nNote: ${subsequentWeeks.length} subsequent weeks will be automatically renumbered.`
      : '';

    const confirmMessage = `Are you sure you want to delete "${week.name}" (${new Date(week.date).toLocaleDateString()})?${renumberMessage}`;

    if (confirm(confirmMessage)) {
      this.scoringService.deleteWeek(week.id).subscribe({
        next: () => {
          this.loadWeeks();
          // Show success message with renumbering info
          if (subsequentWeeks.length > 0) {
            alert(`Week deleted successfully! ${subsequentWeeks.length} subsequent weeks have been renumbered.`);
          }
        },
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
    this.weekForm_date = '';
  }

  getWeekStatus(week: Week): string {
    const now = new Date();
    const weekDate = new Date(week.date);

    // Consider week active if it's the current Wednesday or within the same week
    const today = new Date();
    const currentWeekStart = new Date(today);
    currentWeekStart.setDate(today.getDate() - today.getDay()); // Start of current week (Sunday)
    const currentWeekEnd = new Date(currentWeekStart);
    currentWeekEnd.setDate(currentWeekStart.getDate() + 6); // End of current week (Saturday)

    if (weekDate > currentWeekEnd) return 'Upcoming';
    if (weekDate >= currentWeekStart && weekDate <= currentWeekEnd) return 'Active';
    return 'Completed';
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

  getHolidayWeeks(): { week: Week, reason: string }[] {
    const holidayWeeks: { week: Week, reason: string }[] = [];
    
    for (const week of this.weeks) {
      const weekDate = new Date(week.date);
      const month = weekDate.getMonth() + 1; // JavaScript months are 0-based
      const day = weekDate.getDate();
      const dayOfWeek = weekDate.getDay(); // 0 = Sunday, 1 = Monday, etc.
      
      // Memorial Day (last Monday in May)
      if (month === 5) {
        const lastMondayOfMay = this.getLastMondayOfMonth(2025, 5);
        if (this.isDateInWeek(weekDate, lastMondayOfMay)) {
          holidayWeeks.push({ week, reason: 'Memorial Day Week' });
          continue;
        }
      }
      
      // Independence Day (July 4th)
      if (month === 7 && (day >= 1 && day <= 10)) {
        holidayWeeks.push({ week, reason: 'Independence Day Week' });
        continue;
      }
      
      // Labor Day (first Monday in September)
      if (month === 9) {
        const firstMondayOfSeptember = this.getFirstMondayOfMonth(2025, 9);
        if (this.isDateInWeek(weekDate, firstMondayOfSeptember)) {
          holidayWeeks.push({ week, reason: 'Labor Day Week' });
          continue;
        }
      }
      
      // Thanksgiving week (fourth Thursday in November)
      if (month === 11) {
        const fourthThursdayOfNovember = this.getFourthThursdayOfNovember(2025);
        if (this.isDateInWeek(weekDate, fourthThursdayOfNovember)) {
          holidayWeeks.push({ week, reason: 'Thanksgiving Week' });
          continue;
        }
      }
    }
    
    return holidayWeeks;
  }

  private getLastMondayOfMonth(year: number, month: number): Date {
    const lastDay = new Date(year, month, 0); // Last day of the month
    const lastDayOfWeek = lastDay.getDay();
    const daysToSubtract = lastDayOfWeek === 1 ? 0 : (lastDayOfWeek === 0 ? 6 : lastDayOfWeek - 1);
    return new Date(year, month - 1, lastDay.getDate() - daysToSubtract);
  }

  private getFirstMondayOfMonth(year: number, month: number): Date {
    const firstDay = new Date(year, month - 1, 1);
    const firstDayOfWeek = firstDay.getDay();
    const daysToAdd = firstDayOfWeek === 1 ? 0 : (firstDayOfWeek === 0 ? 1 : 8 - firstDayOfWeek);
    return new Date(year, month - 1, 1 + daysToAdd);
  }

  private getFourthThursdayOfNovember(year: number): Date {
    const firstDay = new Date(year, 10, 1); // November 1st
    const firstDayOfWeek = firstDay.getDay();
    const daysToFirstThursday = firstDayOfWeek <= 4 ? 4 - firstDayOfWeek : 11 - firstDayOfWeek;
    return new Date(year, 10, 1 + daysToFirstThursday + 21); // Add 3 weeks to get fourth Thursday
  }

  private isDateInWeek(weekDate: Date, holidayDate: Date): boolean {
    const timeDiff = Math.abs(weekDate.getTime() - holidayDate.getTime());
    const dayDiff = Math.ceil(timeDiff / (1000 * 3600 * 24));
    return dayDiff <= 3; // Within 3 days of the holiday
  }

  isHolidayWeek(week: Week): boolean {
    return this.getHolidayWeeks().some(hw => hw.week.id === week.id);
  }

  getHolidayReason(week: Week): string {
    const holidayWeek = this.getHolidayWeeks().find(hw => hw.week.id === week.id);
    return holidayWeek ? holidayWeek.reason : '';
  }
}
