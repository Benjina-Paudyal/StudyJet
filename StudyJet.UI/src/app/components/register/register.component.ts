import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CookieService } from 'ngx-cookie-service'
import { AuthService } from '../../services/auth.service';
import { emailExistsValidator, emailValidator, passwordMatchValidator, passwordValidator, usernameExistsValidator } from '../../validators/custom.validator';
import { UserService } from '../../services/user.service';
import { debounceTime } from 'rxjs';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  registerForm : FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  constructor(
    private formBuidler: FormBuilder,
    private userService: UserService,
    private authService: AuthService,
    private CookieService: CookieService,
    private router: Router,
  ){
    this.registerForm = this.formBuidler.group({
      FullName: ['', [Validators.required]], 
      UserName: ['', [Validators.required], [usernameExistsValidator(this.userService)]],
      Email: ['', [Validators.required, emailValidator], [emailExistsValidator(this.userService)]],
      Password: ['', [Validators.required, passwordValidator()]],
      ConfirmPassword: ['', [Validators.required]], 
      ProfilePicture: [null], 
    }, { validators: passwordMatchValidator() }); 
  }

  ngOnInit(): void {
    // Async validators are debounced to avoid excessive API calls
    this.userName?.valueChanges.pipe(debounceTime(500)).subscribe(() => {
      this.userName?.updateValueAndValidity();
    });

    this.email?.valueChanges.pipe(debounceTime(500)).subscribe(() => {
      this.email?.updateValueAndValidity();  
    });
  }

  // Getter methods for form controls
  get userName() {
    return this.registerForm.get('UserName');
  }

  get email() {
    return this.registerForm.get('Email');
  }

  get password() {
    return this.registerForm.get('Password');
  }

  get confirmPassword() {
    return this.registerForm.get('ConfirmPassword');
  }

  get fullName() {
    return this.registerForm.get('FullName');
  }
  
  // Handling file selection for profile picture
  onFileChange(event: any) {
    const file = event.target.files[0];
    this.registerForm.patchValue({ ProfilePicture: file });
  }

// Handle form submission
  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();  
      return;
    }
    this.loading = true; 
    this.errorMessage = null; 
    this.successMessage = null; 

    // Clear validation errors and disable form
  Object.keys(this.registerForm.controls).forEach(key => {
    this.registerForm.get(key)?.setErrors(null);
    this.registerForm.get(key)?.markAsUntouched();
  });
  this.registerForm.disable();


  // Prepare form data 
    const formData = new FormData();
    formData.append('FullName', this.fullName?.value);
    formData.append('UserName', this.userName?.value);
    formData.append('Email', this.email?.value);
    formData.append('Password', this.password?.value);
    formData.append('ConfirmPassword', this.confirmPassword?.value);
      
  // Append prfile picture if exists
    const profilePicture = this.registerForm.get('ProfilePicture')?.value;
    if (profilePicture) {
      formData.append('ProfilePicture', profilePicture);
    }
    // submit
    this.authService.register(formData).subscribe({
      next: () => {
        this.successMessage = "Please check your email to verify and complete your registration.";
        this.registerForm.enable();
        this.registerForm.reset({
          FullName: '',
          UserName: '',
          Email: '',
          Password: '',
          ConfirmPassword: '',
          ProfilePicture: null, 
        });
    setTimeout(() => {
      this.successMessage = null; 
      this.router.navigate(['/']);  
    }, 5000);  
  },
    error: (error) => {
      this.registerForm.enable();
      this.loading = false;
      this.handleRegistrationError(error);  
    },
  });
}

  // Handle registration errors based on the error response from backend
  private handleRegistrationError(error: any) {
    console.log(error);
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

  // Dynamic error message function
  getErrorMessage(controlName: string): string | null {
    const control = this.registerForm.get(controlName);
    if (!control || !control.touched || control.status === 'PENDING' || control.pristine) {
      return null;  
    }
    
    if (control?.touched && control?.invalid) {
      // Sync validation errors
      if (control.hasError('required')) return `${controlName} is required.`;
      if (control.hasError('emailInvalid')) return 'Invalid email format.';
      if (control.hasError('passwordStrength')) return 'Password must be at least 8 characters long and contain an uppercase letter, lowercase letter, number, and special character.';
      if (this.registerForm.errors?.['mismatch']) return 'Passwords must match.';
  
      // Async validation errors
      if (control.hasError('usernameExists')){
        console.log('Email already exists error is triggered'); 
       return 'This username is already taken.';
      }
      if (control.hasError('emailExists')) return 'This email is already registered.';  
    }
  
    return null;
  }
  

}

  


