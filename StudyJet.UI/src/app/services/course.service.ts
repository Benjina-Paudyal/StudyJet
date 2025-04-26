import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Course } from '../models/course.model';

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private courseUrl = `${environment.apiBaseUrl}/courses`; 
  private popularCourseUrl = `${this.courseUrl}/popular`; 

  constructor(private http: HttpClient) { }

  // Fetch popular courses
  getPopularCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(this.popularCourseUrl);
  }

  // Fetch course by ID
  getCourseById(courseId: number): Observable<Course>{
    const url = `${this.courseUrl}/${courseId}`;
    return this.http.get<Course>(url);
  }
 }
