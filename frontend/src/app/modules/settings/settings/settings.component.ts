import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlayersSettingsComponent } from '../components/players-settings/players-settings.component';
import { SeasonsSettingsComponent } from '../components/seasons-settings/seasons-settings.component';
import { SchedulingSettingsComponent } from '../components/scheduling-settings/scheduling-settings.component';
import { WeekManagementComponent } from '../../scoring/components/week-management/week-management.component';
import { ScoreEntryComponent } from '../../scoring/components/score-entry/score-entry.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule, 
    PlayersSettingsComponent, 
    SeasonsSettingsComponent,
    SchedulingSettingsComponent,
    WeekManagementComponent,
    ScoreEntryComponent
  ],
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent {
  activeTab: string = 'players';

  setActiveTab(tab: string) {
    this.activeTab = tab;
  }
}
