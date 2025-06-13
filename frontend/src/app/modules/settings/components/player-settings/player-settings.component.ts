import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PlayerService, Player } from '../../services/player.service';

@Component({
  selector: 'app-player-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './player-settings.component.html'
})
export class PlayerSettingsComponent implements OnInit {
  // Forms
  playerForm: FormGroup;
  
  // Data
  players: Player[] = [];
  
  // UI State
  showPlayerForm = false;
  editingPlayerId: number | null = null;
  loading = false;
  error: string | null = null;

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

  ngOnInit(): void {
    this.loadPlayers();
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

  showAddPlayerForm() {
    this.showPlayerForm = true;
    this.editingPlayerId = null;
    this.playerForm.reset();
  }

  editPlayer(player: Player) {
    this.showPlayerForm = true;
    this.editingPlayerId = player.id!;
    this.playerForm.patchValue(player);
  }

  savePlayer() {
    if (this.playerForm.valid) {
      const playerData = this.playerForm.value;
      this.loading = true;
      this.error = null;
      
      if (this.editingPlayerId) {
        // Update existing player
        const playerToUpdate = { ...playerData, id: this.editingPlayerId };
        this.playerService.updatePlayer(playerToUpdate).subscribe({
          next: () => {
            this.loadPlayers(); // Reload the list
            this.resetForm();
            this.loading = false;
          },
          error: (error: any) => {
            console.error('Error updating player:', error);
            this.error = 'Failed to update player. Please try again.';
            this.loading = false;
          }
        });
      } else {
        // Add new player
        this.playerService.addPlayer(playerData).subscribe({
          next: () => {
            this.loadPlayers(); // Reload the list
            this.resetForm();
            this.loading = false;
          },
          error: (error: any) => {
            console.error('Error adding player:', error);
            this.error = 'Failed to add player. Please try again.';
            this.loading = false;
          }
        });
      }
    }
  }

  deletePlayer(id: number) {
    if (confirm('Are you sure you want to delete this player?')) {
      this.loading = true;
      this.error = null;
      
      this.playerService.deletePlayer(id).subscribe({
        next: () => {
          this.loadPlayers(); // Reload the list
          this.loading = false;
        },
        error: (error: any) => {
          console.error('Error deleting player:', error);
          this.error = 'Failed to delete player. Please try again.';
          this.loading = false;
        }
      });
    }
  }

  resetForm() {
    this.showPlayerForm = false;
    this.editingPlayerId = null;
    this.playerForm.reset();
  }

  cancelForm() {
    this.resetForm();
  }
}
