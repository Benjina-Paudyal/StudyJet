import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, of } from 'rxjs';
import { Course } from '../models/course.model';

@Injectable({
  providedIn: 'root',
})
export class CourseService {
  private courseUrl = `${environment.apiBaseUrl}/courses`;
  private popularCourseUrl = `${this.courseUrl}/popular`;

  constructor(
    private http: HttpClient
  )

     {}

  // Fetch popular courses
  getPopularCourses(): Observable<Course[]> {
    return this.http
      .get<Course[]>(this.popularCourseUrl)
      .pipe(catchError(this.handleError<Course[]>('getPopularCourses', [])));
  }

  // Fetch course by ID
  getCourseById(courseId: number): Observable<Course> {
    const url = `${this.courseUrl}/${courseId}`;
    return this.http
      .get<Course>(url)
      .pipe(catchError(this.handleError<Course>('getCourseById')));
  }

  // Get approved courses
  getApprovedCourses(): Observable<Course[]> {
    return this.http
      .get<Course[]>(`${this.courseUrl}/approved`)
      .pipe(catchError(this.handleError<Course[]>('getApprovedCourses', [])));
  }

  // Error handling
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(`${operation} failed: ${error.message}`);
      console.error('Full error details:', error);
      return of(result as T);
    };
  }
}
