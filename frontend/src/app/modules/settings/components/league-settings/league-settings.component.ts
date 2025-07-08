import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { LeagueSettingsService } from '../../services/league-settings.service';
import { SeasonService, Season } from '../../services/season.service';
import { LeagueNameService } from '../../../../core/services/league-name.service';
import {
    LeagueSettings,
    HandicapCalculationMethod,
    AverageCalculationMethod,
    ScoringMethod,
    PointsSystem,
    EnumOption
} from '../../services/league-settings.service';

@Component({
    selector: 'app-league-settings',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, FormsModule],
    templateUrl: './league-settings.component.html',
    styleUrl: './league-settings.component.css'
})
export class LeagueSettingsComponent implements OnInit {
    settingsForm: FormGroup;
    selectedSeasonId: string = '';
    seasons: Season[] = [];
    isLoading = false;
    isSaving = false;
    error: string | null = null;
    successMessage: string | null = null;

    handicapMethods: EnumOption[] = [];
    averageMethods: EnumOption[] = [];
    scoringMethods: EnumOption[] = [];
    pointsSystems: EnumOption[] = [];

    constructor(
        private fb: FormBuilder,
        private leagueSettingsService: LeagueSettingsService,
        private seasonService: SeasonService,
        private leagueNameService: LeagueNameService,
        private cdr: ChangeDetectorRef
    ) {
        this.settingsForm = this.createForm();
    }

    ngOnInit(): void {
        this.loadEnumOptions();
        this.loadSeasons();
    }

    private loadSeasons(): void {
        this.seasonService.getSeasons().subscribe({
            next: (seasons) => {
                this.seasons = seasons;
            },
            error: (error) => {
                console.error('Error loading seasons:', error);
                this.error = 'Failed to load seasons. Please try again.';
            }
        });
    }

    onSeasonChange(): void {
        if (this.selectedSeasonId) {
            this.loadLeagueSettings();
        }
    }

    private createForm(): FormGroup {
        return this.fb.group({
            leagueName: ['Golf League', [Validators.required, Validators.maxLength(100)]],
            handicapMethod: [0, Validators.required],
            averageMethod: [0, Validators.required],
            legacyInitialWeight: [1, [Validators.required, Validators.min(1), Validators.max(10)]],
            coursePar: [36, [Validators.required, Validators.min(30), Validators.max(80)]],
            courseRating: [35.0, [Validators.required, Validators.min(25), Validators.max(80)]],
            slopeRating: [113, [Validators.required, Validators.min(55), Validators.max(155)]],
            maxRoundsForHandicap: [20, [Validators.required, Validators.min(5), Validators.max(30)]],
            scoringMethod: [0, Validators.required],
            pointsSystem: [0, Validators.required],
            holeWinPoints: [2, [Validators.required, Validators.min(0), Validators.max(5)]],
            holeHalvePoints: [1, [Validators.required, Validators.min(0), Validators.max(3)]],
            matchWinBonus: [2, [Validators.required, Validators.min(0), Validators.max(10)]],
            matchTiePoints: [1, [Validators.required, Validators.min(0), Validators.max(5)]],
            useSessionHandicaps: [false],
            allowHandicapUpdates: [true],
            customRules: ['', Validators.maxLength(2000)]
        });
    }

    private loadEnumOptions(): void {
        this.leagueSettingsService.getHandicapMethods().subscribe({
            next: (methods) => this.handicapMethods = methods,
            error: (error) => console.error('Error loading handicap methods:', error)
        });

        this.leagueSettingsService.getAverageMethods().subscribe({
            next: (methods) => this.averageMethods = methods,
            error: (error) => console.error('Error loading average methods:', error)
        });

        this.leagueSettingsService.getScoringMethods().subscribe({
            next: (methods) => this.scoringMethods = methods,
            error: (error) => console.error('Error loading scoring methods:', error)
        });

        this.leagueSettingsService.getPointsSystems().subscribe({
            next: (systems) => this.pointsSystems = systems,
            error: (error) => console.error('Error loading points systems:', error)
        });
    }

    private loadLeagueSettings(): void {
        if (!this.selectedSeasonId) return;

        this.isLoading = true;
        this.error = null;

        this.leagueSettingsService.getLeagueSettings(this.selectedSeasonId).subscribe({
            next: (settings) => {
                console.log('Loaded settings:', settings);
                this.settingsForm.patchValue({
                    leagueName: settings.leagueName,
                    handicapMethod: settings.handicapMethod,
                    averageMethod: settings.averageMethod,
                    legacyInitialWeight: settings.legacyInitialWeight,
                    coursePar: settings.coursePar,
                    courseRating: settings.courseRating,
                    slopeRating: settings.slopeRating,
                    maxRoundsForHandicap: settings.maxRoundsForHandicap,
                    scoringMethod: settings.scoringMethod,
                    pointsSystem: settings.pointsSystem,
                    holeWinPoints: settings.holeWinPoints,
                    holeHalvePoints: settings.holeHalvePoints,
                    matchWinBonus: settings.matchWinBonus,
                    matchTiePoints: settings.matchTiePoints,
                    useSessionHandicaps: settings.useSessionHandicaps,
                    allowHandicapUpdates: settings.allowHandicapUpdates,
                    customRules: settings.customRules || ''
                });

                // Update the league name service with the loaded name
                this.leagueNameService.setLeagueName(settings.leagueName);

                this.isLoading = false;
                
                // Use Promise.resolve to defer the validation check and avoid change detection issues
                Promise.resolve().then(() => {
                    console.log('Form after patch:', this.settingsForm.value);
                    console.log('Form valid after patch:', this.settingsForm.valid);
                    console.log('Form errors after patch:', this.getFormValidationErrors());
                    this.cdr.detectChanges();
                });
            },
            error: (error) => {
                this.error = 'Failed to load league settings. Please try again.';
                this.isLoading = false;
                console.error('Error loading league settings:', error);
            }
        });
    }

    onSubmit(): void {
        console.log('=== FORM SUBMISSION DEBUG ===');
        console.log('Form valid:', this.settingsForm.valid);
        console.log('Form value:', this.settingsForm.value);
        console.log('Form errors:', this.getFormValidationErrors());
        console.log('Selected season:', this.selectedSeasonId);
        console.log('isSaving:', this.isSaving);
        console.log('Save button enabled:', this.isSaveButtonEnabled());
        
        // Log each control's status
        Object.keys(this.settingsForm.controls).forEach(key => {
            const control = this.settingsForm.get(key);
            if (control && control.invalid) {
                console.log(`Invalid control ${key}:`, control.errors);
            }
        });

        if (!this.settingsForm.valid) {
            console.log('Form is INVALID - cannot submit');
            return;
        }
        
        if (!this.selectedSeasonId) {
            console.log('No season selected - cannot submit');
            return;
        }

        this.isSaving = true;
        this.error = null;
        this.successMessage = null;

        const formValue = this.settingsForm.value;
        const request = {
            leagueName: formValue.leagueName,
            handicapMethod: parseInt(formValue.handicapMethod),
            averageMethod: parseInt(formValue.averageMethod),
            legacyInitialWeight: formValue.legacyInitialWeight,
            coursePar: formValue.coursePar,
            courseRating: formValue.courseRating,
            slopeRating: formValue.slopeRating,
            maxRoundsForHandicap: formValue.maxRoundsForHandicap,
            scoringMethod: parseInt(formValue.scoringMethod),
            pointsSystem: parseInt(formValue.pointsSystem),
            holeWinPoints: formValue.holeWinPoints,
            holeHalvePoints: formValue.holeHalvePoints,
            matchWinBonus: formValue.matchWinBonus,
            matchTiePoints: formValue.matchTiePoints,
            useSessionHandicaps: formValue.useSessionHandicaps,
            allowHandicapUpdates: formValue.allowHandicapUpdates,
            customRules: formValue.customRules
        };

        console.log('Sending request:', request);

        this.leagueSettingsService.updateLeagueSettings(this.selectedSeasonId, request).subscribe({
            next: (savedSettings) => {
                this.isSaving = false;
                this.successMessage = 'League settings saved successfully!';
                console.log('League settings saved successfully:', savedSettings);

                // Update the league name service with the new name
                this.leagueNameService.setLeagueName(savedSettings.leagueName);

                // Clear success message after 5 seconds
                setTimeout(() => {
                    this.successMessage = null;
                    this.cdr.detectChanges();
                }, 5000);
            },
            error: (error) => {
                this.error = 'Failed to save league settings. Please try again.';
                this.isSaving = false;
                console.error('Error saving league settings:', error);
                console.error('Error details:', error.error);
            }
        });
    }

    resetToDefaults(): void {
        if (!this.selectedSeasonId) return;

        if (confirm('Are you sure you want to reset all league settings to defaults? This action cannot be undone.')) {
            this.isSaving = true;
            this.error = null;
            this.successMessage = null;

            this.leagueSettingsService.resetToDefaults(this.selectedSeasonId).subscribe({
                next: (defaultSettings) => {
                    this.settingsForm.patchValue({
                        leagueName: defaultSettings.leagueName,
                        handicapMethod: defaultSettings.handicapMethod,
                        averageMethod: defaultSettings.averageMethod,
                        legacyInitialWeight: defaultSettings.legacyInitialWeight,
                        coursePar: defaultSettings.coursePar,
                        courseRating: defaultSettings.courseRating,
                        slopeRating: defaultSettings.slopeRating,
                        maxRoundsForHandicap: defaultSettings.maxRoundsForHandicap,
                        scoringMethod: defaultSettings.scoringMethod,
                        pointsSystem: defaultSettings.pointsSystem,
                        holeWinPoints: defaultSettings.holeWinPoints,
                        holeHalvePoints: defaultSettings.holeHalvePoints,
                        matchWinBonus: defaultSettings.matchWinBonus,
                        matchTiePoints: defaultSettings.matchTiePoints,
                        useSessionHandicaps: defaultSettings.useSessionHandicaps,
                        allowHandicapUpdates: defaultSettings.allowHandicapUpdates,
                        customRules: defaultSettings.customRules || ''
                    });
                    this.isSaving = false;
                    this.successMessage = 'League settings reset to defaults successfully!';
                    console.log('League settings reset to defaults');

                    // Update the league name service with the default name
                    this.leagueNameService.setLeagueName(defaultSettings.leagueName);

                    // Clear success message after 5 seconds
                    setTimeout(() => {
                        this.successMessage = null;
                        this.cdr.detectChanges();
                    }, 5000);
                },
                error: (error) => {
                    this.error = 'Failed to reset league settings. Please try again.';
                    this.isSaving = false;
                    console.error('Error resetting league settings:', error);
                }
            });
        }
    }

    getHandicapMethodDisplayName(name: string): string {
        switch (name) {
            case 'WorldHandicapSystem': return 'World Handicap System';
            case 'SimpleAverage': return 'Simple Average';
            case 'LegacyLookupTable': return 'Legacy Lookup Table';
            default: return name;
        }
    }

    getAverageMethodDisplayName(name: string): string {
        switch (name) {
            case 'SimpleAverage': return 'Simple Average';
            case 'LegacyWeightedAverage': return 'Legacy Weighted Average';
            default: return name;
        }
    }

    getScoringMethodDisplayName(name: string): string {
        switch (name) {
            case 'MatchPlay': return 'Match Play';
            case 'StrokePlay': return 'Stroke Play';
            default: return name;
        }
    }

    getPointsSystemDisplayName(name: string): string {
        switch (name) {
            case 'HolePointsWithMatchBonus': return 'Hole Points with Match Bonus';
            case 'ScoreBasedPoints': return 'Score-Based Points';
            case 'Custom': return 'Custom';
            default: return name;
        }
    }

    getFormValidationErrors() {
        if (!this.settingsForm) return {};
        
        const result: any = {};
        Object.keys(this.settingsForm.controls).forEach(key => {
            const control = this.settingsForm.get(key);
            if (control && control.errors) {
                result[key] = control.errors;
            }
        });
        return result;
    }

    // Debug method to help identify form validation issues
    getFormDebugInfo(): string {
        if (!this.settingsForm) return 'Form not initialized';

        const formErrors: string[] = [];
        Object.keys(this.settingsForm.controls).forEach(key => {
            const control = this.settingsForm.get(key);
            if (control && control.invalid) {
                const errors = control.errors;
                if (errors) {
                    const errorList = Object.keys(errors).join(', ');
                    formErrors.push(`${key}: ${errorList}`);
                }
            }
        });

        return formErrors.length > 0 ? formErrors.join(' | ') : 'No form errors found';
    }

    getFormStatus(): string {
        return `Valid: ${this.settingsForm?.valid} | Season: ${!!this.selectedSeasonId} | Saving: ${this.isSaving}`;
    }

    isSaveButtonEnabled(): boolean {
        return this.settingsForm.valid && !this.isSaving && !!this.selectedSeasonId;
    }
}
