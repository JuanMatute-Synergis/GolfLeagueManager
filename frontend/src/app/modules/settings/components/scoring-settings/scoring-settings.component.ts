import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

interface ScoringSystem {
  id?: string;
  name: string;
  pointsForWin: number;
  pointsForLoss: number;
  pointsForTie: number;
}

@Component({
  selector: 'app-scoring-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './scoring-settings.component.html',
  styleUrls: ['./scoring-settings.component.css']
})
export class ScoringSettingsComponent implements OnInit {
  scoringForm: FormGroup;
  scoringSystems: ScoringSystem[] = [];
  showScoringForm = false;
  editingScoringId: string | null = null;
  loading = false;
  error: string | null = null;

  constructor(private fb: FormBuilder) {
    this.scoringForm = this.fb.group({
      name: ['', Validators.required],
      pointsForWin: [0, [Validators.required, Validators.min(0)]],
      pointsForLoss: [0, [Validators.required, Validators.min(0)]],
      pointsForTie: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit() {
    this.loadScoringSystems();
  }

  loadScoringSystems() {
    this.loading = true;
    // Mock data for now
    setTimeout(() => {
      this.scoringSystems = [
        {
          id: '1',
          name: 'Standard Points',
          pointsForWin: 2,
          pointsForLoss: 0,
          pointsForTie: 1
        },
        {
          id: '2',
          name: 'Modified Stableford',
          pointsForWin: 3,
          pointsForLoss: 0,
          pointsForTie: 1
        }
      ];
      this.loading = false;
    }, 500);
  }

  showAddScoringForm() {
    this.showScoringForm = true;
    this.editingScoringId = null;
    this.scoringForm.reset({
      name: '',
      pointsForWin: 0,
      pointsForLoss: 0,
      pointsForTie: 0
    });
  }

  editScoring(scoring: ScoringSystem) {
    this.showScoringForm = true;
    this.editingScoringId = scoring.id!;
    this.scoringForm.patchValue({
      name: scoring.name,
      pointsForWin: scoring.pointsForWin,
      pointsForLoss: scoring.pointsForLoss,
      pointsForTie: scoring.pointsForTie
    });
  }

  saveScoring() {
    if (this.scoringForm.invalid) {
      return;
    }

    this.loading = true;
    const formValue = this.scoringForm.value;

    setTimeout(() => {
      if (this.editingScoringId) {
        // Update existing scoring system
        const index = this.scoringSystems.findIndex(s => s.id === this.editingScoringId);
        if (index !== -1) {
          this.scoringSystems[index] = {
            ...this.scoringSystems[index],
            ...formValue
          };
        }
      } else {
        // Add new scoring system
        const newScoring: ScoringSystem = {
          id: Date.now().toString(),
          ...formValue
        };
        this.scoringSystems.push(newScoring);
      }

      this.resetForms();
      this.loading = false;
    }, 500);
  }

  deleteScoring(id: string) {
    if (confirm('Are you sure you want to delete this scoring system?')) {
      this.loading = true;
      setTimeout(() => {
        this.scoringSystems = this.scoringSystems.filter(s => s.id !== id);
        this.loading = false;
      }, 300);
    }
  }

  resetForms() {
    this.showScoringForm = false;
    this.editingScoringId = null;
    this.scoringForm.reset();
    this.error = null;
  }
}
