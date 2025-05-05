import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-confirm2fa',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './confirm2fa.component.html',
  styleUrl: './confirm2fa.component.css'
})
export class Confirm2faComponent {

  verifyForm: FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private authService: AuthService,
    private formBuilder: FormBuilder,
    private router: Router,
  ) {

    this.verifyForm = this.formBuilder.group({
      code: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit(): void {
    if (this.verifyForm.invalid) {
      this.verifyForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    const code = this.verifyForm.get('code')?.value;

    this.authService.confirm2FA(code).subscribe({
      next: (response) => {
        this.successMessage = response.message || '2FA successfully enabled!';
        setTimeout(() => this.router.navigate(['/login']), 3000);
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Verification failed.';
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
}