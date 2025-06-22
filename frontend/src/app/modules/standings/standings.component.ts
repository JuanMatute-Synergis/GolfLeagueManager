import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface PlayerStanding {
  id: string;
  name: string;
  gross: number | null;
  net: number | null;
  average: number | null;
  weekPoints: number | null;
  accumPoints: number | null;
}

interface FlightStanding {
  id: string;
  name: string;
  players: PlayerStanding[];
}

@Component({
  selector: 'app-standings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './standings.component.html',
  styleUrls: ['./standings.component.css']
})
export class StandingsComponent implements OnInit {
  weeks: { id: string, name: string }[] = [];
  selectedWeek: string = '';
  flights: FlightStanding[] = [];
  loading = false;
  error: string | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadWeeks();
  }

  loadWeeks() {
    this.loading = true;
    this.http.get<any[]>('/api/weeks').subscribe({
      next: (weeks) => {
        this.weeks = weeks.map(w => ({ id: w.id, name: w.name }));
        this.selectedWeek = this.weeks[this.weeks.length - 1]?.id;
        this.loadStandings();
      },
      error: err => {
        this.error = 'Failed to load weeks';
        this.loading = false;
      }
    });
  }

  loadStandings() {
    if (!this.selectedWeek) return;
    this.loading = true;
    this.http.get<{ flights: FlightStanding[] }>(`/api/standings/weekly?weekId=${this.selectedWeek}`).subscribe({
      next: (data) => {
        this.flights = data.flights;
        this.loading = false;
      },
      error: err => {
        this.error = 'Failed to load standings';
        this.loading = false;
      }
    });
  }
}
