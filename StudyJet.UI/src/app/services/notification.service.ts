import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Notification } from '../models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = environment.apiBaseUrl;
  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(
    private http: HttpClient
  ) { }

  // Fetch all notifications
  getNotifications(): Observable <Notification[]>{
    return this.http.get<Notification[]>(`${this.apiUrl}/Notification`);
  }

 // Mark specific notification as read
 markAsRead(notificationId: number): Observable<any> {
  return this.http.put(`${this.apiUrl}/Notification/mark-read/${notificationId}`, {});
}

 // Updates unread notifications count
updateUnreadNotificationsCount(notifications: Notification[] | null | undefined): void {
  const unreadCount = (notifications ?? []).filter(n => !n.isRead).length;
  this.unreadCountSubject.next(unreadCount); 
}
}
