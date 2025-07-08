import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
}

export interface PlayerAccountStatus {
  playerId: string;
  firstName: string;
  lastName: string;
  imageUrl?: string;
  email: string;
  phone: string; // Add phone field
  hasUserAccount: boolean;
  username?: string;
  userId?: string;
  isAdmin?: boolean;
}

export interface CreateUserForPlayerRequest {
  playerId: string;
  username: string;
  password: string;
  isAdmin: boolean;
}

export interface ResetUserPasswordRequest {
  userId: string;
  newPassword: string;
}

export interface LinkUserToPlayerRequest {
  userId: string;
  playerId: string;
}

export interface UpdateUserAccountRequest {
  userId: string;
  username: string;
  isAdmin: boolean;
}

export interface UserProfile {
  username: string;
  isAdmin: boolean;
  player?: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    imageUrl?: string;
  };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = '/api/auth';
  private userAdminApiUrl = '/api/UserAdmin';
  private userKey = 'golf_user';
  private loggedIn$ = new BehaviorSubject<boolean>(this.hasValidSession());

  constructor(private http: HttpClient) {}

  login(data: LoginRequest): Observable<LoginResponse> {
    // Clear any previous user data before attempting login
    localStorage.removeItem(this.userKey);
    
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, data, { 
      withCredentials: true // This tells Angular to include cookies
    }).pipe(
      tap(res => {
        // Store username for display purposes
        localStorage.setItem(this.userKey, res.username);
        this.loggedIn$.next(true);
      })
    );
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout`, {}, { 
      withCredentials: true 
    }).pipe(
      tap(() => {
        localStorage.removeItem(this.userKey);
        this.loggedIn$.next(false);
      })
    );
  }

  getToken(): string | null {
    // Tokens are now stored in HttpOnly cookies, not accessible via JavaScript
    // This method is kept for compatibility but will return null
    return null;
  }

  getUsername(): string | null {
    return localStorage.getItem(this.userKey);
  }

  isLoggedIn(): Observable<boolean> {
    return this.loggedIn$.asObservable();
  }

  // Public method for guards/components
  // isAuthenticated(): boolean {
  //   // Instead of just checking localStorage, make a synchronous call to the server
  //   // (Note: Guards require a synchronous return, so this is a limitation of Angular's CanActivate)
  //   // For best UX, use an async guard or resolve, or update the guard to support Observable<boolean>.
  //   // Here, we fallback to localStorage for now, but you should use checkAuthStatus() in your app shell or on navigation events.
  //   return !!localStorage.getItem(this.userKey);
  // }

  // Async version for app shell or components
  isAuthenticatedAsync(): Observable<boolean> {
    return this.checkAuthStatus().pipe(
      // If the server returns authenticated: true, return true
      // Otherwise, return false
      tap((res: any) => {
        if (res && res.authenticated) {
          this.loggedIn$.next(true);
        } else {
          this.loggedIn$.next(false);
        }
      }),
      // Map to boolean
      // (You can use map from rxjs/operators if you want to return just boolean)
    );
  }

  private hasValidSession(): boolean {
    // Check if we have a username stored (indicates potential valid session)
    // The actual JWT validation will happen on the server via cookies
    return !!localStorage.getItem(this.userKey);
  }

  // Method to check authentication status with the server
  checkAuthStatus(): Observable<any> {
    return this.http.get(`${this.apiUrl}/status`, { 
      withCredentials: true 
    });
  }

  // Player account management methods (migrated from AdminService)
  getPlayersWithAccountStatus(): Observable<PlayerAccountStatus[]> {
    return this.http.get<PlayerAccountStatus[]>(`${this.userAdminApiUrl}/players-with-account-status`);
  }

  createUserForPlayer(request: CreateUserForPlayerRequest): Observable<any> {
    return this.http.post(`${this.userAdminApiUrl}/create-for-player`, request);
  }

  resetUserPassword(request: ResetUserPasswordRequest): Observable<any> {
    return this.http.post(`${this.userAdminApiUrl}/reset-password`, request);
  }

  linkUserToPlayer(request: LinkUserToPlayerRequest): Observable<any> {
    return this.http.post(`${this.userAdminApiUrl}/link-user-to-player`, request);
  }

  updateUserAccount(request: UpdateUserAccountRequest): Observable<any> {
    return this.http.post(`${this.userAdminApiUrl}/update-account`, request);
  }

  getDebugUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.userAdminApiUrl}/debug-users`);
  }

  getDebugPlayers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.userAdminApiUrl}/debug-players`);
  }

  getUserProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
  }
}
