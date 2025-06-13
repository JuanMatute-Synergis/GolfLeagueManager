import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PlayerService, Player } from '../../services/player.service';

@Component({
  selector: 'app-players-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './players-settings.component.html',
  styleUrls: ['./players-settings.component.css']
})
export class PlayersSettingsComponent implements OnInit {
  // Forms
  playerForm: FormGroup;
  
  // Data
  players: Player[] = [];
  
  // UI State
  showPlayerForm = false;
  isLoading = false;
  error: string | null = null;
  isEditMode = false;
  editingPlayerId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private playerService: PlayerService
  ) {
    this.playerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.loadPlayers();
  }

  loadPlayers() {
    this.isLoading = true;
    this.error = null;
    
    this.playerService.getPlayers().subscribe({
      next: (players) => {
        this.players = players;
        this.isLoading = false;
      },
      error: (error) => {
        this.error = 'Failed to load players. Please try again.';
        this.isLoading = false;
        console.error('Error loading players:', error);
      }
    });
  }

  showAddPlayerForm() {
    this.showPlayerForm = true;
    this.isEditMode = false;
    this.playerForm.reset();
  }

  editPlayer(player: Player) {
    this.showPlayerForm = true;
    this.isEditMode = true;
    this.editingPlayerId = player.id!;
    this.playerForm.patchValue(player);
  }

  savePlayer() {
    if (this.playerForm.valid) {
      const playerData = this.playerForm.value;
      this.isLoading = true;
      this.error = null;
      
      if (this.editingPlayerId) {
        // Update existing player
        const playerToUpdate = { ...playerData, id: this.editingPlayerId };
        this.playerService.updatePlayer(playerToUpdate).subscribe({
          next: () => {
            this.loadPlayers();
            this.resetForm();
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error updating player:', error);
            this.error = 'Failed to update player. Please try again.';
            this.isLoading = false;
          }
        });
      } else {
        // Add new player
        this.playerService.addPlayer(playerData).subscribe({
          next: () => {
            this.loadPlayers();
            this.resetForm();
            this.isLoading = false;
          },
          error: (error: any) => {
            console.error('Error adding player:', error);
            this.error = 'Failed to add player. Please try again.';
            this.isLoading = false;
          }
        });
      }
    }
  }

  deletePlayer(id: number) {
    if (confirm('Are you sure you want to delete this player?')) {
      this.isLoading = true;
      this.error = null;
      
      this.playerService.deletePlayer(id).subscribe({
        next: () => {
          this.loadPlayers();
          this.isLoading = false;
        },
        error: (error: any) => {
          console.error('Error deleting player:', error);
          this.error = 'Failed to delete player. Please try again.';
          this.isLoading = false;
        }
      });
    }
  }

  resetForm() {
    this.showPlayerForm = false;
    this.isEditMode = false;
    this.editingPlayerId = null;
    this.playerForm.reset();
  }
}
