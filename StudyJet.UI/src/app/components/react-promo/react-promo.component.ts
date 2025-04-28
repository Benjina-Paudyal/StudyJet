import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Course } from '../../models';
import { CourseService } from '../../services/course.service';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-react-promo',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './react-promo.component.html',
  styleUrl: './react-promo.component.css',
})
export class ReactPromoComponent implements OnInit {
  reactCourse: Course | null = null;

  constructor(
    private courseService: CourseService,
    private imageService: ImageService
  ) {}

  ngOnInit(): void {
    this.fetchReactCourse();
  }

  fetchReactCourse(): void {
    this.courseService.getCourseById(3).subscribe({
      next: (course: Course) => {
        this.reactCourse = course;
      },
      error: (error) => {
        console.error('Error fetching React course', error);
      },
    });
  }

  getCourseImageUrl(imageFilename: string): string {
    return this.imageService.getCourseImageUrl(imageFilename);
  }
}
