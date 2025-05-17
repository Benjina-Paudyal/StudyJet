import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Course } from '../models';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';
import { ImageService } from './image.service';

@Injectable({
  providedIn: 'root'
})
export class PurchaseCourseService {
  private purchasedCoursesSubject = new BehaviorSubject<Course[]>([]);
  purchasedCourses$ = this.purchasedCoursesSubject.asObservable();
  private purchasedCourseUrl = `${environment.apiBaseUrl}/user/purchases/my-courses`;
  private suggestedCourseUrl = `${environment.apiBaseUrl}/user/purchases/suggested-courses`;

  constructor(
    private http: HttpClient,
    private cookieService: CookieService,
    private imageService: ImageService
  ) { }

  // Fetch purchased courses
  fetchPurchaseCourse(): void {
    const token = this.cookieService.get('authToken');
    if (!token) {
      console.error("No token found, user might not be logged in.");
      return;
    }
    
this.http.get<Course[]>(this.purchasedCourseUrl).subscribe({
  next: (courses) => {
    if (!Array.isArray(courses)) {
      console.error('Unexpected response format, expected array but got:', courses);
      this.purchasedCoursesSubject.next([]);
      return;
    }
    
    if (courses.length === 0) {
      // console.warn("No purchased courses found!");
      this.purchasedCoursesSubject.next([]);
      return;
    }
    
    const updatedCourses = courses.map(course => ({
      ...course,
      imageUrl: this.imageService.getCourseImageUrl(course.imageUrl) 
    }));
    this.purchasedCoursesSubject.next(updatedCourses);
  },
  error: (err) => {
    console.error('Error fetching purchased courses:', err);
  }
});

  }

   // Observable to listen for purchased courses
   getPurchaseCourse(): Observable<Course[]> {
    return this.purchasedCoursesSubject.asObservable();
  }

  // Synchronous check if a course is purchased
  isCoursePurchased(courseId: number): boolean {
    return this.purchasedCoursesSubject.value.some(course => course.courseID === courseId);  
  }

  // Suggested Courses
  getSuggestedCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(this.suggestedCourseUrl);
  }

  // Create checkout session for a course
  createCheckoutSession(courseId: number): Observable<{ url: string }> {
    return this.http.post<{ url: string }>(
      `${environment.apiBaseUrl}/user/purchases/create-checkout-session`, 
      { courseId }
    );
  }
}



