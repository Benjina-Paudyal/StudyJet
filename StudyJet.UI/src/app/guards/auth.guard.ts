
import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { DecodedToken, decodeToken } from '../models';

// Route guard to check authentication and role
export const authGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const router = inject(Router);
  const cookieService = inject(CookieService);
  const token = cookieService.get('authToken');

  const customMessage = route.data?.['message'] || 'Please log in to continue.';

  if (token) {
    const decodedToken: DecodedToken | null = decodeToken(token);

    // Check if the token is valid and not expired
    if (decodedToken && decodedToken.exp * 1000 > Date.now()) {
      const requiredRole = route.data?.['role'];

      // Check if the user has the required role
      if (requiredRole) {
        if (decodedToken.role.toLowerCase() === requiredRole.toLowerCase()) {
          return true;
        } else {
          console.warn('Unauthorized: Role mismatch');
          router.navigate(['/unauthorized']);
          return false;
        }
      }
      return true;
    } else {
      alert('Your session has expired. Please log in again.');
      cookieService.delete('authToken');
      router.navigate(['/login']);
      return false;
    }
  } else {
    alert(customMessage);
    router.navigate(['/login']);
    return false;
  }
};











































// import { inject } from '@angular/core';
// import { Router, CanActivateFn } from '@angular/router';
// import { CookieService } from 'ngx-cookie-service';
// import { DecodedToken, decodeToken } from '../models';

// export const AuthGuard: CanActivateFn = (route) => {
//   const router = inject(Router);
//   const cookieService = inject(CookieService);
//   const token = cookieService.get('authToken');
//   const customMessage = route.data?.['message'] || 'Please log in to continue.';

//   if (token) {
//     const decodedToken: DecodedToken | null = decodeToken(token);

//     if (decodedToken && decodedToken.exp * 1000 > Date.now()) {
//       const requiredRole = route.data?.['role'];

//       if (requiredRole) {
//         if (decodedToken['role'].toLowerCase() === requiredRole.toLowerCase()) {
//           return true;
//         } else {
//           console.warn('Unauthorized: Role mismatch');
//           router.navigate(['/unauthorized']);
//           return false;
//         }
//       }

//       return true; 
//     } else {
//       console.warn('Token expired');
//       alert('Your session has expired. Please log in again.');
//       cookieService.delete('authToken');
//       router.navigate(['/login']);
//       return false;
//     }
//   } else {
//     console.warn('No token found');
//     alert('customMessage');
//     // router.navigate(['/login']);
//     return false;
//   }
// };
