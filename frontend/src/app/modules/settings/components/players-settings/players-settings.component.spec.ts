import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayersSettingsComponent } from './players-settings.component';

describe('PlayersSettingsComponent', () => {
  let component: PlayersSettingsComponent;
  let fixture: ComponentFixture<PlayersSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayersSettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlayersSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default player form values', () => {
    expect(component.playerForm.get('name')?.value).toBe('');
    expect(component.playerForm.get('email')?.value).toBe('');
    expect(component.playerForm.get('handicap')?.value).toBe(null);
  });

  it('should mark form as invalid when required fields are empty', () => {
    expect(component.playerForm.valid).toBeFalsy();
  });

  it('should load players on init', () => {
    expect(component.players.length).toBeGreaterThan(0);
  });
});
