import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { DecodedToken, decodeToken } from '../models';

export const AuthGuard: CanActivateFn = (route) => {
  const router = inject(Router);
  const cookieService = inject(CookieService);
  const token = cookieService.get('authToken');

  if (token) {
    const decodedToken: DecodedToken | null = decodeToken(token);

    if (decodedToken && decodedToken.exp * 1000 > Date.now()) {
      const requiredRole = route.data?.['role'];

      if (requiredRole) {
        if (decodedToken['role'].toLowerCase() === requiredRole.toLowerCase()) {
          return true;
        } else {
          console.warn('Unauthorized: Role mismatch');
          router.navigate(['/unauthorized']);
          return false;
        }
      }

      return true; 
    } else {
      console.warn('Token expired');
      cookieService.delete('authToken');
      router.navigate(['/login']);
      return false;
    }
  } else {
    console.warn('No token found');
    router.navigate(['/login']);
    return false;
  }
};
