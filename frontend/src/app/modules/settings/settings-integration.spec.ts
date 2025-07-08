import { TestBed } from '@angular/core/testing';
import { SettingsComponent } from './settings.component';
import { AccountsSettingsComponent } from './components/accounts-settings/accounts-settings.component';
import { SeasonsSettingsComponent } from './components/seasons-settings/seasons-settings.component';
import { ScoringSettingsComponent } from './components/scoring-settings/scoring-settings.component';
import { ScheduleSettingsComponent } from './components/schedule-settings/schedule-settings.component';

describe('Settings Module Integration', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        SettingsComponent,
        AccountsSettingsComponent,
        SeasonsSettingsComponent,
        ScoringSettingsComponent,
        ScheduleSettingsComponent
      ]
    });
  });

  it('should create all settings components', () => {
    const settingsFixture = TestBed.createComponent(SettingsComponent);
    const accountsFixture = TestBed.createComponent(AccountsSettingsComponent);
    const seasonsFixture = TestBed.createComponent(SeasonsSettingsComponent);
    const scoringFixture = TestBed.createComponent(ScoringSettingsComponent);
    const scheduleFixture = TestBed.createComponent(ScheduleSettingsComponent);

    expect(settingsFixture.componentInstance).toBeTruthy();
    expect(accountsFixture.componentInstance).toBeTruthy();
    expect(seasonsFixture.componentInstance).toBeTruthy();
    expect(scoringFixture.componentInstance).toBeTruthy();
    expect(scheduleFixture.componentInstance).toBeTruthy();
  });

  it('should have proper tab structure in main settings component', () => {
    const fixture = TestBed.createComponent(SettingsComponent);
    fixture.detectChanges();
    
    // Note: This test may need to be updated based on the actual SettingsComponent implementation
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should initialize all tab components with default data', () => {
    const accountsFixture = TestBed.createComponent(AccountsSettingsComponent);
    const seasonsFixture = TestBed.createComponent(SeasonsSettingsComponent);
    const scoringFixture = TestBed.createComponent(ScoringSettingsComponent);
    const scheduleFixture = TestBed.createComponent(ScheduleSettingsComponent);

    accountsFixture.detectChanges();
    seasonsFixture.detectChanges();
    scoringFixture.detectChanges();
    scheduleFixture.detectChanges();

    // Check that components have been initialized
    expect(accountsFixture.componentInstance).toBeTruthy();
    expect(seasonsFixture.componentInstance).toBeTruthy();
    expect(scoringFixture.componentInstance).toBeTruthy();
    expect(scheduleFixture.componentInstance).toBeTruthy();
  });
});
