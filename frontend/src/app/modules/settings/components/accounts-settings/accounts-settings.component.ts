import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService, PlayerAccountStatus, CreateUserForPlayerRequest, UpdateUserAccountRequest } from 'src/app/core/services/auth.service';
import { UserProfileService } from 'src/app/core/services/user-profile.service';
import { PlayerService, Player } from '../../services/player.service';

@Component({
  selector: 'app-accounts-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './accounts-settings.component.html',
  styleUrls: ['./accounts-settings.component.css']
})
export class AccountsSettingsComponent implements OnInit {
  // Account management properties
  players: PlayerAccountStatus[] = [];
  errorMessage = '';
  successMessage = '';

  // Player management properties
  playerForm: FormGroup;
  showPlayerForm = false;
  isLoading = false;
  isEditMode = false;
  editingPlayerId: string | null = null;

  // Account form properties
  accountForm: FormGroup;
  showAccountForm = false;
  isAccountEditMode = false;
  isPasswordResetMode = false;
  editingAccountPlayerId: string | null = null;

  // View state
  activeView: 'list' | 'accounts' = 'list';

  constructor(
    private authService: AuthService,
    private playerService: PlayerService,
    private userProfileService: UserProfileService,
    private fb: FormBuilder
  ) {
    this.playerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required]
    });

    this.accountForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      isAdmin: [false]
    });
  }

  ngOnInit() {
    this.loadPlayers();
  }

  setActiveView(view: 'list' | 'accounts') {
    this.activeView = view;
    if (view === 'accounts') {
      this.loadPlayersWithAccountStatus();
    } else {
      this.loadPlayers();
    }
  }

  // Account Management Methods
  loadPlayersWithAccountStatus() {
    this.authService.getPlayersWithAccountStatus().subscribe({
      next: (players) => {
        console.log('Players with account status:', players); // Debug log
        this.players = players;
      },
      error: (err) => {
        console.error('Error loading players with account status:', err); // Debug log
        this.errorMessage = 'Failed to load players.';
      }
    });
  }

  // This method is now handled by the modal form
  // createAccount(player: PlayerAccountStatus) - replaced by showCreateAccountForm()
  // showResetPassword, hideResetPassword, submitResetPassword - replaced by showResetPasswordForm()

  // Player Management Methods
  loadPlayers() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.playerService.getPlayers().subscribe({
      next: (players) => {
        // Convert Player[] to PlayerAccountStatus[] for unified handling
        this.players = players.map(p => ({
          playerId: p.id!,
          firstName: p.firstName,
          lastName: p.lastName,
          imageUrl: undefined, // Player interface doesn't have imageUrl
          email: p.email,
          hasUserAccount: false,
          username: undefined,
          userId: undefined
        }));
        
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load players. Please try again.';
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

  editPlayer(player: PlayerAccountStatus) {
    this.showPlayerForm = true;
    this.isEditMode = true;
    this.editingPlayerId = player.playerId;
    this.playerForm.patchValue({
      firstName: player.firstName,
      lastName: player.lastName,
      email: player.email,
      phone: '' // Will need to add phone to PlayerAccountStatus or fetch separately
    });
  }

  savePlayer() {
    if (this.playerForm.valid) {
      const playerData = this.playerForm.value;
      this.isLoading = true;
      this.errorMessage = '';
      
      if (this.editingPlayerId) {
        // Update existing player
        const playerToUpdate = { ...playerData, id: this.editingPlayerId };
        this.playerService.updatePlayer(playerToUpdate).subscribe({
          next: () => {
            this.loadPlayers();
            this.resetForm();
            this.isLoading = false;
            this.successMessage = 'Player updated successfully!';
          },
          error: (error: any) => {
            console.error('Error updating player:', error);
            this.errorMessage = 'Failed to update player. Please try again.';
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
            this.successMessage = 'Player added successfully!';
          },
          error: (error: any) => {
            console.error('Error adding player:', error);
            this.errorMessage = 'Failed to add player. Please try again.';
            this.isLoading = false;
          }
        });
      }
    }
  }

  deletePlayer(id: string) {
    if (confirm('Are you sure you want to delete this player?')) {
      this.isLoading = true;
      this.errorMessage = '';
      
      this.playerService.deletePlayer(id).subscribe({
        next: () => {
          this.loadPlayers();
          this.isLoading = false;
          this.successMessage = 'Player deleted successfully!';
        },
        error: (error: any) => {
          console.error('Error deleting player:', error);
          this.errorMessage = 'Failed to delete player. Please try again.';
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

  clearMessages() {
    this.errorMessage = '';
    this.successMessage = '';
  }

  // Debug method - can be called from browser console
  debugUserPlayerLinks() {
    console.log('=== DEBUG: Checking user-player links ===');
    
    this.authService.getDebugUsers().subscribe({
      next: (users) => {
        console.log('All users:', users);
      },
      error: (err) => console.error('Error getting users:', err)
    });

    this.authService.getDebugPlayers().subscribe({
      next: (players) => {
        console.log('All players:', players);
      },
      error: (err) => console.error('Error getting players:', err)
    });

    this.authService.getPlayersWithAccountStatus().subscribe({
      next: (playersWithStatus) => {
        console.log('Players with account status:', playersWithStatus);
        
        // Find any discrepancies
        const playersWithAccounts = playersWithStatus.filter(p => p.hasUserAccount);
        const playersWithoutAccounts = playersWithStatus.filter(p => !p.hasUserAccount);
        
        console.log('Players WITH accounts:', playersWithAccounts);
        console.log('Players WITHOUT accounts:', playersWithoutAccounts);
      },
      error: (err) => console.error('Error getting player account status:', err)
    });
  }

  // Method to manually link a user to a player (for fixing data issues)
  linkUserToPlayer(userId: string, playerId: string) {
    this.authService.linkUserToPlayer({ userId, playerId }).subscribe({
      next: () => {
        this.successMessage = 'User linked to player successfully!';
        this.loadPlayersWithAccountStatus();
      },
      error: (err) => {
        this.errorMessage = 'Failed to link user to player: ' + (err.error || err.message);
      }
    });
  }

  // Note: getNewAccountData method removed as it's no longer needed with modal forms

  // Account Form Methods
  showCreateAccountForm(player: PlayerAccountStatus) {
    this.showAccountForm = true;
    this.isAccountEditMode = false;
    this.editingAccountPlayerId = player.playerId;
    
    // For creating, password is required
    this.accountForm = this.fb.group({
      username: ['', Validators.required],
      password: ['golfpassword', Validators.required],
      isAdmin: [false]
    });
  }

  showEditAccountForm(player: PlayerAccountStatus) {
    if (player.userId) {
      this.showAccountForm = true;
      this.isAccountEditMode = true;
      this.editingAccountPlayerId = player.playerId;
      
      // For editing, we don't require password
      this.accountForm = this.fb.group({
        username: [player.username || '', Validators.required],
        password: [''], // Password not required for editing
        isAdmin: [player.isAdmin || false]
      });
    }
  }

  showResetPasswordForm(player: PlayerAccountStatus) {
    if (player.userId) {
      this.showAccountForm = true;
      this.isAccountEditMode = false;
      this.isPasswordResetMode = true;
      this.editingAccountPlayerId = player.playerId;
      
      // For password reset, only password is required
      this.accountForm = this.fb.group({
        username: [{ value: player.username || '', disabled: true }], // Disabled since we're not updating it
        password: ['', Validators.required],
        isAdmin: [{ value: player.isAdmin || false, disabled: true }] // Disabled since we're not updating it
      });
    }
  }

  // Getter to check if account form is valid based on mode
  get isAccountFormValid(): boolean {
    if (this.isPasswordResetMode) {
      // For password reset, only password is required
      return this.accountForm.get('password')?.valid ?? false;
    } else if (this.isAccountEditMode) {
      // For editing, only username is required
      return this.accountForm.get('username')?.valid ?? false;
    } else {
      // For creating, username and password are required
      return this.accountForm.valid;
    }
  }

  saveAccount() {
    if (this.isAccountFormValid && this.editingAccountPlayerId) {
      const formData = this.accountForm.value;
      this.isLoading = true;
      this.errorMessage = '';

      if (this.isPasswordResetMode) {
        // Reset password
        const player = this.players.find(p => p.playerId === this.editingAccountPlayerId);
        if (player && player.userId) {
          this.authService.resetUserPassword({ 
            userId: player.userId, 
            newPassword: formData.password 
          }).subscribe({
            next: () => {
              this.successMessage = 'Password reset successfully!';
              this.errorMessage = '';
              this.resetAccountForm();
              this.userProfileService.refreshProfile(); // Refresh profile in navbar
              this.isLoading = false;
            },
            error: (err) => {
              this.errorMessage = err.error || 'Failed to reset password.';
              this.successMessage = '';
              this.isLoading = false;
            }
          });
        }
      } else if (this.isAccountEditMode) {
        // Update existing account
        const player = this.players.find(p => p.playerId === this.editingAccountPlayerId);
        if (player && player.userId) {
          const request: UpdateUserAccountRequest = {
            userId: player.userId,
            username: formData.username,
            isAdmin: formData.isAdmin
          };
          
          this.authService.updateUserAccount(request).subscribe({
            next: () => {
              this.successMessage = 'Account updated successfully!';
              this.errorMessage = '';
              this.resetAccountForm();
              this.loadPlayersWithAccountStatus();
              this.userProfileService.refreshProfile(); // Refresh profile in navbar
              this.isLoading = false;
            },
            error: (err) => {
              this.errorMessage = err.error || 'Failed to update account.';
              this.successMessage = '';
              this.isLoading = false;
            }
          });
        }
      } else {
        // Create new account
        const req: CreateUserForPlayerRequest = {
          playerId: this.editingAccountPlayerId,
          username: formData.username,
          password: formData.password,
          isAdmin: formData.isAdmin
        };
        
        this.authService.createUserForPlayer(req).subscribe({
          next: () => {
            this.successMessage = 'Account created successfully!';
            this.errorMessage = '';
            this.resetAccountForm();
            this.loadPlayersWithAccountStatus();
            this.userProfileService.refreshProfile(); // Refresh profile in navbar
            this.isLoading = false;
          },
          error: (err) => {
            this.errorMessage = err.error || 'Failed to create account.';
            this.successMessage = '';
            this.isLoading = false;
          }
        });
      }
    }
  }

  resetAccountForm() {
    this.showAccountForm = false;
    this.isAccountEditMode = false;
    this.isPasswordResetMode = false;
    this.editingAccountPlayerId = null;
    this.accountForm.reset();
  }

}
