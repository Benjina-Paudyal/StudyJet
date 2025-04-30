import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient} from '@angular/common/http';
import { catchError, map, Observable, of, tap, throwError } from 'rxjs';
import { Course } from '../models/course/course.model';
import { ImageService } from './image.service';

@Injectable({
  providedIn: 'root',
})
export class CourseService {
  private apiUrl = `${environment.apiBaseUrl}`;
  private endpoints = {
    courseUrl: `${this.apiUrl}/course`,         
    popularCourses: `${this.apiUrl}/popular`,   
    approvedCourses: `${this.apiUrl}/approved`, 
    purchasedCourses: `${this.apiUrl}/user/purchases`, 
  };

  constructor(
    private http: HttpClient,
    private imageService: ImageService
  ){}

  // Fetch all courses
  getAllCourses(): Observable<Course[]> {
    return this.http
      .get<Course[]>(this.endpoints.courseUrl)  
      .pipe(catchError(this.handleError<Course[]>('getAllCourses', [])));
  }

  // Fetch popular courses
  getPopularCourses(): Observable<Course[]> {
    return this.http
      .get<Course[]>(this.endpoints.popularCourses)
      .pipe(catchError(this.handleError<Course[]>('getPopularCourses', [])));
  }

  // Fetch course by ID
  getCourseById(courseId: number): Observable<Course> {
    const url = `${this.endpoints.courseUrl}/${courseId}`;
    return this.http
      .get<Course>(url)
      .pipe(catchError(this.handleError<Course>('getCourseById')));
  }

  // Fetch approved courses
  getApprovedCourses(): Observable<Course[]> {
    return this.http
      .get<Course[]>(this.endpoints.approvedCourses)
      .pipe(catchError(this.handleError<Course[]>('getApprovedCourses', [])));
  }

  // Search courses
  searchCourses(query: string): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.endpoints.courseUrl}/search?query=${encodeURIComponent(query)}`)
      .pipe(
        // Optional transformation if needed
        map(courses => courses.map(course => ({
          ...course,
          instructorName: course.instructorName || 'Unknown' 
        }))),
        catchError(this.handleError<Course[]>('searchCourses', []))
      );
  }
  

  // Fetch total course count
  getCourseCount(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/courses/GetTotalCourses`).pipe(
      catchError(this.handleError<number>('getCourseCount', 0))
    );
  }
  
  
 // Approve new course
  approveCourse(courseId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/courses/approve/${courseId}`, {});
  }
  
// Approve course update
approveCourseUpdate(courseId: number): Observable<any> {
  if (courseId === null || courseId === undefined || isNaN(courseId)) {
    console.error('Invalid courseId passed:', courseId);
    return throwError(() => new Error('Invalid course ID provided'));
  }
  return this.http.put(`${this.apiUrl}/courses/approve-update/${courseId}`, {});
}

// Get pending courses
getPendingCourses(): Observable<Course[]> {
  return this.http
    .get<Course[]>(`${this.apiUrl}/courses/pending`)
    .pipe(catchError(this.handleError<Course[]>('getPendingCourses', [])));
}


// Get courses by instructor
getCoursesByInstructor(): Observable<Course[]> {
  console.log('Fetching courses for instructor...');
  return this.http.get<Course[]>(`${this.apiUrl}/courses/course-by-instructor`).pipe(
    tap((response: Course[]) => console.log('API Response:', response)),
    catchError(this.handleError<Course[]>('getCoursesByInstructor', []))
  );
}

// Get the total number of courses for the instructor
getTotalCoursesForInstructor(): Observable<{ totalCourses: number }> {
  return this.http.get<{ totalCourses: number }>(
    `${this.apiUrl}/courses/total-courses-by-instructor`
  ).pipe(
    catchError(this.handleError<{ totalCourses: number }>('getTotalCoursesForInstructor', { totalCourses: 0 }))
  );
}

// Add a new course
createCourse(courseData: any): Observable<any> {
  // Ensure course status is explicitly set to 'Pending'
  courseData.status = "Pending";
  return this.http.post<any>(`${this.apiUrl}/courses/create`, courseData)
    .pipe(
      catchError(this.handleError<any>('createCourse'))
    );
}

// Update an existing course
updateCourse(courseId: number, courseData: any): Observable<any> {
  return this.http.post<any>(
    `${this.apiUrl}/courses/${courseId}/submitUpdate`, 
    courseData
  ).pipe(
    catchError(this.handleError<any>('updateCourse'))
  );
}

// Reject a course
rejectCourse(courseId: number): Observable<any> {
  return this.http.put<any>(
    `${this.apiUrl}/courses/reject/${courseId}`, 
    {}
  ).pipe(
    catchError(this.handleError<any>('rejectCourse'))
  );
}

// Reject a course update
rejectCourseUpdate(courseId: number): Observable<any> {
  return this.http.post<any>(
    `${this.apiUrl}/courses/reject-updates/${courseId}`, 
    {}
  ).pipe(
    catchError(this.handleError<any>('rejectCourseUpdate'))
  );
}

// Error handling
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      const userFriendlyMessage = 'An error occurred. Please try again later.';
      console.error(`${operation} failed: ${error.message}`);
      console.error('Full error details:', error);
      return of(result as T); // Return a default value to keep app running
    };
   }
   
}
