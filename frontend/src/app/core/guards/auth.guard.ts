import { Injectable } from '@angular/core';
import { CanActivateFn, Router, UrlTree, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

export const authGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  return authService.isAuthenticatedAsync().pipe(
    map((isAuth) => {
      if (isAuth) {
        return true;
      } else {
        return router.createUrlTree(['/auth/sign-in'], { queryParams: { returnUrl: state.url } });
      }
    }),
    catchError(() => of(router.createUrlTree(['/auth/sign-in'], { queryParams: { returnUrl: state.url } })))
  );
};
