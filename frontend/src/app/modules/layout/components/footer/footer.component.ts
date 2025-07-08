import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeagueNameService } from '../../../../core/services/league-name.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css'],
  standalone: true,
  imports: [CommonModule],
})
export class FooterComponent implements OnInit {
  public year: number = new Date().getFullYear();
  leagueName$: Observable<string>;

  constructor(private leagueNameService: LeagueNameService) {
    this.leagueName$ = this.leagueNameService.leagueName$;
  }

  ngOnInit(): void {}
}
