import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    // Initialize the form with email field and validation
    this.forgotPasswordForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  // Getter for email form control to simplify form access
  get email() {
    return this.forgotPasswordForm.get('email');
  }

  // Handles form submission
  onSubmit() {
    // If form is invalid, mark all controls as touched to trigger validation
    if (this.forgotPasswordForm.invalid) {
      this.forgotPasswordForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const email = this.email?.value;

    // Call the AuthService to send the password reset instructions
    this.authService.forgotPassword(email).subscribe({
      next: () => {
        this.successMessage =
          'A password reset link has been sent to your email address. Please check your inbox.';
        this.loading = false;
      },
      error: () => {
        this.errorMessage =
          'An error occurred while sending the reset instructions. Please try again.';
        this.loading = false;
      },
    });
  }
}




