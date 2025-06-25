import { Injectable } from '@angular/core';
import {
  HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Debug logging for cookie-based authentication
    console.log(`🔍 JWT Interceptor - URL: ${req.url}`);
    console.log(`🔍 JWT Interceptor - Using HttpOnly cookies for authentication`);
    
    // Clone the request to include credentials (cookies)
    const cloned = req.clone({
      withCredentials: true // This tells Angular to include cookies
    });
    
    console.log(`🔍 JWT Interceptor - Request configured with credentials: ${cloned.withCredentials}`);
    
    return next.handle(cloned).pipe(
      catchError((error: any) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          // Redirect to sign-in page on 401 Unauthorized
          this.router.navigate(['/auth/sign-in']);
        }
        return throwError(() => error);
      })
    );
  }
}
