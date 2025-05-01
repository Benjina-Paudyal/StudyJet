import { Component } from '@angular/core';
import { UserLogin } from '../../models';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { WishlistService } from '../../services/wishlist.service';
import { Router, RouterModule } from '@angular/router';
import { NavbarService } from '../../services/navbar.service';
import { UserService } from '../../services/user.service';
import { ImageService } from '../../services/image.service';
import { CookieService } from 'ngx-cookie-service';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';

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
    private imageService: ImageService,
    private cookieService: CookieService
  ){
    // Initialize form with validators
    this.loginForm = this.formBuilder.group({
      Email: ['',[Validators.required, Validators.email]],
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
    this.authService.login(loginData).subscribe({
      next: (response) =>{
        if(response.requires2FA) {
          this.router.navigate(['/verify-2fa']);
        }else if (response.requiresPasswordChange) {
          this.router.navigate(['/reset-password']);
        } else {
          this.authService.handleSuccessfulLogin(response);
          this.profileImageUrl = this.imageService.getProfileImageUrl(response.profilePictureUrl ?? 'default.png');

          // Store profile picture URL in cookies to persist across page refreshes
        this.cookieService.set('profilePictureUrl', this.profileImageUrl, { expires: 7 });
          this.loadWishlist();

          const navbarType = this.authService.getNavbarTypeFromRoles();
          this.navbarService.setNavbarType(navbarType);
           this.navigateToDashboard(navbarType);
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






