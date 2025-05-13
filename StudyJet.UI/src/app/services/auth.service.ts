import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { environment } from '../../environments/environment';
import { BehaviorSubject, catchError, map, Observable, of, ReplaySubject, switchMap, tap, throwError, } from 'rxjs';
import { ImageService } from './image.service';
import { CookieService } from 'ngx-cookie-service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { UserRegistration, UserLogin, LoginResponse, AuthTokenPayload, UserRegistrationResponse, ForgotPasswordResponse, ResetPasswordResponse, InstructorRegistrationResponse, ChangePasswordResponse, VerifyPasswordResponse, Disable2FAResponse, AuthResponse, } from '../models';
import { NavbarService } from './navbar.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiBaseUrl}/Auth`;

  // state subjects
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  private profileImageSubject = new BehaviorSubject<string | null>(null);
  public profileImage$ = this.profileImageSubject.asObservable();

  private navbarTypeSubject = new BehaviorSubject<'admin' | 'instructor' | 'student' | 'default'>('default');
  navbarType$ = this.navbarTypeSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
    private imageService: ImageService,
    private cookieService: CookieService,
    private navbarService: NavbarService,
  ) {
    this.checkAuthState();

  // Restore profile image from cookies if available
    const savedUrl = this.cookieService.get('profileImageUrl');
    if (savedUrl) {
      const fullUrl = this.imageService.getProfileImageUrl(savedUrl);
      this.profileImageSubject.next(fullUrl);
    }
  }

  // Validates authentication state on app load
  private checkAuthState(): void {
    const token = this.cookieService.get('authToken');
    const username = this.cookieService.get('authEmail');
    const roles = this.cookieService.get('roles');
    const rawProfile = this.cookieService.get('profileImageUrl');

    if (token && username && !this.isTokenExpired(token)) {
      this.isAuthenticatedSubject.next(true);

      if (roles) {
        this.updateNavbarType(JSON.parse(roles));
      }

      if (rawProfile) {
        const fullProfileUrl = this.imageService.getProfileImageUrl(rawProfile);
        this.setProfileImage(fullProfileUrl);
      }
    } else {
      this.isAuthenticatedSubject.next(false);
      this.clearAuthCookies();
    }
  }

  // JWT expiration check
  private isTokenExpired(token: string): boolean {
    try {
      const payload: AuthTokenPayload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 < Date.now();
    } catch (e) {
      return true;
    }
  }

  //Sets all necessary cookies after login
  private setAuthCookies(
    token: string,
    username: string,
    roles: string[],
    profilePictureUrl: string,
    fullName: string,
    userId: string
  ): void {
    const cookieOptions = { path: '/', secure: true, sameSite: 'Lax' as const, expires: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000), };
    this.cookieService.set('authToken', token, cookieOptions);
    this.cookieService.set('username', username, cookieOptions);
    this.cookieService.set('roles', JSON.stringify(roles), cookieOptions);
    this.cookieService.set('profileImageUrl', profilePictureUrl, cookieOptions);
    this.cookieService.set('fullName', fullName, cookieOptions);
    this.cookieService.set('userId', userId, cookieOptions);
  }

  // Remove all auth-related cookies
  private clearAuthCookies(): void {
    const cookiesToRemove = [
      'authToken', 'profileImageUrl', 'username', 'roles', 'fullName', 'userId', 'tempToken', 'authEmail'
    ];
    cookiesToRemove.forEach((cookie) => this.cookieService.delete(cookie, '/'));
  }

  // Handles student registration
  register(formData: FormData): Observable<UserRegistrationResponse> {
    return this.http.post<UserRegistrationResponse>(`${this.apiUrl}/register`, formData).pipe(
      tap({
        next: (response: UserRegistrationResponse) => {
          const profilePictureUrl = response.profilePictureUrl
            ? this.imageService.getProfileImageUrl(response.profilePictureUrl)
            : this.imageService.getProfileImageUrl('profilepic.png');
          this.setProfileImage(profilePictureUrl);
        },
      }),
      catchError(this.handleError<UserRegistrationResponse>('register'))
    );
  }

  // Handles instructor registration
  registerInstructor(formData: FormData): Observable<InstructorRegistrationResponse> {
    return this.http
      .post<InstructorRegistrationResponse>(`${this.apiUrl}/register-instructor`, formData)
      .pipe(catchError(this.handleError<InstructorRegistrationResponse>('registerInstructor')));
  }

  // Creates FormData for registration request
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

  // Handles login and all related conditions
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
          return;
        }
        if (response.token && response.userName && response.roles) {
          this.handleSuccessfulLogin(response);
        }
      }),
      map((response: LoginResponse) => response),
      catchError((error: any) => {
        console.error('Login failed:', error);
        return throwError(() => error);
      })
    );
  }


  // Handle successful login and state updates
  handleSuccessfulLogin(response: LoginResponse): void {
    if (!response.token || !response.userName || !response.roles || !response.userID) {
      throw new Error('Invalid login response - missing required fields');
    }
    const profilePictureUrl = response.profilePictureUrl || '/images/profiles/profilepic.png';
    const fullProfileUrl = this.imageService.getProfileImageUrl(profilePictureUrl);

    this.cookieService.set('authEmail', response.userName, { secure: true, sameSite: 'Strict', path: '/', });
    this.cookieService.set('authToken', response.token, { secure: true, sameSite: 'Strict', path: '/', expires: 7 });
    this.cookieService.set('userID', response.userID, { secure: true, sameSite: 'Strict', path: '/', });

    this.setAuthCookies(response.token, response.userName, response.roles, fullProfileUrl, response.fullName || '', response.userID);
    this.setProfileImage(fullProfileUrl);
    this.updateNavbarType(response.roles);
    this.isAuthenticatedSubject.next(true);
    this.cookieService.delete('tempToken');
  }

  // Updates profile image URL in state and cookies
  setProfileImage(url: string): void {
    this.cookieService.set('profileImageUrl', url, { secure: true, sameSite: 'Lax', path: '/', });
    this.profileImageSubject.next(url);
  }

  // Retrieves current profile image URL
  getProfileImage(): string {
    return this.imageService.getProfileImageUrl(
      this.cookieService.get('profileImageUrl') || ''
    );
  }

  // Map roles to navbar type
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

  // Emits navbar type based on user roles
  updateNavbarType(roles: string[]): void {
    let navbarType: 'admin' | 'instructor' | 'student' | 'default' = 'default';
    if (roles.includes('Admin')) {
      navbarType = 'admin';
    } else if (roles.includes('Instructor')) {
      navbarType = 'instructor';
    } else if (roles.includes('Student')) {
      navbarType = 'student';
    }
    this.navbarTypeSubject.next(navbarType);
  }


  // Clears session and cookies on logout
  async logout(): Promise<void> {
    try {
      this.clearAuthCookies();
      sessionStorage.clear();
      this.isAuthenticatedSubject.next(false);
      sessionStorage.setItem('logoutSuccess', 'true');
      await this.router.navigate(['/home']);
      this.navbarService.setNavbarType('default');
    } catch (error) {
      console.error('Logout failed:', error);
    }
  }

  // Generic HTTP error handler
  handleError<T>(operation = 'operation'): (error: any) => Observable<T> {
    return (error: any): Observable<T> => {
      console.error(`${operation} failed:`, error);

      if (error instanceof HttpErrorResponse) {
        return throwError(() => error);
      }
      let errorMessage = 'Something went wrong';
      if (error.error instanceof ErrorEvent) {
        errorMessage = `Error: ${error.error.message}`;
      } else if (error.error?.message) {
        errorMessage = error.error.message;
      } else if (error.message) {
        errorMessage = error.message;
      }
      return throwError(() => new Error(errorMessage));
    };
  }

  // Return user's email from cookie
  getEmail(): Observable<string> {
    const email = this.cookieService.get('authEmail');
    if (email) {
      return of(email);
    } else {
      return throwError(() => new Error('Email not found.'));
    }
  }

  // Returns user roles from cookie
  getRoles(): Observable<string[]> {
    const roles = this.cookieService.get('roles');
    return roles ? of(JSON.parse(roles)) : of([]);
  }

  // Change password
  changePassword(
    currentPassword: string,
    newPassword: string
  ): Observable<ChangePasswordResponse> {
    return this.http
      .post<ChangePasswordResponse>(`${this.apiUrl}/change-password`, { currentPassword, newPassword })
      .pipe(catchError(this.handleError<ChangePasswordResponse>('changePassword')));
  }

  // Request forgot password
  forgotPassword(email: string): Observable<ForgotPasswordResponse> {
    return this.http
      .post<ForgotPasswordResponse>(`${this.apiUrl}/forgot-password`, { email })
      .pipe(catchError(this.handleError<ForgotPasswordResponse>('forgotPassword')));
  }

  // Reset password
  resetPassword(email: string, token: string, newPassword: string): Observable<ResetPasswordResponse> {
    return this.http
      .post<ResetPasswordResponse>(`${this.apiUrl}/reset-password`, { email, token, newPassword })
      .pipe(catchError(this.handleError<ResetPasswordResponse>('resetPassword')));
  }

  // Verifies the current password before change
  verifyCurrentPassword(
    email: string,
    token: string,
    currentPassword: string
  ): Observable<VerifyPasswordResponse> {
    const url = `${this.apiUrl}/verify-password`;
    return this.http.post<VerifyPasswordResponse>(url, { email, token, password: currentPassword });
  }

  // Checks local auth state
  isAuthenticated(): boolean {
    const token = this.cookieService.get('authToken');
    return (
      this.isAuthenticatedSubject.value &&
      !!token &&
      !this.isTokenExpired(token)
    );
  }

  // Initiates 2FA and returns QR code
  enable2FA(): Observable<{ qrCode: string }> {
    return this.http
      .post(`${this.apiUrl}/initiate-2fa`, {}, { responseType: 'blob' })
      .pipe(switchMap((blob) => this.convertBlobToBase64(blob)));
  }

  // Converts image blob to base64
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

  // Confirms 2FA code
  confirm2FA(code: string): Observable<any> {
    const email = this.cookieService.get('authEmail');
    if (!email) {
      return throwError(() => new Error('Email is missing or not found in cookies.'));
    }
    return this.http
      .post(`${this.apiUrl}/confirm-2fa`, { code, email })
      .pipe(
        catchError((error) => {
          return throwError(() => new Error(error.error?.message || 'Error confirming 2FA.'));
        })
      );
  }

  // Verifies login using 2FA code
  verify2FALogin(code: string): Observable<LoginResponse> {
    const tempToken = this.cookieService.get('tempToken');
    if (!tempToken) {
      return throwError(() => new Error('Temporary token is missing.'));
    }
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/verify-2fa-login`, {
        code,
        tempToken,
      })
      .pipe(
        tap((response: LoginResponse) => {
          if (response.token && response.userName && response.roles) {
            this.handleSuccessfulLogin(response);
          }
        }),
        catchError((error: any) => {
          const handledError = this.handleError<LoginResponse>('verify2FALogin')(error);
          return throwError(() => handledError);
        })
      );
  }

  // Checks if 2FA is currently enabled
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

  // Disables 2FA for the user
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
}
