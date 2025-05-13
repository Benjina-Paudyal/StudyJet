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

  // Fetch approved courses from the service
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

  // Fetch the wishlist items if the user is authenticated
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

  // Toggle course in the wishlist (add or remove)
  toggleWishlist(courseID: number, event: MouseEvent): void {
    event.stopPropagation();
    if (!this.authService.isAuthenticated()) {
      alert('Please log in first to add to your wishlist.');
      this.router.navigate(['/login']);
      return;
    }
    // Add or remove course from wishlist based on current state
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

  // Get the URL for a course image based on its filename
  getCourseImageUrl(imageFilename: string): string {
    return this.imageService.getCourseImageUrl(imageFilename);
  }

  // Show modal for a selected course with positioning based on the clicked card
  showModal(course: Course, event: MouseEvent): void {
    this.selectedCourse = course;
    const cardElement = (event.target as HTMLElement).closest('.course-wrapper');
    if (cardElement) {
      const cardRect = cardElement.getBoundingClientRect();

      this.modalLeft = `${cardRect.left}px`;
      this.modalTop = `${cardRect.top - cardElement.clientHeight}px`;
    }
  }

  // Hide modal when called
  hideModal(): void {
    this.selectedCourse = null;
  }
}

