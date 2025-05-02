import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
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
export class Enable2faComponent {

  qrCode: string | null = null;
  twoFACode: string | null = null;
  verifyForm: FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  is2FAEnabled = false;
  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.verifyForm = this.formBuilder.group({
      code: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    this.checkTwoFAStatus();
  }

  checkTwoFAStatus(): void {
    this.authService.check2FAStatus().subscribe({
      next: (response) => {
        this.is2FAEnabled = response.isEnabled;
        this.errorMessage = null;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while checking 2FA status.';
        console.error('2FA Status Error:', this.errorMessage);
      },
    });
  }

  enable2FA(): void {
    this.authService.enable2FA().subscribe({
      next: (response) => {
        this.qrCode = 'data:image/png;base64,' + response.qrCode;
        this.errorMessage = null;
        setTimeout(() => this.router.navigate(['/confirm-2fa']), 6000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'An error occurred while enabling 2FA.';
      },
    });
  }

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
        this.errorMessage = 'StudyJet says: ' + (error.error?.message || 'Failed to disable 2FA.');
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  confirmDisable2FA(): void {
    if (confirm('Are you sure you want to disable Two-Factor Authentication? This will reduce the security of your account.')) {
      this.disable2FA();
    }
  }

}












