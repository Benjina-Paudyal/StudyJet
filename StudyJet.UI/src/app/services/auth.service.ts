import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { environment } from '../../environments/environment';
import { BehaviorSubject, catchError, map, Observable, of, ReplaySubject, switchMap, tap, throwError, } from 'rxjs';
import { ImageService } from './image.service';
import { CookieService } from 'ngx-cookie-service';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { UserRegistration, UserLogin, LoginResponse, AuthTokenPayload, UserRegistrationResponse, ForgotPasswordResponse, ResetPasswordResponse, InstructorRegistrationResponse, ChangePasswordResponse, VerifyPasswordResponse, Disable2FAResponse, } from '../models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiBaseUrl}/Auth`;
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
    private imageService: ImageService,
    private cookieService: CookieService
  ) {
    this.checkAuthState();
  }

  // Check authstate on app initialization
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
      const payload: AuthTokenPayload = JSON.parse(atob(token.split('.')[1])); // Decode the JWT payload
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

  // Register student
  register(formData: FormData): Observable<UserRegistrationResponse> {
    return this.http.post<UserRegistrationResponse>(`${this.apiUrl}/register`, formData).pipe(
      tap({
        next: (response: UserRegistrationResponse) => {
          const profilePictureUrl = response.profilePictureUrl
            ? this.imageService.getProfileImageUrl(response.profilePictureUrl)
            : this.imageService.getProfileImageUrl('default-profilepic.jpg');
          this.setProfileImage(profilePictureUrl);
        },
      }),
      catchError(this.handleError<UserRegistrationResponse>('register'))
    );
  }


  // register instructor
  registerInstructor(formData: FormData): Observable<InstructorRegistrationResponse> {
    return this.http
      .post<InstructorRegistrationResponse>(`${this.apiUrl}/register-instructor`, formData)
      .pipe(catchError(this.handleError<InstructorRegistrationResponse>('registerInstructor')));
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
        if (response.requires2FA && response.tempToken) {
          this.cookieService.set('tempToken', response.tempToken, {
            secure: true,
            sameSite: 'Strict',
            expires: new Date(Date.now() + 24 * 60 * 60 * 1000),
          });
          return;
        }
        if (response.requiresPasswordChange && response.resetToken) {
          sessionStorage.setItem('resetToken', response.resetToken);
          sessionStorage.setItem('email', user.Email);
          this.router.navigate(['/reset-password']);
          return;
        }
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

  // Handle the logic after a successful login
  public handleSuccessfulLogin(response: LoginResponse): void {
    if (!response.token || !response.username || !response.roles || !response.userID) {
      throw new Error('Invalid login response - missing required fields');
    }
    const profilePictureUrl =
      response.profilePictureUrl || '/images/profiles/profilepic.png';

    // Set authentication cookies
    this.setAuthCookies(
      response.token,
      response.username,
      response.roles,
      this.imageService.getProfileImageUrl(profilePictureUrl),
      response.fullName || '',
      response.userID
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
  setProfileImage(url: string): void {
    this.cookieService.set('profileImageUrl', url, {
      secure: true,
      sameSite: 'Lax',
      path: '/',
    });
  }

  // Get profile image URL
  getProfileImage(): string {
    return this.imageService.getProfileImageUrl(this.cookieService.get('profileImageUrl'));
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
  getRoles(): Observable<string[]> {
    const roles = this.cookieService.get('roles');
    return roles ? of(JSON.parse(roles)) : of([]);
  }

  // Change Password
  changePassword(
    currentPassword: string,
    newPassword: string
  ): Observable<ChangePasswordResponse> {
    return this.http
      .post<ChangePasswordResponse>(`${this.apiUrl}/change-password`, { currentPassword, newPassword })
      .pipe(catchError(this.handleError<ChangePasswordResponse>('changePassword')));
  }

  // Forgot Password
  forgotPassword(email: string): Observable<ForgotPasswordResponse> {
    return this.http
      .post<ForgotPasswordResponse>(`${this.apiUrl}/forgot-password`, { email })
      .pipe(catchError(this.handleError<ForgotPasswordResponse>('forgotPassword')));
  }

  // Reset Password
  resetPassword(token: string, newPassword: string): Observable<ResetPasswordResponse> {
    return this.http
      .post<ResetPasswordResponse>(`${this.apiUrl}/reset-password`, { token, newPassword })
      .pipe(catchError(this.handleError<ResetPasswordResponse>('resetPassword')));
  }

  // verify current password
  verifyCurrentPassword(
    email: string,
    token: string,
    currentPassword: string
  ): Observable<VerifyPasswordResponse> {
    const url = `${this.apiUrl}/verify-password`;
    return this.http.post<VerifyPasswordResponse>(url, { email, token, password: currentPassword });
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
            const profilePictureUrl = this.imageService.getProfileImageUrl(
              response.profilePictureUrl || 'profilepic.png'
            );
            this.setProfileImage(profilePictureUrl); 

            // Set authentication cookies and profile image
            this.setAuthCookies(
              response.token,
              response.username,
              response.roles,
              profilePictureUrl,
              response.fullName || '',
              response.userID || ''
            );

            this.isAuthenticatedSubject.next(true);
            this.setProfileImage(profilePictureUrl);
            
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
    return this.http
      .post(`${this.apiUrl}/enable-2fa`, {}, { responseType: 'blob' })
      .pipe(switchMap((blob) => this.convertBlobToBase64(blob)));
  }

  // Check 2FA Status
  check2FAStatus(): Observable<{ isEnabled: boolean; error?: string }> {
    return this.http
      .get<{ isEnabled: boolean }>(`${this.apiUrl}/check-2fa-status`)
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
  disable2FA(): Observable<Disable2FAResponse> {
    return this.http.post<Disable2FAResponse>(`${this.apiUrl}/disable-2fa`, {}).pipe(
      catchError((error) => {
        console.error('Error disabling 2FA:', error);
        return throwError(
          () => new Error(error?.error?.message || 'Failed to disable 2FA.')
        );
      })
    );
  }


  getNavbarTypeFromRoles(): Observable<'admin' | 'instructor' | 'student' | 'default'> {
    return this.getRoles().pipe(
      map((roles) => {
        if (roles.includes('Admin')) {
          return 'admin';
        } else if (roles.includes('Instructor')) {
          return 'instructor';
        } else if (roles.includes('Student')) {
          return 'student';
        }
        return 'default';
      })
    );
  }


}

