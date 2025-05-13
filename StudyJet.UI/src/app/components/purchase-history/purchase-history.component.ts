import { Component } from '@angular/core';
import { Course } from '../../models';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { PurchaseCourseService } from '../../services/purchase-course.service';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-purchase-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './purchase-history.component.html',
  styleUrl: './purchase-history.component.css'
})
export class PurchaseHistoryComponent {

  purchasedCourses: Course[] = [];  

  constructor(
    private purchaseCourseService: PurchaseCourseService, 
    private imageService: ImageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.purchaseCourseService.fetchPurchaseCourse();
    this.getPurchaseCourse();
  }

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
  
  navigateToDetail(courseID: number): void {
    this.router.navigate(['/courses', courseID]);
  }
   // track the items
   trackByCourseId(index: number, course: Course): number {
    return course.courseID;
  }
}
  

