import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { Observable } from 'rxjs';
import { LeagueNameService } from '../../core/services/league-name.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-auth',
  standalone: true,
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css'],
  imports: [AngularSvgIconModule, RouterOutlet, CommonModule],
})
export class AuthComponent implements OnInit {
  leagueName$: Observable<string>;

  constructor(private leagueNameService: LeagueNameService) {
    this.leagueName$ = this.leagueNameService.leagueName$;
  }

  ngOnInit(): void {}
}
