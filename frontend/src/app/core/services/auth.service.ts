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

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = '/api/auth';
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
}
