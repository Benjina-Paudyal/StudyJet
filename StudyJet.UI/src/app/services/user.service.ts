import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { BehaviorSubject, catchError, map, Observable, of, tap } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';
import { ImageService } from './image.service';
import { CourseWithStudents, Student, User } from '../models';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = `${environment.apiBaseUrl}`;
  public currentUser: any = null;
  public profileImageUrl: string | null = null;
  private userSubject = new BehaviorSubject<any>(null);
  user$ = this.userSubject.asObservable();
  private wishlistSubject = new BehaviorSubject<any[]>([]);
  wishlist$ = this.wishlistSubject.asObservable();

  constructor(
    private http: HttpClient,
  ) {}

 // Check if the provided email already exists in the system
  checkEmailExists(email: string): Observable<boolean> {
    const url = `${this.apiUrl}/User/email-exists?email=${encodeURIComponent(
      email
    )}`;
    return this.http.get<{ emailExists: boolean }>(url).pipe(
      map((response) => response.emailExists),
      catchError(() => of(false))
    );
  }

  // Check if the provided username already exists in the system
  checkUsernameExists(username: string): Observable<boolean> {
    const url = `${
      this.apiUrl
    }/User/username-exists?username=${encodeURIComponent(username)}`;
    return this.http.get<{ usernameExists: boolean }>(url).pipe(
      map((response) => response.usernameExists),
      catchError(() => of(false))
    );
  }

   // Set the current user in the BehaviorSubject
  setUser(user: any) {
    this.userSubject.next(user);
  }

  // Clear the current user data
  clearUser(): void {
    this.userSubject.next(null);
  }

  // Fetch the wishlist for the current user
  getWishlistForCurrentUser(): Observable<any[]> {
    const url = `${this.apiUrl}/Wishlist`;
    return this.http
      .get<any[]>(url)
      .pipe(catchError(this.handleError<any[]>('getWishlistForCurrentUser')));
  }

  // Set the wishlist (after fetching)
   setWishlist(wishlist: any[]): void {
    this.wishlistSubject.next(wishlist);
  }

  // Clear the wishlist
  clearWishlist(): void {
    this.wishlistSubject.next([]);
  }

   // Fetch total student count
   getStudentCount(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/User/count-students`).pipe(
      catchError(this.handleError<number>('getStudentCount', 0))
    );
  }

  // Fetch total instructor count
  getInstructorCount(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/User/count-instructors`).pipe(
      catchError(this.handleError<number>('getInstructorCount', 0))
    );
  }

  // Fetch total students 
  getStudents(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/User/GetUsersByRole/student`).pipe(
      catchError(this.handleError<User[]>('getStudents', []))
    );
  }

  // Fetch total instructors
  getInstructors(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/User/GetUsersByRole/instructor`).pipe(
      map(instructors => instructors.map(inst => ({
        ...inst,
        createdCourses: inst.createdCourses || [] 
      }))),
      catchError(this.handleError<User[]>('getInstructors', []))
    );
  }

  // Fetch only the enrolled students count
  getEnrolledUsersCountForInstructor(): Observable<number> {
    return this.http.get<{ count: number; students: Student[] }>(
      `${this.apiUrl}/course/instructor/courses/students`
    ).pipe(
      map(response => response.count)
    );
  }

   // Fetch courses with students for instructor
  getCoursesWithStudentsForInstructor(): Observable<CourseWithStudents[]> {
    return this.http.get<CourseWithStudents[]>(`${this.apiUrl}/Course/instructor/courses/students`);
  }
  
  // Generic Error Handling
  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(`${operation} failed: ${error.message}`);
      return of(result as T);
    };
  }
}
