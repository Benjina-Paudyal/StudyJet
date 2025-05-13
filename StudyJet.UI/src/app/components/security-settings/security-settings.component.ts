import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { BehaviorSubject, Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { CommonModule } from '@angular/common';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-security-settings',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './security-settings.component.html',
  styleUrl: './security-settings.component.css'
})
export class SecuritySettingsComponent implements OnInit, OnDestroy {

  profileImageUrl: string | null = null;
  userName$ = new BehaviorSubject<string>('');
  private readonly unsubscribe$ = new Subject<void>();

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private cookieService: CookieService,
    private router: Router,
  ) { }

  ngOnInit(): void {

    if (this.authService.isAuthenticated()) {
      this.profileImageUrl = this.authService.getProfileImage();
    }

    const username = this.cookieService.get('username');
    if (username) {
      this.userName$.next(username); 
    }

    // Subscribe to user service for real-time updates (if needed)
    this.userService.user$
      .pipe(takeUntil(this.unsubscribe$))
      .subscribe((user) => {
        if (user) {
          if (user.username) {
            this.userName$.next(user.username); 
          }
          if (user.profilePicture) {
            this.profileImageUrl = this.authService.getProfileImage();
          }
        }
        this.cdr.detectChanges();
      });
  }


  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

  goToChangePassword(): void {
    this.router.navigate(['/security-settings/change-password']);
  }
}



