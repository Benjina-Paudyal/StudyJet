import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../../services/notification.service';
import { CommonModule } from '@angular/common';
import { Notification } from '../../models';


@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css'
})
export class NotificationComponent implements OnInit {
  notifications: Notification[] = [];
  unreadNotificationsCount = 0;
  loading = false;
  error: string | null = null;

  constructor(
    private notificationService: NotificationService,
    private router: Router
  ) {}
  ngOnInit(): void {
    this.loadNotifications()
}

loadNotifications(): void {
  this.loading = true;
  this.error = null;

  this.notificationService.getNotifications().subscribe({
    next: (notifications: Notification[]) => {
      // Convert date string to Date object for easier formatting in the template
      this.notifications = notifications.map(notification => ({
        ...notification,
        dateCreated: new Date(notification.dateCreated)
      }));

      // Count and update unread notifications
       this.unreadNotificationsCount = this.notifications.filter(n => !n.isRead).length;
       this.notificationService.updateUnreadNotificationsCount(this.notifications);
      
      this.loading = false;
    },
    error: (err) => {
      console.error('Error fetching notifications:', err);
      this.error = 'Failed to load notifications.';
      this.loading = false;
    }
  });
}
  
markAsRead(): void {
  // Mark all unread notifications as read
  this.notifications.forEach(notification => {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe(
        () => {
          notification.isRead = true;
        },
        (error) => {
          console.error('Error marking notification as read:', error);
        }
      );
    }
  });
  // Update unread count in the shared service
  this.notificationService.updateUnreadNotificationsCount(this.notifications);
}


handleNotificationClick(notification: Notification): void {
  const courseId = notification.courseID;  
  // Navigate to course detail page if courseID is available
  if (courseId) {
    this.router.navigate(['/courses', courseId]);
  } 
 
  // Mark clicked notification as read if it isn't already
  if (!notification.isRead) {
    this.notificationService.markAsRead(notification.id).subscribe(() => {
      notification.isRead = true;
      this.unreadNotificationsCount--;
       this.notificationService.updateUnreadNotificationsCount(this.notifications);
    });
  }
}

}





