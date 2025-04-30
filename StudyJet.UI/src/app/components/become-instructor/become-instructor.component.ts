import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { FormControl, FormsModule, Validators } from '@angular/forms';
import { EmailService } from '../../services/email.service';
import { catchError, of } from 'rxjs';
import { CommonModule } from '@angular/common';
import { UploadResponse } from '../../models';
import { emailValidator } from '../../validators/custom.validator';

@Component({
  selector: 'app-become-instructor',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './become-instructor.component.html',
  styleUrl: './become-instructor.component.css'
})
export class BecomeInstructorComponent {
  isLoading = false; 

  @ViewChild('formElement') formElement: ElementRef<HTMLFormElement> | undefined;
  email = '';
  name = '';
  message = '';
  cvFile: File | null = null;
  cvFileUrl = '';

  constructor(
    private http: HttpClient, 
    private emailService: EmailService
  ) { }

// Handle file change event
onFileChange(event: Event): void {
  const input = event.target as HTMLInputElement;
  if (input?.files?.length) {
    const file = input.files[0]; 

    // Check if the file is a PDF
    const allowedExtensions = ['application/pdf'];
    if (!allowedExtensions.includes(file.type)) {
      alert('Only PDF format is allowed.');
      this.cvFile = null; 
      input.value = ''; 
      return;
    }
    this.cvFile = file; 
  }
}

  
  // Handle form submission
  onSubmit(): void {
   // Custom email validation using the validator
   const emailControl = new FormControl(this.email, [Validators.required, emailValidator]);
   if (emailControl.invalid) {
     alert('Please enter a valid email address.');
     return;
   }

    if (this.cvFile && this.email) {
      const formData = new FormData();
      formData.append('cvFile', this.cvFile); 
      formData.append('email', this.email);    
      formData.append('message', this.message);

      this.isLoading = true;

      this.http.post<UploadResponse>('https://localhost:7248/api/UploadCV/upload-cv', formData)
        .pipe(
          catchError(error => {
            console.error('Upload failed', error);
            alert('There was an error uploading the CV.');
            this.isLoading = false;
            return of(null);  
          })
        )
        .subscribe((response) => {
          if (response && response.fileUrl) {
            this.cvFileUrl = response.fileUrl;
            this.sendEmail();
          }
        });
    } else {
      alert('Please enter your email and upload your CV.');
    }
  }

  // Send the email using EmailService
  sendEmail(): void {
    const emailData = {
      name: this.name,
      email: this.email,
      message: this.message,
      cv_attachment: this.cvFile?.name,
      cv_link: `https://localhost:7248${this.cvFileUrl}`
    };

    this.isLoading = true; 

    this.emailService.sendEmail(emailData)
      .then((response) => {
        this.isLoading = false;
        alert('Your CV has been submitted successfully!');
        this.closeModal();
        this.resetForm();
      })
      .catch((error) => {
        this.isLoading = false;
        console.error('Error sending email', error);
      
        if (error.status === 0) {
          alert('Network error! Please check your internet connection.');
        } else if (error.error?.message) {
          // Specific message from backend
          alert(`Error: ${error.error.message}`);
        } else if (error.error?.errors) {
          // Multiple validation errors 
          const errorMessages = Object.values(error.error.errors).flat().join('\n');
          alert(`Validation errors:\n${errorMessages}`);
        } else {
          alert('There was a problem submitting your CV. Please try again later.');
        }
      });
    }

  // Close modal manually
  private closeModal(): void {
    const modal = document.getElementById('becomeInstructorModal');
    const backdrop = document.querySelector('.modal-backdrop');

    if (modal) {
      modal.classList.remove('show');
      modal.setAttribute('aria-hidden', 'true');
      modal.style.display = 'none';
    }
    if (backdrop) {
      backdrop.remove();
    }
    document.body.classList.remove('modal-open');
    document.body.style.removeProperty('padding-right');
  }

  private resetForm() {
    this.name = '';
    this.email = '';
    this.message = '';
    this.cvFile = null;
    this.cvFileUrl = '';
     if (this.formElement) {
    const fileInput: HTMLInputElement | null = this.formElement.nativeElement.querySelector('input[type="file"]');
    if (fileInput) {
      fileInput.value = ''; 
    }
  }
}
}