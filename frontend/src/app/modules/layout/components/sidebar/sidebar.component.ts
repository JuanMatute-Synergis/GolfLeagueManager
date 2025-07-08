import { NgClass, NgIf, CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import packageJson from '../../../../../../package.json';
import { MenuService } from '../../services/menu.service';
import { LeagueNameService } from '../../../../core/services/league-name.service';
import { SidebarMenuComponent } from './sidebar-menu/sidebar-menu.component';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css'],
  imports: [CommonModule, NgClass, NgIf, RouterModule, AngularSvgIconModule, SidebarMenuComponent],
})
export class SidebarComponent implements OnInit {
  public appJson: any = packageJson;
  leagueName$: Observable<string>;

  constructor(
    public menuService: MenuService,
    private leagueNameService: LeagueNameService
  ) {
    this.leagueName$ = this.leagueNameService.leagueName$;
  }

  ngOnInit(): void { }

  public toggleSidebar() {
    this.menuService.toggleSidebar();
  }
}
