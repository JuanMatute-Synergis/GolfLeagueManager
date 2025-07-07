import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlayerService, Player } from '../../../settings/services/player.service';

@Component({
  selector: 'app-players-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './players-list.component.html',
  styleUrls: ['./players-list.component.css']
})
export class PlayersListComponent implements OnInit {
  // Data
  players: Player[] = [];

  // UI State
  isLoading = false;
  error: string | null = null;

  constructor(
    private playerService: PlayerService
  ) { }

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

  dismissError() {
    this.error = null;
  }
}
