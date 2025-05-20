import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { debounceTime } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { usernameExistsValidator, emailValidator, emailExistsValidator } from '../../validators/custom.validator';

@Component({
  selector: 'app-register-instructor',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
  templateUrl: './register-instructor.component.html',
  styleUrl: './register-instructor.component.css'
})
export class RegisterInstructorComponent implements OnInit {
  registerInstructorForm: FormGroup; 
  loading = false; 
  errorMessage: string | null = null; 
  successMessage: string | null = null; 


  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private router: Router,
    private authService: AuthService,
  ) {
    this.registerInstructorForm = this.formBuilder.group({
      FullName: ['', [Validators.required]], 
      UserName: ['', [Validators.required], [usernameExistsValidator(this.userService)]],
      Email: ['', [Validators.required, emailValidator], [emailExistsValidator(this.userService)]],
      ProfilePicture: [null], 
    });
  }

    ngOnInit(): void {
      // Adding debounce on value changes for username and email
      this.userName?.valueChanges.pipe(
        debounceTime(500)  
      ).subscribe(() => {
        this.userName?.updateValueAndValidity();  
      });
  
      this.email?.valueChanges.pipe(
        debounceTime(500)
      ).subscribe(() => {
        this.email?.updateValueAndValidity();  
      });
    }

  get fullName() { 
    return this.registerInstructorForm.get('FullName'); 
  }

  get userName() { 
    return this.registerInstructorForm.get('UserName');
   }

  get email() { 
    return this.registerInstructorForm.get('Email'); 
  }

  // Handle file selection
  onFileChange(event: any) {
    const file = event.target.files[0];
    this.registerInstructorForm.patchValue({ ProfilePicture: file });
  }

  // Submit instructor registration
  onSubmit() {
    if (this.registerInstructorForm.invalid) {
      this.registerInstructorForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

     // Disable async validators to prevent flicker
    this.toggleAsyncValidators(false);

    const formData = new FormData();
    formData.append('FullName', this.fullName?.value);
    formData.append('UserName', this.userName?.value);
    formData.append('Email', this.email?.value);
    formData.append('Password', 'Instructor@123');  
    formData.append('ConfirmPassword', 'Instructor@123'); 

    if (this.registerInstructorForm.get('ProfilePicture')?.value) {
      formData.append('ProfilePicture', this.registerInstructorForm.get('ProfilePicture')?.value);
    }

    this.authService.registerInstructor(formData).subscribe({
      next: () => {
        this.successMessage = "Instructor registered successfully. Confirmation email sent.";
        this.loading = false;
        this.registerInstructorForm.reset();

        setTimeout(() => {
          this.successMessage = null; 
          this.router.navigate(['/manage-instructors']);  
        }, 5000);  
      },

      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || "Registration failed. Please try again.";

      // Re-enable async validators on error to restore validation behavior
      this.toggleAsyncValidators(true);
      },
    });
  }

  // Handle registration errors
  private handleRegistrationError(error: any) {

    if (error.status === 400 && error.error) {
      if (error.error.usernameExists) {
        this.errorMessage = 'This username is already taken.';
      } else if (error.error.emailExists) {
        this.errorMessage = 'This email is already registered.';
      } else if (typeof error.error === 'string') {
        this.errorMessage = error.error; 
      } else {
        this.errorMessage = 'Registration failed. Please try again.';
      }
    } else {
      this.errorMessage = 'An unexpected error occurred. Please try again later.';
    }
  }




  private toggleAsyncValidators(enable: boolean) {
  if (enable) {
    this.userName?.setAsyncValidators(usernameExistsValidator(this.userService));
    this.email?.setAsyncValidators(emailExistsValidator(this.userService));
  } else {
    this.userName?.clearAsyncValidators();
    this.email?.clearAsyncValidators();
  }
  this.userName?.updateValueAndValidity({ emitEvent: false });
  this.email?.updateValueAndValidity({ emitEvent: false });
}

}

