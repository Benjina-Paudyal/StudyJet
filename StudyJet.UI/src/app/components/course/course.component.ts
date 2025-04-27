import { Component, OnInit } from '@angular/core';
import { Course } from '../../models/course.model';
import { CourseService } from '../../services/course.service';
import { ImageService } from '../../services/image.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-course',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './course.component.html',
  styleUrl: './course.component.css'
})
export class CourseComponent implements OnInit {
courses: Course[] = [];
selectedCourse: Course | null = null;
modalLeft = '0px';
modalTop = '0px';

constructor(
  private courseService: CourseService,
  private imageService: ImageService,

){}

ngOnInit() : void {
  this.getCourses();
}

// Fetch approved courses
getCourses(): void {
  this.courseService.getApprovedCourses().subscribe({
    next: (courses: Course[]) => {
      this.courses = courses;
    },
    error: (error) => {
      console.error('Error fetching popular courses', error);
    },
  });
}

// Fetch course image URL
getCourseImageUrl(imageFilename: string): string {
  return this.imageService.getCourseImageUrl(imageFilename);
}

// Show modal for a selected course
showModal(course: Course, event: MouseEvent): void {
  this.selectedCourse = course;
  const cardElement = (event.target as HTMLElement).closest('.course-wrapper');
  if (cardElement) {
    const cardRect = cardElement.getBoundingClientRect();

    this.modalLeft = `${cardRect.left}px`;
    this.modalTop = `${cardRect.top - cardElement.clientHeight}px`;
  }
}

// Hide modal
hideModal(): void {
  this.selectedCourse = null;
}

}

