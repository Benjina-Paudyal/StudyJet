import { Component } from '@angular/core';
import { UserLogin } from '../../models';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { WishlistService } from '../../services/wishlist.service';
import { Router, RouterModule } from '@angular/router';
import { NavbarService } from '../../services/navbar.service';
import { UserService } from '../../services/user.service';
import { CookieService } from 'ngx-cookie-service';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { InactivityService } from '../../services/inactivity.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  profileImageUrl: string | null = null;
  wishlist: any[] = [];

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private wishlistService: WishlistService,
    private navbarService: NavbarService,
    private userService: UserService,
    private cookieService: CookieService,
    private inactivityService: InactivityService
  ) {
    // Initialize form with validators
    this.loginForm = this.formBuilder.group({
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', [Validators.required]],
    });
  }

  // Form field accessors for easy reference
  get email() {
    return this.loginForm.get('Email');
  }

  get password() {
    return this.loginForm.get('Password');
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    const loginData: UserLogin = {
      Email: this.email?.value,
      Password: this.password?.value,
    };

    // temporary
    this.navbarService.setNavbarType('hidden');
    this.authService.login(loginData).subscribe({
      next: (response) => {

        if (response.requires2FA) {
          // Handle 2FA case
          this.cookieService.set('authEmail', loginData.Email);
          this.router.navigate(['/verify2fa-login']);

        } else if (response.requiresPasswordChange) {
          // User needs to reset password after email confirmation
          this.cookieService.set('resetToken', response.resetToken ?? '', { secure: true, sameSite: 'Strict' });
          this.cookieService.set('mail', response.email ?? '', { secure: true, sameSite: 'Strict' }); 

          // Navigate to the reset password page, passing the email directly from the response
          this.router.navigate(['/reset-password'], {
            state: { 
              email: response.email ?? '',
              token: response.resetToken ?? '' },  
          });

        } else {
          this.authService.handleSuccessfulLogin(response);
          this.profileImageUrl = this.authService.getProfileImage();
          this.cookieService.set('profilePictureUrl', this.profileImageUrl, { expires: 7 });
          this.loadWishlist();

          this.authService.getNavbarTypeFromRoles().subscribe((navbarType) => {
            setTimeout(() => {
              this.navbarService.setNavbarType(navbarType);
              this.navigateToDashboard(navbarType);
            }, 1000);
          });
          this.inactivityService.startMonitoring(); 
        }
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        if (error.status === 401) {
          this.errorMessage = 'Incorrect email or password. Please try again.';
        } else if (error.error?.message === 'ChangePasswordRequired') {
          this.router.navigate(['/reset-password'], {
            state: { email: this.email?.value },
          });
        } else {
          this.errorMessage = 'An unknown error occurred. Please try again later.';
        }
      },
    });
  }

  private loadWishlist() {
    this.userService.getWishlistForCurrentUser().subscribe({
      next: (wishlist) => {
        this.wishlistService.wishlistSubject.next(wishlist);
        this.wishlist = wishlist;
      },
      error: (error) => {
        console.error('Failed to load wishlist:', error);
      }
    });
  }

  // Navigate to dashboard based on the navbar type (Admin, Instructor, Student)
  private navigateToDashboard(navbarType: 'admin' | 'instructor' | 'student' | 'default') {
    switch (navbarType) {
      case 'admin':
        this.router.navigate(['/admin-dashboard']);
        break;
      case 'instructor':
        this.router.navigate(['/instructor-dashboard']);
        break;
      case 'student':
        this.router.navigate(['/student-dashboard']);
        break;
      default:
        this.router.navigate(['/home']);
        break;
    }

  }
}






