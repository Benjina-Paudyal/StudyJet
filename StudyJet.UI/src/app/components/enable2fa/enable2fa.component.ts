import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-enable2fa',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './enable2fa.component.html',
  styleUrl: './enable2fa.component.css'
})
export class Enable2faComponent implements OnInit{

  qrCode: string | null = null;
  twoFACode: string | null = null;
  verifyForm: FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  is2FAEnabled = false;
  qrScanned = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    // Initialize the form with validation rules
    this.verifyForm = this.formBuilder.group({
      code: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  ngOnInit(): void {
    // If user is not authenticated, redirect to login page
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    this.checkTwoFAStatus();
  }

  // Check if 2FA is already enabled
  checkTwoFAStatus(): void {
    this.authService.check2FAStatus().subscribe({
      next: (response) => {
        console.log('2FA Status Response:', response); 
        this.is2FAEnabled = response.isEnabled;
        this.errorMessage = null;
      },
      error: (error) => {
        // Set error message if 2FA status check fails
        this.errorMessage = error.error?.message || 'An error occurred while checking 2FA status.';
        console.error('2FA Status Error:', this.errorMessage);
      },
    });
  }

  // Enable 2FA and generate QR code for the user to scan
  enable2FA(): void {
    this.authService.enable2FA().subscribe({
      next: (response) => {
        this.qrCode = 'data:image/png;base64,' + response.qrCode;
        this.errorMessage = null;
      },
      error: (error) => {
        // Set error message if enabling 2FA fails
        this.errorMessage = error.error?.message || 'An error occurred while enabling 2FA.';
      },
    });
  }

  // Proceed to 2FA confirmation step after QR code is scanned
  confirm2FASetup(): void {
    this.qrScanned = true;
    this.router.navigate(['/confirm-2fa']);
  }

  // Disable 2FA for the user
  disable2FA(): void {
    this.loading = true;
    this.authService.disable2FA().subscribe({
      next: () => {
        this.is2FAEnabled = false;
        this.successMessage = 'StudyJet: 2FA has been disabled successfully.';
        this.errorMessage = null;
        setTimeout(() => this.router.navigate(['/login']), 3000);
      },
      error: (error) => {
        // Set error message if disabling 2FA fails
        this.errorMessage = 'StudyJet says: ' + (error.error?.message || 'Failed to disable 2FA.');
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  // Confirm action to disable 2FA with a warning
  confirmDisable2FA(): void {
    if (confirm('Are you sure you want to disable Two-Factor Authentication? This will reduce the security of your account.')) {
      this.disable2FA();
    }
  }
}