import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { CourseService } from '../../services/course.service';
import { NotificationService } from '../../services/notification.service';
import { UserService } from '../../services/user.service';
import { CookieService } from 'ngx-cookie-service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit{

  studentCount = 0;
  instructorCount = 0;
  courseCount = 0;
  coursesSubscription: Subscription | null = null;
  usersSubscription: Subscription | null = null;
  notificationsSubscription: Subscription | null = null;
  profileImageUrl: string | null = null;
  fullName$: Observable<string> = of('');
  userName$: Observable<string> = of('');
  isAuthenticated = false;
  totalUsers: number | null = null;
  totalCourses: number | null = null;
  enrolledStudentsCount = 0;
  notifications: any[] = [];  
  notificationCount = 0;
  loading = true;
  error: string | null = null;
  authSubscription: Subscription | null = null;

  constructor(
    private router: Router,
    private cookieService: CookieService,
    private userService: UserService,
    private courseService: CourseService,
    private notificationService: NotificationService,
    private authService: AuthService,
  ) { }

  ngOnInit(): void {
     // Subscribe to authentication status
    this.authSubscription = this.authService.isAuthenticated$.subscribe((isAuth) => {
      this.isAuthenticated = isAuth;

      if (isAuth) {
        // If authenticated, load user data from cookies
        const userData = {
          username: this.cookieService.get('username'),
          fullName: this.cookieService.get('fullName'),
        };
        this.userName$ = of(userData.username || '');
        this.fullName$ = of(userData.fullName || '');
        this.profileImageUrl = this.authService.getProfileImage();
        this.fetchCounts();
        this.fetchNotifications();
      } else {
        // If not authenticated, redirect to login page
        this.router.navigate(['/login']);
      }
    });
  }

  ngOnDestroy(): void {
    // Unsubscribe from all subscriptions on component destruction
    this.authSubscription?.unsubscribe();
    this.coursesSubscription?.unsubscribe();
    this.usersSubscription?.unsubscribe();
    this.notificationsSubscription?.unsubscribe();
  }
  
  // Fetch counts of students, instructors, and courses
  fetchCounts(): void {
    this.userService.getStudentCount().subscribe({
      next: (count) => {
        this.studentCount = count;
      },
      error: (err) => {
        console.error('Error fetching student count', err);
        this.error = 'Failed to load student count';
      }
    });
    this.userService.getInstructorCount().subscribe({
      next: (count) => {
        this.instructorCount = count;
      },
      error: (err) => {
        console.error('Error fetching instructor count', err);
        this.error = 'Failed to load instructor count';
      }
    });
    this.courseService.getCourseCount().subscribe({
      next: (count) => {
        this.courseCount = count;
      },
      error: (err) => {
        console.error('Error fetching course count', err);
        this.error = 'Failed to load course count';
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

   // Fetch notifications and calculate unread count
  fetchNotifications(): void {
    this.notificationsSubscription = this.notificationService.getNotifications().subscribe(
      (notifications) => {
        this.notifications = notifications;
        this.notificationCount = notifications.filter((n) => !n.isRead).length;
      },
      (error) => {
        console.error('Error fetching notifications:', error);
        this.error = 'Failed to load notifications';
      }
    );
  }
}