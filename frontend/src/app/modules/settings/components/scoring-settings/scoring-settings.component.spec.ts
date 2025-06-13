import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ScoringSettingsComponent } from './scoring-settings.component';

describe('ScoringSettingsComponent', () => {
  let component: ScoringSettingsComponent;
  let fixture: ComponentFixture<ScoringSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScoringSettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ScoringSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default scoring form values', () => {
    expect(component.scoringForm.get('winPoints')?.value).toBe(2);
    expect(component.scoringForm.get('lossPoints')?.value).toBe(0);
    expect(component.scoringForm.get('tiePoints')?.value).toBe(1);
  });

  it('should validate point values are non-negative', () => {
    component.scoringForm.patchValue({ winPoints: -1 });
    expect(component.scoringForm.get('winPoints')?.valid).toBeFalsy();
  });
});
