import { Component, OnInit } from '@angular/core';
import { passwordMatchValidator, passwordValidator } from '../../validators/custom.validator';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  loading: boolean = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  token: string | null = null;
  email: string | null = null;
  currentPassword: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cookieService: CookieService
  ) {
    this.resetPasswordForm = this.formBuilder.group(
      {
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            passwordValidator(), // Enforce strong password rules
          ],
        ],
        confirmPassword: ['', [Validators.required]],
      },
      {
        validators: [
          passwordMatchValidator(),
        ],
      }
    );
  }


  ngOnInit(): void {

    // First check for state (from navigation)
    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras.state) {
      this.email = navigation.extras.state['email'];
      console.log('Email from router state:', this.email);
    }
    // Then check for token and email from cookies or URL
    this.token = this.cookieService.get('resetToken') || this.route.snapshot.queryParams['token'];
    this.email = this.cookieService.get('mail') || this.route.snapshot.queryParams['email'];

    if (!this.token || !this.email) {
      this.errorMessage = 'Invalid reset details. Please try again.';
    }
    this.updatePasswordValidator();
  }

  get password() {
    return this.resetPasswordForm.get('password');
  }

  get confirmPassword() {
    return this.resetPasswordForm.get('confirmPassword');
  }

  updatePasswordValidator(): void {
    this.resetPasswordForm.get('Password')?.setValidators([Validators.required, passwordValidator()]);
    this.resetPasswordForm.get('Password')?.updateValueAndValidity();
  }

  onSubmit(): void {
    this.resetPasswordForm.markAllAsTouched();
    if (this.resetPasswordForm.invalid || !this.token || !this.email) {
      this.errorMessage = 'Invalid reset details. Please try again.';
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const resetPasswordData = {
      email: this.email,
      token: this.token,
      newPassword: this.password?.value,
    };

    this.authService.resetPassword(resetPasswordData.email, resetPasswordData.token, resetPasswordData.newPassword).subscribe({
      next: () => {
        this.successMessage = 'Password successfully updated! Redirecting to login...';
        this.loading = false;
        setTimeout(() => this.router.navigate(['/login']), 3000);
      },
      error: (error) => {
        console.log('resetPassword failed: ', error);
        if (error.error && error.error.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage = 'Something went wrong. Please try again.';
        }
        this.loading = false;
      },
    });
  }
}