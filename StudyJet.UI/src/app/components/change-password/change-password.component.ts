import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { passwordValidator, passwordMatchValidator } from '../../validators/custom.validator';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.css'
})
export class ChangePasswordComponent {

  changePasswordForm: FormGroup;
  loading = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
  ) {

    this.changePasswordForm = this.formBuilder.group(
      {
        CurrentPassword: ['', [Validators.required]],
        Password: ['', [Validators.required, passwordValidator()]],
        ConfirmPassword: ['', [Validators.required]],
      },
      { validators: passwordMatchValidator() }
    );
  }

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
    }
  }
  
    get currentPassword() {
      return this.changePasswordForm.get('CurrentPassword');
    }
  
    get password() {
      return this.changePasswordForm.get('Password');
    }
  
    get confirmPassword() {
      return this.changePasswordForm.get('ConfirmPassword');
    }


     
  onSubmit(): void {
    this.changePasswordForm.markAllAsTouched();

    if (this.changePasswordForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const changePasswordData = {
      currentPassword: this.changePasswordForm.value.CurrentPassword,
      newPassword: this.changePasswordForm.value.Password,
    };

    console.log('Sending changePassword data:', changePasswordData); // Add this log

    this.authService.changePassword(changePasswordData.currentPassword, changePasswordData.newPassword)
    .subscribe({
      next: (response) => {
        this.successMessage = 'Password successfully updated';
        this.loading = false;
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 5000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Something went wrong. Please try again.';
        this.loading = false;
      },
    });
}
}
  












