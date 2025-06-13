import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ScheduleSettingsComponent } from './schedule-settings.component';

describe('ScheduleSettingsComponent', () => {
  let component: ScheduleSettingsComponent;
  let fixture: ComponentFixture<ScheduleSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScheduleSettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ScheduleSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default template form values', () => {
    expect(component.templateForm.get('name')?.value).toBe('');
    expect(component.templateForm.get('numberOfWeeks')?.value).toBe(null);
    expect(component.templateForm.get('flightsPerWeek')?.value).toBe(null);
  });

  it('should load schedule templates on init', () => {
    expect(component.scheduleTemplates.length).toBeGreaterThan(0);
  });

  it('should have default settings form values', () => {
    expect(component.settingsForm.get('defaultPlayDay')?.value).toBe('Tuesday');
    expect(component.settingsForm.get('defaultStartTime')?.value).toBe('9:00 AM');
  });
});
