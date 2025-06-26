import { Injectable } from '@angular/core';
import { CanActivateFn, Router, UrlTree, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { inject } from '@angular/core';
import { UserProfileService } from '../services/user-profile.service';
import { AuthService } from '../services/auth.service';
import { map, catchError, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';

export const adminGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const userProfileService = inject(UserProfileService);
  const authService = inject(AuthService);
  const router = inject(Router);

  // First check if user is authenticated
  return authService.isAuthenticatedAsync().pipe(
    switchMap((isAuth) => {
      if (!isAuth) {
        return of(router.createUrlTree(['/auth/sign-in'], { queryParams: { returnUrl: state.url } }));
      }

      // Check if profile is loaded, if not load it
      const currentProfile = userProfileService.getProfile();
      if (currentProfile) {
        if (currentProfile.isAdmin) {
          return of(true);
        } else {
          return of(router.createUrlTree(['/dashboard'], { queryParams: { error: 'unauthorized' } }));
        }
      } else {
        // Load profile if not available
        return userProfileService.loadProfile().pipe(
          map((profile) => {
            if (profile.isAdmin) {
              return true;
            } else {
              return router.createUrlTree(['/dashboard'], { queryParams: { error: 'unauthorized' } });
            }
          }),
          catchError(() => of(router.createUrlTree(['/dashboard'], { queryParams: { error: 'unauthorized' } })))
        );
      }
    }),
    catchError(() => of(router.createUrlTree(['/auth/sign-in'], { queryParams: { returnUrl: state.url } })))
  );
};
