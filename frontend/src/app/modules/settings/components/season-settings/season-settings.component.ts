import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FlightService, Flight } from '../../services/flight.service';
import { PlayerService, Player } from '../../services/player.service';
import { PlayerFlightAssignmentService, PlayerFlightAssignment } from '../../services/player-flight-assignment.service';

interface Season {
  id: number;
  name: string;
  year: number;
  seasonNumber: number;
  startDate: string;
  endDate: string;
}

@Component({
  selector: 'app-season-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './season-settings.component.html'
})
export class SeasonSettingsComponent implements OnInit {
  // Forms
  seasonForm: FormGroup;
  flightForm: FormGroup;
  
  // Season Data
  seasons: Season[] = [
    {
      id: 1,
      name: 'Spring 2025',
      year: 2025,
      seasonNumber: 1,
      startDate: '2025-03-01',
      endDate: '2025-06-01'
    },
    {
      id: 2,
      name: 'Summer 2025',
      year: 2025,
      seasonNumber: 2,
      startDate: '2025-06-15',
      endDate: '2025-09-01'
    }
  ];
  
  selectedSeasonId: number | null = null;
  
  get selectedSeason(): Season | undefined {
    return this.seasons.find(season => season.id === this.selectedSeasonId);
  }
  
  // Flight Data
  flights: Flight[] = [];
  seasonFlights: Flight[] = [];
  selectedFlightId: number | null = null;
  flightAssignments: PlayerFlightAssignment[] = [];
  
  // Player Data
  players: Player[] = [];
  playersWithHandicap: (Player & { handicap?: number })[] = [];
  
  // Player assignment state
  selectedPlayerId: number | null = null;
  isFlightLeader: boolean = false;
  playerHandicap: number | null = null;
  
  // UI State
  showSeasonForm = false;
  showFlightForm = false;
  editingSeasonId: number | null = null;
  editingFlightId: number | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private flightService: FlightService,
    private playerService: PlayerService,
    private playerFlightAssignmentService: PlayerFlightAssignmentService
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
      date: ['', Validators.required],
      startTime: ['08:00', Validators.required],
      course: ['', Validators.required],
      maxPlayers: [16, [Validators.required, Validators.min(1)]],
      description: [''],
      isActive: [true],
      seasonId: [null]
    });
  }

  ngOnInit(): void {
    this.loadInitialData();
  }

  loadInitialData() {
    this.loadPlayers();
    this.loadFlights();
  }

  loadPlayers() {
    this.loading = true;
    this.error = null;
    
    this.playerService.getPlayers().subscribe({
      next: (players: Player[]) => {
        this.players = players;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading players:', error);
        this.error = 'Failed to load players. Please try again.';
        this.loading = false;
      }
    });
  }

  loadFlights() {
    this.loading = true;
    this.error = null;
    
    this.flightService.getFlights().subscribe({
      next: (flights: Flight[]) => {
        this.flights = flights;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading flights:', error);
        this.error = 'Failed to load flights. Please try again.';
        this.loading = false;
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
    this.seasonForm.patchValue({
      name: season.name,
      year: season.year,
      seasonNumber: season.seasonNumber,
      startDate: season.startDate,
      endDate: season.endDate
    });
  }

  saveSeason() {
    if (this.seasonForm.valid) {
      const seasonData = this.seasonForm.value;
      
      if (this.editingSeasonId) {
        // Update existing season
        const updatedSeason = { ...seasonData, id: this.editingSeasonId };
        const index = this.seasons.findIndex(s => s.id === this.editingSeasonId);
        if (index !== -1) {
          this.seasons[index] = updatedSeason;
        }
      } else {
        // Add new season
        const newId = Math.max(...this.seasons.map(s => s.id)) + 1;
        this.seasons.push({ ...seasonData, id: newId });
      }
      
      this.resetSeasonForm();
    }
  }

  deleteSeason(season: Season) {
    if (confirm('Are you sure you want to delete this season?')) {
      this.seasons = this.seasons.filter(s => s.id !== season.id);
      if (this.selectedSeasonId === season.id) {
        this.selectedSeasonId = null;
        this.seasonFlights = [];
      }
    }
  }

  selectSeason(season: Season) {
    this.selectedSeasonId = season.id;
    this.loadSeasonFlights(season.id);
  }

  resetSeasonForm() {
    this.showSeasonForm = false;
    this.editingSeasonId = null;
    this.seasonForm.reset({
      year: new Date().getFullYear(),
      seasonNumber: 1
    });
  }

  cancelSeasonForm() {
    this.resetSeasonForm();
  }

  // Flight Management
  loadSeasonFlights(seasonId: number) {
    if (!seasonId) return;
    
    this.loading = true;
    this.error = null;
    
    // For now, simulate with mock data since backend endpoint may not exist
    // In a real implementation, we would use:
    // this.flightService.getFlightsBySeason(seasonId).subscribe({...})
    
    // Mock implementation - filter existing flights by seasonId
    setTimeout(() => {
      this.seasonFlights = this.flights.filter(flight => flight.seasonId === seasonId);
      
      // If no flights with this season ID (because our mock data doesn't have seasonId set),
      // create some example flights for the selected season
      if (this.seasonFlights.length === 0) {
        const selectedSeason = this.selectedSeason;
        if (selectedSeason) {
          const mockFlights: Flight[] = [
            {
              id: 101,
              name: 'Morning Flight - ' + selectedSeason.name,
              date: selectedSeason.startDate,
              startTime: '08:00:00',
              course: 'Pine Valley Golf Club',
              maxPlayers: 16,
              description: 'Early morning flight for the ' + selectedSeason.name + ' season',
              isActive: true,
              seasonId: seasonId
            },
            {
              id: 102,
              name: 'Afternoon Flight - ' + selectedSeason.name,
              date: selectedSeason.startDate,
              startTime: '13:00:00', 
              course: 'Augusta National',
              maxPlayers: 20,
              description: 'Afternoon flight for the ' + selectedSeason.name + ' season',
              isActive: true,
              seasonId: seasonId
            }
          ];
          this.seasonFlights = mockFlights;
        }
      }
      
      this.loading = false;
    }, 500); // Simulate API delay
  }
  
  showAddFlightForm() {
    this.showFlightForm = true;
    this.editingFlightId = null;
    this.flightForm.reset({
      maxPlayers: 16,
      startTime: '08:00',
      isActive: true,
      seasonId: this.selectedSeasonId,
      date: this.selectedSeason?.startDate
    });
  }

  editFlight(flight: Flight) {
    this.showFlightForm = true;
    this.editingFlightId = flight.id;
    this.flightForm.patchValue({
      name: flight.name,
      date: flight.date,
      startTime: flight.startTime,
      course: flight.course,
      maxPlayers: flight.maxPlayers,
      description: flight.description,
      isActive: flight.isActive,
      seasonId: flight.seasonId || this.selectedSeasonId
    });
  }

  saveFlight() {
    if (this.flightForm.valid) {
      const flightData = this.flightForm.value;
      
      if (!flightData.seasonId && this.selectedSeasonId) {
        flightData.seasonId = this.selectedSeasonId;
      }
      
      this.loading = true;
      
      if (this.editingFlightId) {
        // Update existing flight
        const updatedFlight = { ...flightData, id: this.editingFlightId };
        this.flightService.updateFlight(updatedFlight).subscribe({
          next: () => {
            if (this.selectedSeasonId) {
              this.loadSeasonFlights(this.selectedSeasonId); // Reload flights for the season
            }
            this.resetFlightForm();
            this.loading = false;
          },
          error: (error: any) => {
            console.error('Error updating flight:', error);
            this.error = 'Failed to update flight. Please try again.';
            this.loading = false;
          }
        });
      } else {
        // Add new flight
        this.flightService.addFlight(flightData).subscribe({
          next: () => {
            if (this.selectedSeasonId) {
              this.loadSeasonFlights(this.selectedSeasonId); // Reload flights for the season
            }
            this.resetFlightForm();
            this.loading = false;
          },
          error: (error: any) => {
            console.error('Error adding flight:', error);
            this.error = 'Failed to add flight. Please try again.';
            this.loading = false;
          }
        });
      }
    }
  }

  deleteFlight(id: number) {
    if (confirm('Are you sure you want to delete this flight?')) {
      this.loading = true;
      
      this.flightService.deleteFlight(id).subscribe({
        next: () => {
          if (this.selectedSeasonId) {
            this.loadSeasonFlights(this.selectedSeasonId); // Reload flights for the season
          }
          if (this.selectedFlightId === id) {
            this.selectedFlightId = null;
          }
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error deleting flight:', error);
          this.error = 'Failed to delete flight. Please try again.';
          this.loading = false;
        }
      });
    }
  }

  resetFlightForm() {
    this.showFlightForm = false;
    this.editingFlightId = null;
    this.flightForm.reset({
      maxPlayers: 16,
      startTime: '08:00',
      isActive: true
    });
  }

  cancelFlightForm() {
    this.resetFlightForm();
  }

  // Flight Player Assignment
  /**
   * Select a flight and load its player assignments
   */
  selectFlight(flight: Flight) {
    this.selectedFlightId = flight.id!;
    this.loadFlightAssignments(flight.id!);
  }
  
  /**
   * Load player assignments for a specific flight
   */
  loadFlightAssignments(flightId: number) {
    this.loading = true;
    this.error = null;
    
    // Reset player selection
    this.selectedPlayerId = null;
    this.isFlightLeader = false;
    this.playerHandicap = null;
    
    // Load assignments
    this.playerFlightAssignmentService.getAssignmentsByFlight(flightId).subscribe({
      next: (assignments: PlayerFlightAssignment[]) => {
        this.flightAssignments = assignments;
        
        // Load players with handicap data
        this.loadPlayersWithHandicap(flightId);
      },
      error: (error: any) => {
        console.error('Error loading flight assignments:', error);
        this.error = 'Failed to load flight assignments. Please try again.';
        this.loading = false;
      }
    });
  }
  
  /**
   * Load players with their handicap data
   */
  loadPlayersWithHandicap(flightId: number) {
    this.playerFlightAssignmentService.getPlayersWithHandicap(flightId, this.players).subscribe({
      next: (players) => {
        this.playersWithHandicap = players;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading player handicaps:', error);
        this.loading = false;
      }
    });
  }
  
  /**
   * Update player handicap when a player is selected
   */
  onPlayerSelected() {
    if (this.selectedPlayerId) {
      const selectedPlayer = this.playersWithHandicap.find(p => p.id === this.selectedPlayerId);
      this.playerHandicap = selectedPlayer?.handicap || null;
    } else {
      this.playerHandicap = null;
    }
  }
  
  /**
   * Get the name of the currently selected flight
   */
  getSelectedFlightName(): string {
    if (!this.selectedFlightId) return '';
    const flight = this.seasonFlights.find(f => f.id === this.selectedFlightId);
    return flight ? flight.name : '';
  }

  /**
   * Get a list of players available to be assigned to the current flight
   * (players not already assigned to this flight)
   */
  getAvailablePlayers(): Player[] {
    if (!this.selectedFlightId) return [];
    
    // Get IDs of players already assigned to this flight
    const assignedPlayerIds = this.flightAssignments.map(a => a.playerId);
    
    // Return players not yet assigned
    return this.players.filter(p => !assignedPlayerIds.includes(p.id!));
  }
  
  /**
   * Get a player's full name by their ID
   */
  getPlayerNameById(playerId: number): string {
    const player = this.players.find(p => p.id === playerId);
    return player ? `${player.firstName} ${player.lastName}` : 'Unknown Player';
  }
  
  /**
   * Assign a player to the selected flight
   */
  assignPlayerToFlight() {
    if (!this.selectedPlayerId || !this.selectedFlightId) return;
    
    this.loading = true;
    
    const assignment: PlayerFlightAssignment = {
      playerId: this.selectedPlayerId,
      flightId: this.selectedFlightId,
      isFlightLeader: this.isFlightLeader,
      handicapAtAssignment: this.playerHandicap || 0
    };
    
    this.playerFlightAssignmentService.addAssignment(assignment).subscribe({
      next: (savedAssignment: PlayerFlightAssignment) => {
        // Add the new assignment to the list
        this.flightAssignments.push(savedAssignment);
        
        // Reset form
        this.selectedPlayerId = null;
        this.isFlightLeader = false;
        this.playerHandicap = null;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error assigning player to flight:', error);
        this.error = 'Failed to assign player to flight. Please try again.';
        this.loading = false;
      }
    });
  }
  
  /**
   * Toggle flight leader status for a player assignment
   */
  toggleFlightLeader(assignment: PlayerFlightAssignment) {
    this.loading = true;
    
    const updatedAssignment = { 
      ...assignment, 
      isFlightLeader: !assignment.isFlightLeader 
    };
    
    this.playerFlightAssignmentService.updateAssignment(updatedAssignment).subscribe({
      next: () => {
        // Update the assignment in the local list
        const index = this.flightAssignments.findIndex(a => a.id === assignment.id);
        if (index !== -1) {
          this.flightAssignments[index] = updatedAssignment;
        }
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error updating flight leader status:', error);
        this.error = 'Failed to update flight leader status. Please try again.';
        this.loading = false;
      }
    });
  }
  
  /**
   * Remove a player from a flight
   */
  removeAssignment(assignmentId: number) {
    this.loading = true;
    
    this.playerFlightAssignmentService.removeAssignment(assignmentId).subscribe({
      next: () => {
        // Remove the assignment from the local list
        this.flightAssignments = this.flightAssignments.filter(a => a.id !== assignmentId);
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error removing player from flight:', error);
        this.error = 'Failed to remove player from flight. Please try again.';
        this.loading = false;
      }
    });
  }
}
