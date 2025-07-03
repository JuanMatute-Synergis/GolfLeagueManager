import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { LeagueSettingsService } from '../../services/league-settings.service';
import { SeasonService, Season } from '../../services/season.service';
import {
    LeagueSettings,
    HandicapCalculationMethod,
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

    handicapMethods: EnumOption[] = [];
    scoringMethods: EnumOption[] = [];
    pointsSystems: EnumOption[] = [];

    constructor(
        private fb: FormBuilder,
        private leagueSettingsService: LeagueSettingsService,
        private seasonService: SeasonService
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
            handicapMethod: [0, Validators.required],
            coursePar: [36, [Validators.required, Validators.min(30), Validators.max(45)]],
            courseRating: [35.0, [Validators.required, Validators.min(25), Validators.max(45)]],
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
                this.settingsForm.patchValue({
                    handicapMethod: settings.handicapMethod,
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
                this.isLoading = false;
            },
            error: (error) => {
                this.error = 'Failed to load league settings. Please try again.';
                this.isLoading = false;
                console.error('Error loading league settings:', error);
            }
        });
    }

    onSubmit(): void {
        if (!this.settingsForm.valid || !this.selectedSeasonId) return;

        this.isSaving = true;
        this.error = null;

        const formValue = this.settingsForm.value;
        const request = {
            handicapMethod: parseInt(formValue.handicapMethod),
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

        this.leagueSettingsService.updateLeagueSettings(this.selectedSeasonId, request).subscribe({
            next: (savedSettings) => {
                this.isSaving = false;
                // Could emit an event or show a success message here
                console.log('League settings saved successfully');
            },
            error: (error) => {
                this.error = 'Failed to save league settings. Please try again.';
                this.isSaving = false;
                console.error('Error saving league settings:', error);
            }
        });
    }

    resetToDefaults(): void {
        if (!this.selectedSeasonId) return;

        if (confirm('Are you sure you want to reset all league settings to defaults? This action cannot be undone.')) {
            this.isSaving = true;
            this.error = null;

            this.leagueSettingsService.resetToDefaults(this.selectedSeasonId).subscribe({
                next: (defaultSettings) => {
                    this.settingsForm.patchValue({
                        handicapMethod: defaultSettings.handicapMethod,
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
                    console.log('League settings reset to defaults');
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
}
