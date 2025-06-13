import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SeasonsSettingsComponent } from './seasons-settings.component';

describe('SeasonsSettingsComponent', () => {
  let component: SeasonsSettingsComponent;
  let fixture: ComponentFixture<SeasonsSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SeasonsSettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SeasonsSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default season form values', () => {
    expect(component.seasonForm.get('name')?.value).toBe('');
    expect(component.seasonForm.get('startDate')?.value).toBe('');
    expect(component.seasonForm.get('endDate')?.value).toBe('');
  });

  it('should load seasons on init', () => {
    expect(component.seasons.length).toBeGreaterThan(0);
  });
});
