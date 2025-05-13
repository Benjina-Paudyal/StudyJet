import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { ImageService } from '../../services/image.service';
import { CookieService } from 'ngx-cookie-service';
import { UserService } from '../../services/user.service';
import { CourseService } from '../../services/course.service';
import { NotificationService } from '../../services/notification.service';
import { Course, User, Notification } from '../../models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-instructor-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './instructor-dashboard.component.html',
  styleUrl: './instructor-dashboard.component.css'
})
export class InstructorDashboardComponent implements OnInit {
  coursesSubscription: Subscription | null = null;
  notificationsSubscription: Subscription | null = null;
  profileImageUrl: string | null = null;
  fullName$: Observable<string> = of('');
  userName$: Observable<string> = of('');
  isAuthenticated = false;
  courses: Course[] = [];
  totalCourses: number | null = null;
  instructorId: string | null = null;
  enrolledStudentsCount = 0;
  enrolledStudents: User[] = [];
  notificationCount = 0;
  loading = true;
  error: string | null = null;
  showStudentList = false;
  loadEnrolledStudetns = false;
  notifications: Notification[] = [];
  authSubscription: Subscription | null = null;


  constructor(
    private router: Router,
    private imageService: ImageService,
    private cookieService: CookieService,
    private userService: UserService,
    private courseService: CourseService,
    private authService: AuthService,
    private notificationService: NotificationService,
  ) { }

  ngOnInit(): void {
    this.authSubscription = this.authService.isAuthenticated$.subscribe((isAuth) => {
      this.isAuthenticated = isAuth;
  
      if (isAuth) {
        const userData = {
          username: this.cookieService.get('username'),
          fullName: this.cookieService.get('fullName'),
          profilePicture: this.cookieService.get('profileImageUrl'),
          instructorId: this.cookieService.get('userId'),  
        };
         // If instructor ID is missing, redirect to login
      if (!userData.instructorId) {
        console.error('Instructor ID is missing.');
        this.router.navigate(['/login']);
        return;
      }
    
      // set user data
      this.userName$ = of(userData.username || '');
      this.fullName$ = of(userData.fullName || '');
      this.profileImageUrl = this.imageService.getProfileImageUrl(userData.profilePicture || 'profilepic.png');
      this.instructorId = userData.instructorId || null;


        this.loadCourses();
        this.loadEnrolledStudentsCount();
        this.loadTotalCourses();
      } else {
      this.isAuthenticated = false;
      this.router.navigate(['/login']);
    }
    });
  }

  ngOnDestroy(): void {
    this.coursesSubscription?.unsubscribe();
    this.authSubscription?.unsubscribe();
    this.notificationsSubscription?.unsubscribe();
  }

  // Fetch courses for the instructor
  loadCourses(): void {
    this.coursesSubscription = this.courseService.getCoursesByInstructor().subscribe({
      next: (response: Course[]) => {
        if (Array.isArray(response)) {
          this.courses = response;
        } else {
          this.courses = [];
        }
      },
      error: (error) => {
        console.error('Error fetching courses:', error);
      }
    });
  }

  // Fetch total number of courses
  loadTotalCourses(): void {
    this.courseService.getTotalCoursesForInstructor().subscribe({
      next: (response: { totalCourses: number }) => {
        if (response && response.totalCourses !== undefined) {
          this.totalCourses = response.totalCourses;
        } else {
          this.totalCourses = 0;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error fetching total courses:', error);
        this.error = "Failed to fetch courses.";
        this.loading = false;
      }
    });
  }

  // Fetch the total count of enrolled students
  loadEnrolledStudentsCount(): void {
    this.userService.getEnrolledUsersCountForInstructor().subscribe({
      next: (count) => {
        this.enrolledStudentsCount = count;
      },
      error: (error) => {
        console.error('Error fetching enrolled students count:', error);
        this.error = 'Failed to fetch enrolled students count.';
      }
    });
  }

  // Fetch the total count of unread notifications
  loadNotificationCount(): void {
    this.notificationService.getNotifications().subscribe({
      next: (notifications: Notification[]) => {
        this.notificationCount = notifications.filter(n => !n.isRead).length;
      },
      error: (error) => {
        console.error('Error fetching notifications:', error);
        this.error = 'Failed to fetch notifications.';
      }
    });
  }


  // Toggle the visibility of enrolled students list
  toggleStudentList(): void {
    this.showStudentList = !this.showStudentList;
  }

  // Fetch notifications and update the unread count
  loadNotificationsAndUpdateUnreadCount(): void {
    this.notificationService.getNotifications().subscribe({
      next: (notifications: Notification[]) => {
        this.notifications = notifications;
        this.notificationCount = notifications.filter(n => !n.isRead).length;
      },
      error: (error) => {
        console.error('Error fetching notifications:', error);
        this.error = 'Failed to fetch notifications.';
      }
    });
  }
}


