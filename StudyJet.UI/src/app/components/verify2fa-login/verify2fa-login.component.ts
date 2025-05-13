import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';
import { NavbarService } from '../../services/navbar.service';

@Component({
  selector: 'app-verify2fa-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './verify2fa-login.component.html',
  styleUrl: './verify2fa-login.component.css'
})
export class Verify2faLoginComponent implements OnInit{
    code = '';
    tempToken: string | null = null;
    errorMessage: string | null = null;

    constructor(
      private authService: AuthService, 
      private router: Router,
      private cookieService: CookieService,
      private navbarService: NavbarService,
      private cdr: ChangeDetectorRef
    ) {}
  
    ngOnInit() {
      // Retrieve tempToken from cookies to verify the session
      const tempToken = this.cookieService.get('tempToken');

      // If no tempToken is found, session is expired and user is redirected to login page
      if (!tempToken) {
        this.errorMessage = 'Session expired. Please log in again.';
        this.router.navigate(['/login']);
      }
    }
    
    // Method to verify 2FA login
    verify2FALogin() {

      // Hide the navbar during the verification process
      this.navbarService.setNavbarType('hidden');
    
      // Call authService to verify the 2FA code
      this.authService.verify2FALogin(this.code).subscribe({
        next: (response) => {
          if (response.token) {
            this.authService.handleSuccessfulLogin(response); 
            
            // Get user roles from the response
            const roles = response.roles || [];
    
            this.cdr.detectChanges();
    
            setTimeout(() => {
              this.authService.getNavbarTypeFromRoles().subscribe((navbarType) => {
                this.navbarService.setNavbarType(navbarType);
              });
    
              // Redirect user to the appropriate dashboard based on their role
              if (roles.includes('Admin')) {
                this.router.navigate(['/admin-dashboard'], { 
                  replaceUrl: true 
                });
              } else if (roles.includes('Instructor')) {
                this.router.navigateByUrl('/', {skipLocationChange: true}).then(() => {
                  this.router.navigate(['/instructor-dashboard'], {
                    replaceUrl: true 
                  });
                });
              } else {
                this.router.navigate(['/student-dashboard'], { 
                  replaceUrl: true 
                });
              }
            }, 1000); 
          }
        },
        error: (error) => {
          this.errorMessage = error.message || 'Verification failed';
        }
      });
    }
}
  
