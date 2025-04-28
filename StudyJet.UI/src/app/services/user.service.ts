import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { BehaviorSubject, catchError, map, Observable, of } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiBaseUrl}`;
  public currentUser: any = null;
  public profileImageUrl: string | null = null;
  private userSubject = new BehaviorSubject<any>(null);
  user$ = this.userSubject.asObservable();
  constructor(
    private http: HttpClient,
    private cookieService: CookieService,
  ) { }

  private getAuthHeaders(): HttpHeaders {
    const token = this.cookieService.get('authToken');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
    });
  }

  // Check if email exists
  checkEmailExists(email: string): Observable<boolean> {
    const url = `${this.apiUrl}/User/email-exists?email=${encodeURIComponent(email)}`;
    return this.http.get<{ emailExists: boolean }>(url).pipe(
      map(response => response.emailExists),
      catchError(() => of(false))
    );
  }

  // Check if username exists
  checkUsernameExists(username: string): Observable<boolean> {
    const url = `${this.apiUrl}/User/username-exists?username=${encodeURIComponent(username)}`;
    return this.http.get<{ usernameExists: boolean }>(url).pipe(
      map(response => response.usernameExists),
      catchError(() => of(false))
    );
  }


  setUser(user: any) {
    console.log('Setting user in UserService:', user);
    this.userSubject.next(user);
  }

  clearUser(): void {
    this.userSubject.next(null);
  }

}
