import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { environment } from '../../environments/environment';
import { BehaviorSubject,catchError, map, Observable, of,switchMap, tap,throwError,} from 'rxjs';
import { ImageService } from './image.service';
import { CookieService } from 'ngx-cookie-service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { UserRegistration, UserLogin, UserProfile, LoginResponse, AuthTokenPayload,} from '../models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiBaseUrl}/Auth`;
  private profilePictureUrl: string | null = null;
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  constructor(
    private http: HttpClient,
    private router: Router,
    private imageService: ImageService,
    private cookieService: CookieService
  ) {
    this.checkAuthState();
  }

  // Check authstate
  private checkAuthState(): void {
    const token = this.cookieService.get('authToken');
    const username = this.cookieService.get('username');
    if (token && username && !this.isTokenExpired(token)) {
      this.isAuthenticatedSubject.next(true);
    } else {
      this.isAuthenticatedSubject.next(false);
      this.clearAuthCookies();
    }
  }

  // Check if token is expired
  private isTokenExpired(token: string): boolean {
    try {
      const payload: AuthTokenPayload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 < Date.now();
    } catch (e) {
      return true;
    }
  }

  // Clear all authentication cookies
  private clearAuthCookies(): void {
    const cookiesToRemove = [
      'authToken',
      'profileImageUrl',
      'username',
      'roles',
      'fullName',
      'userId',
      'tempToken',
      'authEmail',
    ];
    cookiesToRemove.forEach((cookie) => this.cookieService.delete(cookie, '/'));
  }

  // Set authentication cookies securely
  private setAuthCookies(
    token: string,
    username: string,
    roles: string[],
    profilePictureUrl: string,
    fullName: string,
    userId: string
  ): void {
    const cookieOptions = {
      path: '/',
      secure: true,
      sameSite: 'Strict' as const,
      expires: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000), 
    };

    this.cookieService.set('authToken', token, cookieOptions);
    this.cookieService.set('username', username, cookieOptions);
    this.cookieService.set('roles', JSON.stringify(roles), cookieOptions);
    this.cookieService.set('profileImageUrl', profilePictureUrl, cookieOptions);
    this.cookieService.set('fullName', fullName, cookieOptions);
    this.cookieService.set('userId', userId, cookieOptions);
  }

  // Set the authentication state in the app
  setAuthenticationState(isAuthenticated: boolean): void {
    this.isAuthenticatedSubject.next(isAuthenticated);
  }

  // Get observable of auth state
  get authStatus$(): Observable<boolean> {
    return this.isAuthenticatedSubject.asObservable();
  }

  // Register student
  register(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, formData).pipe(
      tap({
        next: (response: any) => {
          if (response.profilePictureUrl) {
            this.setProfileImage(response.profilePictureUrl);
          }
        },
      }),
      catchError(this.handleError('register'))
    );
  }

  // register instructor
   registerInstructor(formData: FormData): Observable<any> {
    const token = this.cookieService.get('authToken'); // Use ngx-cookie-service to get the token
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    return this.http
      .post(`${this.apiUrl}/register-instructor`, formData, { headers })
      .pipe(catchError(this.handleError<any>('registerInstructor')));
  }

  // Method to create the FormData object for registration
  createRegistrationFormData(user: UserRegistration): FormData {
    const formData = new FormData();
    formData.append('UserName', user.UserName);
    formData.append('Email', user.Email);
    formData.append('Password', user.Password);
    formData.append('ConfirmPassword', user.ConfirmPassword);
    formData.append('FullName', user.FullName || '');
    if (user.ProfilePicture) {
      formData.append(
        'ProfilePicture',
        user.ProfilePicture,
        user.ProfilePicture.name
      );
    }
    return formData;
  }
  
  // Login
   login(user: UserLogin): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, user).pipe(
      tap((response: LoginResponse) => {
        // Handle 2FA case
        if (response.requires2FA && response.tempToken) {
          this.cookieService.set('tempToken', response.tempToken, {
            secure: true,
            sameSite: 'Strict',
            expires: 1,
          });
          return;
        }
  
        // Handle password reset case
        if (response.requiresPasswordChange && response.resetToken) {
          sessionStorage.setItem('resetToken', response.resetToken);
          sessionStorage.setItem('email', user.Email);
          this.router.navigate(['/reset-password']);
          return;
        }
  
        // Handle successful login
        if (response.token && response.username && response.roles) {
          this.handleSuccessfulLogin(response);
        }
      }),
      map((response: LoginResponse) => response),
      catchError((error: any) => {
        const handledError = this.handleError<LoginResponse>('login')(error);
        return throwError(() => handledError);
      })
    );
  }
  
  private handleSuccessfulLogin(response: LoginResponse): void {
    if (!response.token || !response.username || !response.roles) {
      throw new Error('Invalid login response - missing required fields');
    }
  
    const profilePictureUrl = 
      response.profilePictureUrl || '/images/profiles/profilepic.png';
  
    this.setAuthCookies(
      response.token,
      response.username,
      response.roles,
      this.imageService.getProfileImageUrl(profilePictureUrl),
      response.fullName || '',
      response.userId || ''
    );
  
    this.isAuthenticatedSubject.next(true);
    this.setProfileImage(profilePictureUrl);
  }

  // Logout method
  async logout(): Promise<void> {
    try {
      this.clearAuthCookies();
      sessionStorage.clear();
      this.isAuthenticatedSubject.next(false);
      sessionStorage.setItem('logoutSuccess', 'true'); 
      await this.router.navigate(['/home']);
    } catch (error) {
      console.error('Logout failed:', error);
    }
  }

  // Handle error
  private handleError<T>(operation = 'operation'): (error: any) => Observable<T> {
    return (error: any): Observable<T> => {
      let errorMessage = 'Something went wrong';
      
      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Error: ${error.error.message}`;
      } else if (error.error?.message) {
        // Server-side error with message
        errorMessage = error.error.message;
      } else if (error.message) {
        // Other error with message
        errorMessage = error.message;
      }
      console.error(`${operation} failed:`, error);
      return throwError(() => new Error(errorMessage));
    };
  }

  // Store profile image URL
  setProfileImage(url: string | null): void {
    const defaultImage = this.imageService.getProfileImageUrl('profilepic.png');
    const imageUrl = url ?? defaultImage;
    
    this.profilePictureUrl = imageUrl;
    this.cookieService.set('profileImageUrl', imageUrl, {
      secure: true,
      sameSite: 'Lax',
      path: '/',
    });
  }

  // Store username
  setUserName(username: string): void {
    this.cookieService.set('username', username, {
      secure: true,
      sameSite: 'Strict',
      path: '/',
    });
  }

  // Store email 
  setEmail(email: string): void {
    this.cookieService.set('authEmail', email, {
      secure: true,
      sameSite: 'Strict',
      path: '/',
    });
  }

  // Get profile image URL
  getProfileImage(): string | null {
    return (
      this.profilePictureUrl ??
      this.cookieService.get('profileImageUrl') ??
      null
    );
  }

  // Get email
  getEmail(): Observable<string> {
    const email = this.cookieService.get('authEmail');
    if (email) {
      return of(email);
    } else {
      return throwError(() => new Error('Email not found.'));
    }
  }

  // Get roles
  getRoles(): string[] {
    const roles = this.cookieService.get('roles');
    return roles ? JSON.parse(roles) : [];
  }

  // Change Password
  changePassword(
    currentPassword: string,
    newPassword: string
  ): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/change-password`, { currentPassword, newPassword })
      .pipe(catchError(this.handleError('changePassword')));
  }

  // Forgot Password
  forgotPassword(email: string): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/forgot-password`, { email })
      .pipe(catchError(this.handleError('forgotPassword')));
  }

  // Reset Password
  resetPassword(token: string, newPassword: string): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/reset-password`, { token, newPassword })
      .pipe(catchError(this.handleError('resetPassword')));
  }

  // verify current password
  verifyCurrentPassword(
    email: string,
    token: string,
    currentPassword: string
  ): Observable<any> {
    const url = `${this.apiUrl}/verify-password`;
    return this.http.post(url, { email, token, password: currentPassword });
  }

  // Verify 2FA when login
  verify2FALogin(email: string, code: string): Observable<LoginResponse> {
    const payload = {
      tempToken: this.cookieService.get('tempToken'),
      code: code,
      email: email,
    };

    return this.http
      .post<LoginResponse>(`${this.apiUrl}/verify-2fa-login`, payload)
      .pipe(
        tap((response) => {
          if (response.token && response.username && response.roles) {
            this.cookieService.delete('tempToken');

            const profilePictureUrl =
              response.profilePictureUrl || '/images/profiles/default.png';

            this.setAuthCookies(
              response.token,
              response.username,
              response.roles,
              profilePictureUrl,
              response.fullName || '',
              response.userId || ''
            );

            this.isAuthenticatedSubject.next(true);
            this.setProfileImage(profilePictureUrl);

            // Navigate based on role
            if (response.roles.includes('Instructor')) {
              this.router.navigate(['/instructor-dashboard'], {
                replaceUrl: true,
              });
            } else if (response.roles.includes('Admin')) {
              this.router.navigate(['/admin-dashboard'], { replaceUrl: true });
            } else {
              this.router.navigate(['/student-dashboard'], {
                replaceUrl: true,
              });
            }
          }
        }),
        catchError((error) => {
          return throwError(
            () =>
              new Error(
                error.error?.message ||
                  'An error occurred during 2FA verification.'
              )
          );
        })
      );
  }


  // Checking if user is authenticated
  isAuthenticated(): boolean {
    const token = this.cookieService.get('authToken');
    return (
      this.isAuthenticatedSubject.value &&
      !!token &&
      !this.isTokenExpired(token)
    );
  }

  // Enable 2FA
  enable2FA(): Observable<{ qrCode: string }> {
    // Replace the token retrieval method to use ngx-cookie-service
    const token = this.cookieService.get('authToken'); // Using ngx-cookie-service

    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    return this.http
      .post(`${this.apiUrl}/enable-2fa`, {}, { headers, responseType: 'blob' })
      .pipe(switchMap((blob) => this.convertBlobToBase64(blob)));
  }

  // Check 2FA Status
  check2FAStatus(): Observable<{ isEnabled: boolean; error?: string }> {
    const token = this.cookieService.get('authToken'); 
    if (!token) {
      return of({
        isEnabled: false,
        error: 'No authentication token found',
      });
    }
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http
      .get<{ isEnabled: boolean }>(`${this.apiUrl}/check-2fa-status`, {
        headers,
      })
      .pipe(
        map((response) => ({
          isEnabled: response.isEnabled,
        })),
        catchError((error) => {
          console.error('Error checking 2FA status:', error);
          return of({
            isEnabled: false,
            error: error.error?.message || 'Failed to check 2FA status',
          });
        })
      );
  }

  // Convert Blob to Base64
  private convertBlobToBase64(blob: Blob): Observable<{ qrCode: string }> {
    return new Observable((observer) => {
      const reader = new FileReader();
      reader.readAsDataURL(blob);
      reader.onloadend = () => {
        const base64data = reader.result as string;
        const qrCode = base64data.replace(/^data:image\/png;base64,/, '');
        observer.next({ qrCode });
        observer.complete();
      };
      reader.onerror = (error) => observer.error(error);
    });
  }

  // Verify 2FA when enabling
  verify2FA(
    email: string,
    code: string
  ): Observable<{ token: string; roles: string[] }> {
    return this.http
      .post<{ token: string; roles: string[] }>(`${this.apiUrl}/verify-2fa`, {
        email,
        code,
      })
      .pipe(
        catchError((error) => {
          return throwError(
            () =>
              new Error(
                error.error?.message || 'An error occurred while verifying 2FA'
              )
          );
        })
      );
  }

  // Disable 2FA
  disable2FA(): Observable<any> {
    const token = this.cookieService.get('authToken');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.post(`${this.apiUrl}/disable-2fa`, {}, { headers }).pipe(
      catchError((error) => {
        console.error('Error disabling 2FA:', error);
        return throwError(
          () => new Error(error?.error?.message || 'Failed to disable 2FA.')
        );
      })
    );
  }
}

