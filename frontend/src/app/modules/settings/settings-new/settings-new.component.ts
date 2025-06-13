import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlayersSettingsComponent } from '../components/players-settings/players-settings.component';
import { SeasonsSettingsComponent } from '../components/seasons-settings/seasons-settings.component';
import { ScoringSettingsComponent } from '../components/scoring-settings/scoring-settings.component';
import { ScheduleSettingsComponent } from '../components/schedule-settings/schedule-settings.component';

@Component({
  selector: 'app-settings-new',
  imports: [
    CommonModule, 
    PlayersSettingsComponent, 
    SeasonsSettingsComponent,
    ScoringSettingsComponent,
    ScheduleSettingsComponent
  ],
  templateUrl: './settings-new.component.html',
  styleUrl: './settings-new.component.css'
})
export class SettingsNewComponent {
  activeTab: string = 'players';

  setActiveTab(tab: string) {
    this.activeTab = tab;
  }
}
