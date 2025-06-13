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
  seasonFlights: Flight[] = [];
  flightAssignments: PlayerFlightAssignment[] = [];
  players: Player[] = [];
  playersWithHandicap: (Player & { handicap?: number })[] = [];
  
  // Selected data
  selectedSeasonId: number | null = null;
  selectedFlightId: number | null = null;
  selectedPlayerId: number | null = null;
  
  // UI state
  showSeasonForm = false;
  showFlightForm = false;
  showPlayerAssignmentModal = false;
  isLoading = false;
  error: string | null = null;
  isEditMode = false;
  editingSeasonId: number | null = null;
  editingFlightId: number | null = null;
  isFlightLeader: boolean = false;
  playerHandicap: number | null = null;

  get selectedSeason(): Season | undefined {
    return this.seasons.find(season => season.id === this.selectedSeasonId);
  }

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
      isActive: [true]
    });
  }

  ngOnInit() {
    this.loadPlayers();
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
      if (this.editingSeasonId) {
        const idx = this.seasons.findIndex(s => s.id === this.editingSeasonId);
        if (idx !== -1) {
          this.seasons[idx] = { ...formValue, id: this.editingSeasonId };
        }
      } else {
        const newId = Math.max(...this.seasons.map(s => s.id), 0) + 1;
        this.seasons.push({ ...formValue, id: newId });
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
    this.seasonForm.reset();
  }

  // Flight Management
  loadSeasonFlights(seasonId: number) {
    if (!seasonId) return;
    
    this.isLoading = true;
    this.error = null;
    
    // Mock implementation - creates example flights for the selected season
    setTimeout(() => {
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
      this.isLoading = false;
    }, 500);
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
      startTime: '08:00',
      isActive: true
    });
  }

  editFlight(flight: Flight) {
    this.showFlightForm = true;
    this.editingFlightId = flight.id!;
    
    const formattedDate = flight.date ? flight.date.split('T')[0] : '';
    const formattedTime = flight.startTime ? flight.startTime.substring(0, 5) : '08:00';
    
    this.flightForm.patchValue({
      ...flight,
      date: formattedDate,
      startTime: formattedTime
    });
  }

  saveFlight() {
    if (this.flightForm.valid && this.selectedSeasonId) {
      const flightData = this.flightForm.value;
      this.isLoading = true;
      
      const flightPayload: Flight = {
        ...flightData,
        date: flightData.date,
        startTime: flightData.startTime + ':00',
        seasonId: this.selectedSeasonId
      };
      
      setTimeout(() => {
        if (this.editingFlightId) {
          const seasonFlightIndex = this.seasonFlights.findIndex(f => f.id === this.editingFlightId);
          if (seasonFlightIndex !== -1) {
            this.seasonFlights[seasonFlightIndex] = { 
              ...flightPayload, 
              id: this.editingFlightId,
              updatedAt: new Date().toISOString() 
            };
          }
        } else {
          const newId = Math.max(...this.seasonFlights.map(f => f.id || 0)) + 1;
          const newFlight: Flight = {
            ...flightPayload,
            id: newId,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString()
          };
          this.seasonFlights.push(newFlight);
        }
        
        this.resetFlightForm();
        this.isLoading = false;
      }, 500);
    }
  }

  deleteFlight(id: number) {
    if (confirm('Are you sure you want to delete this flight?')) {
      this.seasonFlights = this.seasonFlights.filter(f => f.id !== id);
      if (this.selectedFlightId === id) {
        this.selectedFlightId = null;
        this.flightAssignments = [];
      }
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

  // Player Flight Assignment
  selectFlight(flight: Flight) {
    this.selectedFlightId = flight.id!;
    this.loadFlightAssignments(flight.id!);
  }

  loadFlightAssignments(flightId: number) {
    this.isLoading = true;
    this.error = null;
    
    this.selectedPlayerId = null;
    this.isFlightLeader = false;
    this.playerHandicap = null;
    
    this.playerFlightAssignmentService.getAssignmentsByFlight(flightId).subscribe({
      next: (assignments: PlayerFlightAssignment[]) => {
        this.flightAssignments = assignments;
        this.loadPlayersWithHandicap(flightId);
      },
      error: (error: any) => {
        console.error('Error loading flight assignments:', error);
        this.error = 'Failed to load flight assignments. Please try again.';
        this.isLoading = false;
      }
    });
  }

  loadPlayersWithHandicap(flightId: number) {
    this.playerFlightAssignmentService.getPlayersWithHandicap(flightId, this.players).subscribe({
      next: (players) => {
        this.playersWithHandicap = players;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading player handicaps:', error);
        this.isLoading = false;
      }
    });
  }

  onPlayerSelected() {
    if (this.selectedPlayerId) {
      const selectedPlayer = this.playersWithHandicap.find(p => p.id === this.selectedPlayerId);
      this.playerHandicap = selectedPlayer?.handicap || null;
    } else {
      this.playerHandicap = null;
    }
  }

  getSelectedFlightName(): string {
    if (!this.selectedFlightId) return '';
    const flight = this.seasonFlights.find(f => f.id === this.selectedFlightId);
    return flight ? flight.name : '';
  }

  getAvailablePlayers(): Player[] {
    if (!this.selectedFlightId) return [];
    
    const assignedPlayerIds = this.flightAssignments.map(a => a.playerId);
    return this.players.filter(p => !assignedPlayerIds.includes(p.id!));
  }

  getPlayerNameById(playerId: number): string {
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
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error updating flight leader status:', error);
        this.error = 'Failed to update flight leader status. Please try again.';
        this.isLoading = false;
      }
    });
  }

  removeAssignment(assignmentId: number) {
    this.isLoading = true;
    
    this.playerFlightAssignmentService.removeAssignment(assignmentId).subscribe({
      next: () => {
        this.flightAssignments = this.flightAssignments.filter(a => a.id !== assignmentId);
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error removing player from flight:', error);
        this.error = 'Failed to remove player from flight. Please try again.';
        this.isLoading = false;
      }
    });
  }
}
