import { Component, OnInit } from '@angular/core';
import { Course } from '../../models';
import { Observable, of } from 'rxjs';
import { PurchaseCourseService } from '../../services/purchase-course.service';
import { Router, RouterModule } from '@angular/router';
import { ImageService } from '../../services/image.service';
import { CookieService } from 'ngx-cookie-service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.css'
})
export class StudentDashboardComponent implements OnInit {
  purchasedCourses: Course[] = [];
  suggestedCourses: Course[] = [];
  profileImageUrl: string | null = null;
  fullName$: Observable<string> = of('');
  userName$: Observable<string> = of('');
  isAuthenticated = false;
  selectedCourse: Course | null = null;
  modalLeft = '105%';
  modalRight = 'auto';
  modalTop = 'auto';
  showFullContent = false;
  
  constructor(
    private purchaseCourseService: PurchaseCourseService,
    private router: Router,
    private imageService: ImageService,
    private cookieService: CookieService,
  ) { }

  ngOnInit(): void {
    const token = this.cookieService.get('authToken');
    const userData = {
      username: this.cookieService.get('username'),
      fullName: this.cookieService.get('fullName'),
      profilePicture: this.cookieService.get('profileImageUrl'),
    };

    if (token && userData) {
      this.isAuthenticated = true;
      this.userName$ = of(userData.username || '');
      this.fullName$ = of(userData.fullName || '');
      this.profileImageUrl = this.imageService.getProfileImageUrl(userData.profilePicture || 'profilepic.png');
      this.purchaseCourseService.fetchPurchaseCourse();
    } else {
      this.isAuthenticated = false;
      this.router.navigate(['/login']);
    }

    // Get purchased courses
    this.purchaseCourseService.getPurchaseCourse().subscribe({
      next: (courses: Course[]) => {
        this.purchasedCourses = courses.map(course => ({
          ...course,
          imageUrl: this.imageService.getCourseImageUrl(course.imageUrl)
        }));

        if (this.purchasedCourses.length === 0) {
          this.loadSuggestedCourses();
        }
      },
      error: (err) => console.error('Error fetching courses:', err)
    });
  }

  loadSuggestedCourses(): void {
    this.purchaseCourseService.getSuggestedCourses().subscribe({
      next: (courses: Course[]) => {
        this.suggestedCourses = courses.map(course => ({
          ...course,
          imageUrl: this.imageService.getCourseImageUrl(course.imageUrl)
        }));
      },
      error: (err) => {
        console.error('Error fetching suggested courses:', err);
        if (err.status === 401) {
          console.warn('Unauthorized access. Redirecting to login...');
          this.router.navigate(['/login']);
        }
      },
    });
  }
} 



