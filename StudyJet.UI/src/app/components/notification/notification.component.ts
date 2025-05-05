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
      console.log('Notifications:', notifications);
      this.notifications = notifications.map(notification => ({
        ...notification,
        dateCreated: new Date(notification.dateCreated)
      }));
       // Update unread notifications count in the service
       this.notificationService.updateUnreadNotificationsCount(this.notifications);
      this.unreadNotificationsCount = this.notifications.filter(n => !n.isRead).length;
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

  this.notificationService.updateUnreadNotificationsCount(this.notifications);
}


handleNotificationClick(notification: Notification): void {
  console.log('Notification clicked:', notification);
  const courseId = notification.courseID;  

  if (courseId) {
    this.router.navigate(['/courses', courseId]);
  } else {
    console.log('No courseId found for notification');
  }
 
  if (!notification.isRead) {
    this.notificationService.markAsRead(notification.id).subscribe(() => {
      notification.isRead = true;
      this.unreadNotificationsCount--;
    });
  }
}



}





