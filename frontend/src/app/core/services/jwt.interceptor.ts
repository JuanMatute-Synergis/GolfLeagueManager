import { Injectable } from '@angular/core';
import {
  HttpEvent, HttpInterceptor, HttpHandler, HttpRequest
} from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor() {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Debug logging for cookie-based authentication
    console.log(`🔍 JWT Interceptor - URL: ${req.url}`);
    console.log(`🔍 JWT Interceptor - Using HttpOnly cookies for authentication`);
    
    // Clone the request to include credentials (cookies)
    const cloned = req.clone({
      setHeaders: {
        // Remove any Authorization header since we're using cookies
        // 'Authorization': undefined  // This would actually set it to "undefined"
      },
      withCredentials: true // This tells Angular to include cookies
    });
    
    console.log(`🔍 JWT Interceptor - Request configured with credentials: ${cloned.withCredentials}`);
    
    return next.handle(cloned);
  }
}
