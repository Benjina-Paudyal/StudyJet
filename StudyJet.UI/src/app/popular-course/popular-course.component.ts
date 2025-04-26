import { Component, OnInit } from '@angular/core';
import { Course } from '../models/course.model';
import { CourseService } from '../services/course.service';
import { ImageService } from '../services/image.service';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-popular-course',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './popular-course.component.html',
  styleUrl: './popular-course.component.css',
})
export class PopularCourseComponent implements OnInit {
  courses: Course[] = [];
  popularCourses: Course[] = [];
  selectedCourse: Course | null = null;
  modalLeft = '105%';
  modalRight = 'auto';
  modalTop = 'auto';
  showFullContent = false;

  constructor(
    private courseService: CourseService,
    private imageService: ImageService
  ) {}

  ngOnInit(): void {
    this.fetchPopularCourses();
  }

  // Fetch popular courses
  fetchPopularCourses(): void {
    this.courseService.getPopularCourses().subscribe({
      next: (courses: Course[]) => {
        this.popularCourses = courses;
      },
      error: (error) => {
        console.error('Error fetching popular courses', error);
      },
    });
  }

  getCourseImageUrl(imageFilename: string): string {
    return this.imageService.getCourseImageUrl(imageFilename);
  }

  showModal(course: Course): void {
    this.selectedCourse = course;
  }

  hideModal(): void {
    this.selectedCourse = null;
  }
  toggleModalContent(course: Course): void {
    this.selectedCourse = this.selectedCourse === course ? null : course;
    this.showFullContent = this.selectedCourse !== null;
  }
}
