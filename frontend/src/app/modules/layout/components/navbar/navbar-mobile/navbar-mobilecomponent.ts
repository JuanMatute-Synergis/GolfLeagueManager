import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MenuService } from '../../../services/menu.service';
import { LeagueNameService } from '../../../../../core/services/league-name.service';
import { NavbarMobileMenuComponent } from './navbar-mobile-menu/navbar-mobile-menu.component';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { NgClass } from '@angular/common';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-navbar-mobile',
  standalone: true,
  templateUrl: './navbar-mobile.component.html',
  styleUrls: ['./navbar-mobile.component.css'],
  imports: [CommonModule, NgClass, AngularSvgIconModule, NavbarMobileMenuComponent],
})
export class NavbarMobileComponent implements OnInit {
  leagueName$: Observable<string>;

  constructor(
    public menuService: MenuService,
    private leagueNameService: LeagueNameService
  ) {
    this.leagueName$ = this.leagueNameService.leagueName$;
  }

  ngOnInit(): void {}

  public toggleMobileMenu(): void {
    this.menuService.showMobileMenu = false;
  }
}
