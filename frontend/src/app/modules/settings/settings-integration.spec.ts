import { TestBed } from '@angular/core/testing';
import { SettingsComponent } from './settings.component';
import { PlayersSettingsComponent } from '../components/players-settings/players-settings.component';
import { SeasonsSettingsComponent } from '../components/seasons-settings/seasons-settings.component';
import { ScoringSettingsComponent } from '../components/scoring-settings/scoring-settings.component';
import { ScheduleSettingsComponent } from '../components/schedule-settings/schedule-settings.component';

describe('Settings Module Integration', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        SettingsComponent,
        PlayersSettingsComponent,
        SeasonsSettingsComponent,
        ScoringSettingsComponent,
        ScheduleSettingsComponent
      ]
    });
  });

  it('should create all settings components', () => {
    const settingsFixture = TestBed.createComponent(SettingsComponent);
    const playersFixture = TestBed.createComponent(PlayersSettingsComponent);
    const seasonsFixture = TestBed.createComponent(SeasonsSettingsComponent);
    const scoringFixture = TestBed.createComponent(ScoringSettingsComponent);
    const scheduleFixture = TestBed.createComponent(ScheduleSettingsComponent);

    expect(settingsFixture.componentInstance).toBeTruthy();
    expect(playersFixture.componentInstance).toBeTruthy();
    expect(seasonsFixture.componentInstance).toBeTruthy();
    expect(scoringFixture.componentInstance).toBeTruthy();
    expect(scheduleFixture.componentInstance).toBeTruthy();
  });

  it('should have proper tab structure in main settings component', () => {
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    
    expect(fixture.componentInstance.activeTab).toBe('players');
  });

  it('should initialize all tab components with default data', () => {
    const playersFixture = TestBed.createComponent(PlayersSettingsComponent);
    const seasonsFixture = TestBed.createComponent(SeasonsSettingsComponent);
    const scoringFixture = TestBed.createComponent(ScoringSettingsComponent);
    const scheduleFixture = TestBed.createComponent(ScheduleSettingsComponent);

    playersFixture.detectChanges();
    seasonsFixture.detectChanges();
    scoringFixture.detectChanges();
    scheduleFixture.detectChanges();

    // Check that components have been initialized with mock data
    expect(playersFixture.componentInstance.players.length).toBeGreaterThan(0);
    expect(seasonsFixture.componentInstance.seasons.length).toBeGreaterThan(0);
    expect(scoringFixture.componentInstance.scoringForm.valid).toBeTruthy();
    expect(scheduleFixture.componentInstance.scheduleTemplates.length).toBeGreaterThan(0);
  });
});
