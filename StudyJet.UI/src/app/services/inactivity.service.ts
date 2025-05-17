import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class InactivityService {
   private boundResetInactivityTimeout!: () => void;
  private timeout: any;
  private warningTimeout: any;
  private readonly INACTIVITY_TIMEOUT: number = 60 * 20 * 1000; // 20 minutes of inactivity

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

   // Cleanup on destroy
  ngOnDestroy(): void {
    this.stopMonitoring(); 
  }
  
  // Start tracking inactivity
startMonitoring(): void {
  this.boundResetInactivityTimeout = this.resetInactivityTimeout.bind(this); // ✅ bind once
  this.resetInactivityTimeout();  
  window.addEventListener('mousemove', this.boundResetInactivityTimeout);
  window.addEventListener('keydown', this.boundResetInactivityTimeout);
}

// Stop tracking
stopMonitoring(): void {
  clearTimeout(this.timeout);  
  clearTimeout(this.warningTimeout); 
  if (this.boundResetInactivityTimeout) {
    window.removeEventListener('mousemove', this.boundResetInactivityTimeout);
    window.removeEventListener('keydown', this.boundResetInactivityTimeout);
  }
}


   // Reset both warning and logout timers
   private resetInactivityTimeout(): void {
    clearTimeout(this.timeout);
    clearTimeout(this.warningTimeout);
    this.timeout = setTimeout(() => this.handleInactivity(), this.INACTIVITY_TIMEOUT); // final : 2 minute
  }

  // Handle inactivity and logout user
  private handleInactivity(): void {
    this.authService.logout().then(() => {
      this.router.navigate(['/home']);
    });
  }
}