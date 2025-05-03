import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Course } from '../../models';
import { AuthService } from '../../services/auth.service';
import { CourseService } from '../../services/course.service';

@Component({
  selector: 'app-manage-courses',
  standalone: true,
  imports: [CommonModule,RouterLink],
  templateUrl: './manage-courses.component.html',
  styleUrl: './manage-courses.component.css'
})
export class ManageCoursesComponent implements OnInit {

  courses: Course[] = [];
  pendingCourses: Course[] = [];
  isAdmin = false;
  isApproving = false;
  isRejecting = false;

  constructor(
    private courseService: CourseService,
    private authService: AuthService,
  ) { }

  ngOnInit(): void {
    this.getCourses();
    this.loadPendingCourses();
    this.checkAdminRole();
  }

  getCourses(): void {
    this.courseService.getAllCourses().subscribe({
      next: (courses: Course[]) => {
        this.courses = courses.map(course => {
          // Map numeric status (0, 1, 2) to string status ('Pending', 'Approved', 'Rejected')
          switch (course.status) {
            case 0:
              course.status = 'Pending';
              break;
            case 1:
              course.status = 'Approved';
              break;
            case 2:
              course.status = 'Rejected';
              break;
          }
  
          // Update image URL if needed (assuming you need full URL for images)
          course.imageUrl = `${environment.imageBaseUrl}${course.imageUrl}`;
          return course;
        });
      },
      error: (err) => console.error('Error fetching courses', err),
    });
  }
  
  checkAdminRole(): void {
    this.authService.getRoles().subscribe((roles) => {
      this.isAdmin = roles.includes('Admin');
    });
  }

  loadPendingCourses(): void {
    this.courseService.getPendingCourses().subscribe(courses => {
      this.pendingCourses = courses.filter(course => course.status === 'Pending');
    });
  }


approveCourse(course: Course): void {
  if (this.isApproving) return;
  
  const message = course.isUpdate 
    ? 'Approve these course updates?'
    : 'Approve this new course?';

  if (!confirm(message)) return;

  this.isApproving = true;
  const approval$ = course.isUpdate
    ? this.courseService.approveCourseUpdate(course.courseID)
    : this.courseService.approveCourse(course.courseID);

  approval$.subscribe({
    next: () => {
      course.status = 'Approved';
      this.pendingCourses = this.pendingCourses.filter(c => c.courseID !== course.courseID);
      alert(course.isUpdate ? 'Update approved!' : 'Course approved!');
    },
    error: (err) => alert('Error: ' + (err.error?.message || 'Approval failed')),
    complete: () => this.isApproving = false
  });
}



rejectCourse(course: Course): void {
  if (this.isRejecting) return;

  const message = course.isUpdate
    ? 'Reject these course updates?'
    : 'Reject this new course?';

  if (!confirm(message)) return;

  this.isRejecting = true;
  const rejection$ = course.isUpdate
    ? this.courseService.rejectCourseUpdate(course.courseID)
    : this.courseService.rejectCourse(course.courseID);

  rejection$.subscribe({
    next: () => {
      course.status = 'Rejected';
      this.pendingCourses = this.pendingCourses.filter(c => c.courseID !== course.courseID);
      alert(course.isUpdate ? 'Update rejected!' : 'Course rejected!');
    },
    error: (err) => alert('Error: ' + (err.error?.message || 'Rejection failed')),
    complete: () => this.isRejecting = false
  });
}
}


