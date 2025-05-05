import { Component, OnInit } from '@angular/core';
import { Course } from '../../models';
import { CourseService } from '../../services/course.service';
import { ImageService } from '../../services/image.service';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { WishlistService } from '../../services/wishlist.service';

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
  wishlist: number[] = [];
  modalLeft = '0px';
  modalTop = '0px';

  constructor(
    private courseService: CourseService,
    private imageService: ImageService,
    private authService: AuthService,
    private wishlistService: WishlistService,
    private router: Router,

  ) { }

  ngOnInit(): void {
    this.getCourses();
    this.getWishlist();
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

  getWishlist(): void {
    if (this.authService.isAuthenticated()) {
      this.wishlistService.wishlist$.subscribe({
        next: (wishlistItems) => {
          this.wishlist = wishlistItems.map(item => item.courseID);
        },
        error: (err) => {
          console.error('Error fetching wishlist', err);
        }
      });
    }
  }

  toggleWishlist(courseID: number, event: MouseEvent): void {
    event.stopPropagation();
    if (!this.authService.isAuthenticated()) {
      alert('Please log in first to add to your wishlist.');
      this.router.navigate(['/login']);
      return;
    }

    if (this.wishlist.includes(courseID)) {
      this.wishlistService.removeCourseFromWishlist(courseID).subscribe({
        next: () => {
          this.wishlist = this.wishlist.filter((id) => id !== courseID);
          alert('Course removed from your wishlist.');
        },
        error: (err) => {
          console.error('Error removing course from wishlist', err);
          alert('Error removing course from wishlist.');
        },
      });
    } else {
      this.wishlistService.addCourseToWishlist(courseID).subscribe({
        next: () => {
          this.wishlist.push(courseID);
          alert('Course added to your wishlist.');
        },
        error: (err) => {
          console.error('Error adding course to wishlist', err);
          alert('Error adding course to wishlist.');
        },
      });
    }
  }

  getCourseImageUrl(imageFilename: string): string {
    return this.imageService.getCourseImageUrl(imageFilename);
  }

  showModal(course: Course, event: MouseEvent): void {
    this.selectedCourse = course;
    const cardElement = (event.target as HTMLElement).closest('.course-wrapper');
    if (cardElement) {
      const cardRect = cardElement.getBoundingClientRect();

      this.modalLeft = `${cardRect.left}px`;
      this.modalTop = `${cardRect.top - cardElement.clientHeight}px`;
    }
  }

  hideModal(): void {
    this.selectedCourse = null;
  }

}

