import { AbstractControl, AsyncValidatorFn, FormGroup, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { UserService } from '../services/user.service';

// Check if the password and confirm password match
export function passwordMatchValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const formGroup = control as FormGroup;
    const password = formGroup.get('Password')?.value;
    const confirmPassword = formGroup.get('ConfirmPassword')?.value;

    if (!password || !confirmPassword) {
      return null;
    }
    return password === confirmPassword ? null : { mismatch: true };
  };
}

// Check if new password is different from the current password.
export function differentPasswordValidator(currentPassword: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const newPassword = control.value;
    return newPassword === currentPassword ? { samePassword: true } : null;
  };
}

//Check if the email already exists 
export function emailExistsValidator(userService: UserService): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value) {
      return of(null);
    }
    return userService.checkEmailExists(control.value).pipe(
      debounceTime(300), 
      distinctUntilChanged(), 
      switchMap(emailExists => {
        if (emailExists) {
          return of({ emailExists: true }); 
        } else {
          return of(null); 
        }
      }),
      catchError(() => of(null)) 
    );
  };
}

// Compares two inputs
export function differentPasswordGroupValidator(): ValidatorFn {
  return (formGroup: AbstractControl): ValidationErrors | null => {
    const currentPassword = formGroup.get('CurrentPassword')?.value;
    const newPassword = formGroup.get('Password')?.value;

    if (!currentPassword || !newPassword) return null;

    return currentPassword === newPassword ? { samePassword: true } : null;
  };
}

//Enforce strong password rules
export function passwordValidator(): ValidatorFn {
  return (control: AbstractControl): { [key: string]: boolean } | null => {
    const password = control.value;
    const passwordPattern = /^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?":{}|<>]).{8,}$/;

    if (password && !passwordPattern.test(password)) {
      return { passwordStrength: true };
    }
    return null;
  };
}

//Check if the email format is valid
export function emailValidator(control: AbstractControl): ValidationErrors | null {
  const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  if (!control.value) {
    return null;
  }
  return emailPattern.test(control.value) ? null : { emailInvalid: true };
}

export function usernameExistsValidator(userService: UserService): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value) {
      return of(null); 
    }
    return userService.checkUsernameExists(control.value).pipe(
      debounceTime(300),  
      distinctUntilChanged(),  
      switchMap(usernameExists => {
        if (usernameExists) {
          return of({ usernameExists: true });
        } else {
          return of(null); 
        }
      }),
      catchError(() => of(null))  
    );
  };
}