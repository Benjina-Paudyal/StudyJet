import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { ForgotPasswordResponse } from '../models';
import { catchError, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ForgotPasswordService {
  private apiUrl = `${environment.apiBaseUrl}/Auth`;

  constructor(
    private http: HttpClient
  ) { }

  // Request a password reset
  requestPasswordReset(email: string): Observable<ForgotPasswordResponse> {
    return this.http.post<ForgotPasswordResponse>(`${this.apiUrl}/forgot-password`, { email })
      .pipe(
        catchError(error => {
          console.error('Error requesting password reset:', error);
          throw error; 
        })
      );
  }
}
