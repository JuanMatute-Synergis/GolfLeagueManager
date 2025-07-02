import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FlightService, Flight } from '../../services/flight.service';
import { PlayerService, Player } from '../../services/player.service';
import { PlayerFlightAssignmentService, PlayerFlightAssignment } from '../../services/player-flight-assignment.service';
import { SeasonService, Season } from '../../services/season.service';

@Component({
  selector: 'app-seasons-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './seasons-settings.component.html',
  styleUrls: ['./seasons-settings.component.css']
})
export class SeasonsSettingsComponent implements OnInit {
  // Forms
  seasonForm: FormGroup;
  flightForm: FormGroup;

  // Data
  seasons: Season[] = [];
  seasonFlights: Flight[] = [];
  flightAssignments: PlayerFlightAssignment[] = [];
  allSeasonAssignments: PlayerFlightAssignment[] = []; // All assignments for the current season
  players: Player[] = [];

  // Selected data
  selectedSeasonId: string | null = null;
  selectedFlightId: string | null = null;
  selectedPlayerId: string | null = null;

  // UI state
  showSeasonForm = false;
  showFlightForm = false;
  showPlayerAssignmentModal = false;
  isLoading = false;
  error: string | null = null;
  isEditMode = false;

  // Mobile state
  mobileActiveTab: 'seasons' | 'flights' | 'players' | 'league-settings' = 'seasons';

  // Editing state
  editingSeasonId: string | null = null;
  editingFlightId: string | null = null;
  isFlightLeader: boolean = false;
  playerHandicap: number | null = null;

  get selectedSeason(): Season | undefined {
    return this.seasons.find(season => season.id === this.selectedSeasonId);
  }

  constructor(
    private fb: FormBuilder,
    private flightService: FlightService,
    private playerService: PlayerService,
    private playerFlightAssignmentService: PlayerFlightAssignmentService,
    private seasonService: SeasonService
  ) {
    this.seasonForm = this.fb.group({
      name: ['', Validators.required],
      year: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(2100)]],
      seasonNumber: [1, [Validators.required, Validators.min(1)]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required]
    });

    this.flightForm = this.fb.group({
      name: ['', Validators.required],
      maxPlayers: [16, [Validators.required, Validators.min(1)]],
      description: [''],
      isActive: [true]
    });
  }

  ngOnInit() {
    this.loadPlayers();
    this.loadSeasons();
  }

  loadPlayers() {
    this.playerService.getPlayers().subscribe({
      next: (players: Player[]) => {
        this.players = players;
      },
      error: (error: any) => {
        console.error('Error loading players:', error);
      }
    });
  }

  loadSeasons() {
    this.seasonService.getSeasons().subscribe({
      next: (seasons: Season[]) => {
        this.seasons = seasons;
      },
      error: (error: any) => {
        console.error('Error loading seasons:', error);
        this.error = 'Failed to load seasons. Please try again.';
      }
    });
  }

  // Season Management
  showAddSeasonForm() {
    this.showSeasonForm = true;
    this.editingSeasonId = null;
    this.seasonForm.reset({
      year: new Date().getFullYear(),
      seasonNumber: 1
    });
  }

  editSeason(season: Season) {
    this.showSeasonForm = true;
    this.editingSeasonId = season.id;
    this.seasonForm.patchValue(season);
  }

  saveSeason() {
    if (this.seasonForm.valid) {
      const formValue = this.seasonForm.value;
      this.isLoading = true;

      if (this.editingSeasonId) {
        // Update existing season
        const seasonToUpdate: Season = {
          ...formValue,
          id: this.editingSeasonId
        };

        this.seasonService.updateSeason(seasonToUpdate).subscribe({
          next: () => {
            // Update local array
            const idx = this.seasons.findIndex(s => s.id === this.editingSeasonId);
            if (idx !== -1) {
              this.seasons[idx] = seasonToUpdate;
            }
            this.resetSeasonForm();
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error updating season:', error);
            this.error = 'Failed to update season. Please try again.';
            this.isLoading = false;
          }
        });
      } else {
        // Create new season
        const newSeason: Omit<Season, 'id'> = formValue;

        this.seasonService.addSeason(newSeason as Season).subscribe({
          next: (savedSeason: Season) => {
            this.seasons.push(savedSeason);
            this.resetSeasonForm();
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error creating season:', error);
            this.error = 'Failed to create season. Please try again.';
            this.isLoading = false;
          }
        });
      }
    }
  }

  deleteSeason(season: Season) {
    if (confirm('Are you sure you want to delete this season?')) {
      this.isLoading = true;

      this.seasonService.deleteSeason(season.id).subscribe({
        next: () => {
          this.seasons = this.seasons.filter(s => s.id !== season.id);
          if (this.selectedSeasonId === season.id) {
            this.selectedSeasonId = null;
            this.seasonFlights = [];
          }
          this.isLoading = false;
        },
        error: (error: any) => {
          console.error('Error deleting season:', error);
          this.error = 'Failed to delete season. Please try again.';
          this.isLoading = false;
        }
      });
    }
  }

  selectSeason(season: Season) {
    this.selectedSeasonId = season.id;
    this.loadSeasonFlights(season.id);
  }

  resetSeasonForm() {
    this.showSeasonForm = false;
    this.editingSeasonId = null;
    this.seasonForm.reset();
  }

  // Flight Management
  loadSeasonFlights(seasonId: string) {
    if (!seasonId) return;

    this.isLoading = true;
    this.error = null;

    // Use the actual API to load flights for the selected season
    this.flightService.getFlightsBySeason(seasonId).subscribe({
      next: (flights: Flight[]) => {
        this.seasonFlights = flights;
        this.isLoading = false;
        // Load all season assignments after flights are loaded
        this.loadAllSeasonAssignments(seasonId);
      },
      error: (error: any) => {
        console.error('Error loading season flights:', error);
        this.error = 'Failed to load flights for this season. Please try again.';
        this.isLoading = false;
      }
    });
  }

  showAddFlightForm() {
    if (!this.selectedSeasonId) {
      this.error = 'Please select a season first';
      return;
    }
    this.showFlightForm = true;
    this.editingFlightId = null;
    this.flightForm.reset({
      maxPlayers: 16,
      isActive: true
    });
  }

  editFlight(flight: Flight) {
    this.showFlightForm = true;
    this.editingFlightId = flight.id!;

    this.flightForm.patchValue({
      ...flight
    });
  }

  saveFlight() {
    if (this.flightForm.valid && this.selectedSeasonId) {
      const flightData = this.flightForm.value;
      this.isLoading = true;

      const flightPayload: Flight = {
        ...flightData,
        seasonId: this.selectedSeasonId
      };

      if (this.editingFlightId) {
        // Update existing flight
        flightPayload.id = this.editingFlightId;
        this.flightService.updateFlight(flightPayload).subscribe({
          next: () => {
            const flightIndex = this.seasonFlights.findIndex(f => f.id === this.editingFlightId);
            if (flightIndex !== -1) {
              this.seasonFlights[flightIndex] = {
                ...flightPayload,
                updatedAt: new Date().toISOString()
              };
            }
            this.resetFlightForm();
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error updating flight:', error);
            this.error = 'Failed to update flight. Please try again.';
            this.isLoading = false;
          }
        });
      } else {
        // Create new flight
        this.flightService.addFlight(flightPayload).subscribe({
          next: (savedFlight: Flight) => {
            this.seasonFlights.push(savedFlight);
            this.resetFlightForm();
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error creating flight:', error);
            this.error = 'Failed to create flight. Please try again.';
            this.isLoading = false;
          }
        });
      }
    }
  }

  deleteFlight(id: string) {
    if (confirm('Are you sure you want to delete this flight?')) {
      this.flightService.deleteFlight(id).subscribe({
        next: () => {
          this.seasonFlights = this.seasonFlights.filter(f => f.id !== id);
          if (this.selectedFlightId === id) {
            this.selectedFlightId = null;
            this.flightAssignments = [];
          }
        },
        error: (error: any) => {
          console.error('Error deleting flight:', error);
          this.error = 'Failed to delete flight. Please try again.';
        }
      });
    }
  }

  resetFlightForm() {
    this.showFlightForm = false;
    this.editingFlightId = null;
    this.flightForm.reset({
      maxPlayers: 16,
      isActive: true
    });
  }

  // Player Flight Assignment
  selectFlight(flight: Flight) {
    this.selectedFlightId = flight.id!;
    this.loadFlightAssignments(flight.id!);
  }

  loadFlightAssignments(flightId: string) {
    this.isLoading = true;
    this.error = null;

    this.selectedPlayerId = null;
    this.isFlightLeader = false;
    this.playerHandicap = null;

    this.playerFlightAssignmentService.getAssignmentsByFlight(flightId).subscribe({
      next: (assignments: PlayerFlightAssignment[]) => {
        this.flightAssignments = assignments;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading flight assignments:', error);
        this.error = 'Failed to load flight assignments. Please try again.';
        this.isLoading = false;
      }
    });
  }

  loadAllSeasonAssignments(seasonId: string) {
    // Get all assignments and filter by season flights
    this.playerFlightAssignmentService.getAllAssignments().subscribe({
      next: (allAssignments: PlayerFlightAssignment[]) => {
        // Filter assignments to only include those for flights in the current season
        const seasonFlightIds = this.seasonFlights.map(flight => flight.id!);
        this.allSeasonAssignments = allAssignments.filter(assignment =>
          seasonFlightIds.includes(assignment.flightId)
        );
      },
      error: (error: any) => {
        console.error('Error loading season assignments:', error);
        this.error = 'Failed to load season assignments. Please try again.';
      }
    });
  }

  onPlayerSelected() {
    if (!this.selectedPlayerId) {
      this.playerHandicap = null;
    }
    // Let the user enter the handicap manually
  }

  getSelectedFlightName(): string {
    if (!this.selectedFlightId) return '';
    const flight = this.seasonFlights.find(f => f.id === this.selectedFlightId);
    return flight ? flight.name : '';
  }

  getAvailablePlayers(): Player[] {
    if (!this.selectedFlightId) return [];

    // Filter out players already assigned to ANY flight in the current season
    const assignedPlayerIds = this.allSeasonAssignments.map(a => a.playerId);
    return this.players.filter(p => !assignedPlayerIds.includes(p.id!));
  }

  getPlayerNameById(playerId: string): string {
    const player = this.players.find(p => p.id === playerId);
    return player ? `${player.firstName} ${player.lastName}` : 'Unknown Player';
  }

  assignPlayerToFlight() {
    if (!this.selectedPlayerId || !this.selectedFlightId) return;

    this.isLoading = true;

    const assignment: PlayerFlightAssignment = {
      playerId: this.selectedPlayerId,
      flightId: this.selectedFlightId,
      isFlightLeader: this.isFlightLeader,
      handicapAtAssignment: this.playerHandicap || 0
    };

    this.playerFlightAssignmentService.addAssignment(assignment).subscribe({
      next: (savedAssignment: PlayerFlightAssignment) => {
        this.flightAssignments.push(savedAssignment);
        // Also add to season-wide assignments
        this.allSeasonAssignments.push(savedAssignment);

        this.selectedPlayerId = null;
        this.isFlightLeader = false;
        this.playerHandicap = null;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error assigning player to flight:', error);
        this.error = 'Failed to assign player to flight. Please try again.';
        this.isLoading = false;
      }
    });
  }

  toggleFlightLeader(assignment: PlayerFlightAssignment) {
    this.isLoading = true;

    const updatedAssignment = {
      ...assignment,
      isFlightLeader: !assignment.isFlightLeader
    };

    this.playerFlightAssignmentService.updateAssignment(updatedAssignment).subscribe({
      next: () => {
        const index = this.flightAssignments.findIndex(a => a.id === assignment.id);
        if (index !== -1) {
          this.flightAssignments[index] = updatedAssignment;
        }
        // Also update in season-wide assignments
        const seasonIndex = this.allSeasonAssignments.findIndex(a => a.id === assignment.id);
        if (seasonIndex !== -1) {
          this.allSeasonAssignments[seasonIndex] = updatedAssignment;
        }
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error updating flight leader status:', error);
        this.error = 'Failed to update flight leader status. Please try again.';
        this.isLoading = false;
      }
    });
  }

  removeAssignment(assignmentId: string) {
    this.isLoading = true;

    this.playerFlightAssignmentService.removeAssignment(assignmentId).subscribe({
      next: () => {
        this.flightAssignments = this.flightAssignments.filter(a => a.id !== assignmentId);
        // Also remove from season-wide assignments
        this.allSeasonAssignments = this.allSeasonAssignments.filter(a => a.id !== assignmentId);
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error removing player from flight:', error);
        this.error = 'Failed to remove player from flight. Please try again.';
        this.isLoading = false;
      }
    });
  }

  // Mobile navigation methods
  setMobileActiveTab(tab: 'seasons' | 'flights' | 'players') {
    this.mobileActiveTab = tab;
  }

  selectSeasonMobile(season: Season) {
    this.selectSeason(season);
    this.setMobileActiveTab('flights');
  }

  selectFlightMobile(flight: Flight) {
    this.selectFlight(flight);
    this.setMobileActiveTab('players');
  }

  getFlightAssignmentCount(flightId: string): number {
    return this.flightAssignments.filter(assignment => assignment.flightId === flightId).length;
  }
}
