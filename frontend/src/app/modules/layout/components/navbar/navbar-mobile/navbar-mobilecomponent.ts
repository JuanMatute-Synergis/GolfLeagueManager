import { Component, OnInit } from '@angular/core';
import { MenuService } from '../../../services/menu.service';
import { NavbarMobileMenuComponent } from './navbar-mobile-menu/navbar-mobile-menu.component';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-navbar-mobile',
  standalone: true,
  templateUrl: './navbar-mobile.component.html',
  styleUrls: ['./navbar-mobile.component.css'],
  imports: [NgClass, AngularSvgIconModule, NavbarMobileMenuComponent],
})
export class NavbarMobileComponent implements OnInit {
  constructor(public menuService: MenuService) {}

  ngOnInit(): void {}

  public toggleMobileMenu(): void {
    this.menuService.showMobileMenu = false;
  }
}
