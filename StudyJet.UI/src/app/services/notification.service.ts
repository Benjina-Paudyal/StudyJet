import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = environment.apiBaseUrl;

  constructor(
    private http: HttpClient
  ) { }

  // Fetch notification
  getNotifications(): Observable <Notification[]>{
    return this.http.get<Notification[]>(`${this.apiUrl}/Notification`);
  }

 // Mark notification as read
 markAsRead(notificationId: number): Observable<any> {
  return this.http.put(`${this.apiUrl}/Notification/mark-read/${notificationId}`, {});
}
  
}
