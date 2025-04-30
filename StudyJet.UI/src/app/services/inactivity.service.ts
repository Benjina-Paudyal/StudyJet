import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class InactivityService {
  private timeout: any;
  private warningTimeout: any;
  private readonly INACTIVITY_TIMEOUT: number = 60 * 2 * 1000;

  constructor(
    private authService: AuthService,
    private router: Router
  ) { 
    this.startMonitoring(); 
  }

   // Cleanup on destroy
  ngOnDestroy(): void {
    this.stopMonitoring(); 
  }
   
  
  // Start tracking inactivity
  startMonitoring(): void {
      this.resetInactivityTimeout();  
      window.addEventListener('mousemove', this.resetInactivityTimeout.bind(this));
      window.addEventListener('keydown', this.resetInactivityTimeout.bind(this));
    }

  // Stop tracking  
  stopMonitoring(): void {
    clearTimeout(this.timeout);  
    clearTimeout(this.warningTimeout); 
    window.removeEventListener('mousemove', this.resetInactivityTimeout.bind(this));
    window.removeEventListener('keydown', this.resetInactivityTimeout.bind(this));
  }

   // Reset both warning and logout timers
   private resetInactivityTimeout(): void {
    clearTimeout(this.timeout);
    clearTimeout(this.warningTimeout);
    this.warningTimeout = setTimeout(() => this.showInactivityWarning(), this.INACTIVITY_TIMEOUT - 60000); // 1minute
    this.timeout = setTimeout(() => this.handleInactivity(), this.INACTIVITY_TIMEOUT); // final : 2 minute
  }

  private showInactivityWarning(): void {
    alert('Your session will expire in 1 minute due to inactivity.');
  }

  // Handle inactivity and logout user
  private handleInactivity(): void {
    this.authService.logout().then(() => {
      console.log('User logged out due to inactivity');
      this.router.navigate(['/home']);
    });
  }

}
