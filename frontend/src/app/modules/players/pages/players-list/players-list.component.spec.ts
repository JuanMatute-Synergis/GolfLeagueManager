import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { PlayersListComponent } from './players-list.component';
import { PlayerService } from '../../../settings/services/player.service';

describe('PlayersListComponent', () => {
  let component: PlayersListComponent;
  let fixture: ComponentFixture<PlayersListComponent>;
  let mockPlayerService: jasmine.SpyObj<PlayerService>;

  const mockPlayers = [
    {
      id: '1',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com',
      phone: '555-0123'
    },
    {
      id: '2',
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@example.com',
      phone: '555-0456'
    }
  ];

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('PlayerService', ['getPlayers']);

    await TestBed.configureTestingModule({
      imports: [PlayersListComponent],
      providers: [
        { provide: PlayerService, useValue: spy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PlayersListComponent);
    component = fixture.componentInstance;
    mockPlayerService = TestBed.inject(PlayerService) as jasmine.SpyObj<PlayerService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load players on init', () => {
    mockPlayerService.getPlayers.and.returnValue(of(mockPlayers));

    component.ngOnInit();

    expect(mockPlayerService.getPlayers).toHaveBeenCalled();
    expect(component.players).toEqual(mockPlayers);
    expect(component.isLoading).toBeFalse();
  });

  it('should handle error when loading players', () => {
    mockPlayerService.getPlayers.and.returnValue(throwError(() => new Error('Test error')));

    component.ngOnInit();

    expect(component.error).toBe('Failed to load players. Please try again.');
    expect(component.isLoading).toBeFalse();
  });

  it('should dismiss error', () => {
    component.error = 'Test error';

    component.dismissError();

    expect(component.error).toBeNull();
  });
});
