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
      const tempToken = this.cookieService.get('tempToken');
      if (!tempToken) {
        this.errorMessage = 'Session expired. Please log in again.';
        this.router.navigate(['/login']);
      }
    }
    
    verify2FALogin() {
      // temporary
      this.navbarService.setNavbarType('hidden');
    
      this.authService.verify2FALogin(this.code).subscribe({
        next: (response) => {
          if (response.token) {
            this.authService.handleSuccessfulLogin(response);  // Handle successful login
            const roles = response.roles || [];
    
            this.cdr.detectChanges();
    
            setTimeout(() => {
              this.authService.getNavbarTypeFromRoles().subscribe((navbarType) => {
                this.navbarService.setNavbarType(navbarType); // Update navbar type
              });
    
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
  
