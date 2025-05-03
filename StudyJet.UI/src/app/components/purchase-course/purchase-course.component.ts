import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Course } from '../../models';
import { PurchaseCourseService } from '../../services/purchase-course.service';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-purchase-course',
  standalone: true,
  imports: [CommonModule,RouterLink],
  templateUrl: './purchase-course.component.html',
  styleUrl: './purchase-course.component.css'
})
export class PurchaseCourseComponent {
  purchasedCourses: Course[] = [];  

  constructor(
    private purchaseCourseService: PurchaseCourseService,
    private imageService: ImageService,
    private router: Router,
) { }

ngOnInit(): void {
  // Fetch purchased courses on component load
  this.purchaseCourseService.fetchPurchaseCourse();
  this.getPurchaseCourse();
}

// Get purchased courses
getPurchaseCourse(): void {
  this.purchaseCourseService.getPurchaseCourse().subscribe({
    next: (courses: Course[]) => {
      this.purchasedCourses = courses.map(course => ({
        ...course,
        imageUrl: this.imageService.getCourseImageUrl(course.imageUrl)
      }));  
    },
    error: (err) => {
      console.error('Error fetching purchased courses:', err);
    }
  });
}

trackByCourseId(index: number, course: Course): number {
  return course.courseID;
}

navigateToDetail(courseID: number): void {
  this.router.navigate(['/courses', courseID]);
}

isCoursePurchased(courseId: number): boolean {
  return this.purchasedCourses.some(course => course.courseID === courseId);
}

proceedToCheckout(courseId: number): void {
  this.purchaseCourseService.createCheckoutSession(courseId).subscribe({
    next: (response) => {
      if (response.url) {
        window.location.href = response.url; 
      }
    },
    error: (err) => {
      console.error('Error creating checkout session:', err);
    }
  });
}
}



