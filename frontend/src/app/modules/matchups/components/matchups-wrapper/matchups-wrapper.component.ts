import { Component } from '@angular/core';
import { ScoreEntryComponent } from '../../../scoring/components/score-entry/score-entry.component';

/**
 * Wrapper component for the Matchups page.
 * Uses the ScoreEntryComponent in read-only 'matchups' mode to display matchup results.
 */
@Component({
  selector: 'app-matchups-wrapper',
  standalone: true,
  imports: [ScoreEntryComponent],
  template: `<app-score-entry mode="matchups"></app-score-entry>`
})
export class MatchupsWrapperComponent {
}
