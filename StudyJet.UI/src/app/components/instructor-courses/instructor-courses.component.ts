import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Course } from '../../models';
import { CourseService } from '../../services/course.service';

@Component({
  selector: 'app-instructor-courses',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './instructor-courses.component.html',
  styleUrl: './instructor-courses.component.css'
})
export class InstructorCoursesComponent implements OnInit {
  courses: Course[] = [];
  errorMessage = '';
  loading = true;
  private imageBaseUrl = environment.imageBaseUrl;
  isEditMode = false;
  selectedCourse: Course | null = null;
  totalCourses: number | null = null;
  error: string | null = null;


  constructor(
    private courseService: CourseService,
    private router: Router,
  ) { }

  ngOnInit(): void {
    this.fetchCourses();
    this.loadTotalCourses();
  }

  fetchCourses(): void {
    this.courseService.getCoursesByInstructor().subscribe({
      next: (courses: Course[]) => {
        this.courses = courses.map((course) => {
          // Map numeric status to string status
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
          course.imageUrl = `${this.imageBaseUrl}${course.imageUrl}`;
          return course;
        });
      },
      error: (err) => console.error('Error fetching courses', err)
    });
  }

  // Submit the form (Add or Update)
  onSubmit(): void {
    if (!this.selectedCourse) {
      alert('No course selected.');
      return;
    }

    if (!this.selectedCourse.status) {
      this.selectedCourse.status = "Pending";
    }

    if (this.isEditMode) {
      this.courseService.updateCourse(this.selectedCourse.courseID, this.selectedCourse).subscribe({
        next: () => {
          this.fetchCourses();
          alert('Course updated successfully!');
        },
        error: (err) => {
          console.error('Error updating course:', err);
          alert('Error updating course.');
        }
      });
    } else {
      this.courseService.createCourse(this.selectedCourse).subscribe({
        next: () => {
          this.fetchCourses();
          alert('Course added successfully!');
        },
        error: (err) => {
          console.error('Error adding course:', err);
          alert('Error adding course.');
        }
      });
    }
  }

  loadTotalCourses(): void {
    this.courseService.getTotalCoursesForInstructor().subscribe(
      (response: { totalCourses: number }) => {
        if (response && response.totalCourses !== undefined) {
          this.totalCourses = response.totalCourses;
        } else {
          this.totalCourses = 0;
        }
        this.loading = false;
      },
      (error) => {
        console.error('Error fetching total courses:', error);
        this.error = "Failed to fetch courses.";
        this.loading = false;
      }
    );
  }

  updateCourse(courseId: number): void {
    this.router.navigate(['/update-course', courseId]);
  }

  navigateToAddCourse(): void {
    this.router.navigate(['/add-course']);
  }

}


