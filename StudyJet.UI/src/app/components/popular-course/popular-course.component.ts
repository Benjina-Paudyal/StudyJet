import { Component, OnInit } from '@angular/core';
import { Course } from '../../models';
import { CourseService } from '../../services/course.service';
import { ImageService } from '../../services/image.service';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';
import { WishlistService } from '../../services/wishlist.service';

@Component({
  selector: 'app-popular-course',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './popular-course.component.html',
  styleUrl: './popular-course.component.css',
})
export class PopularCourseComponent implements OnInit {
  popularCourses: Course[] = [];
  selectedCourse: Course | null = null;
  modalLeft = '105%';
  modalRight = 'auto';
  modalTop = 'auto';
  showFullContent = false;
  wishlist: number[] = [];
  cart: number[] = [];

  constructor(
    private courseService: CourseService,
    private imageService: ImageService,
    private router: Router,
    private wishlistService: WishlistService,
    private authService: AuthService,
    private cartService: CartService
  ) { }

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

  navigateToDetail(courseID: number) {
    this.router.navigate(['/courses', courseID]);
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

  // Load wishlist from the server
  loadWishlist(): void {
    this.wishlistService.getWishlist().subscribe({
      next: (wishlist) => {
        this.wishlist = wishlist.map((course: any) => course.courseID);
      },
      error: (err) => console.error('Error fetching wishlist:', err)
    });
  }
  
  toggleWishlist(courseId: number): void {
    if (!this.authService.isAuthenticated()) {
      alert('Please log in first to add to your wishlist.');
      this.router.navigate(['/login']);
      return;
    }
    if (this.wishlist.includes(courseId)) {
      this.wishlistService.removeCourseFromWishlist(courseId).subscribe({
        next: () => {
          this.wishlist = this.wishlist.filter(id => id !== courseId);
          alert('Course removed from wishlist!');
        },
        error: (err) => console.error('Error removing course from wishlist:', err)
      });
    } else {
      this.wishlistService.addCourseToWishlist(courseId).subscribe({
        next: () => {
          this.wishlist.push(courseId);
          alert('Course added to wishlist!');
        },
        error: (err) => console.error('Error adding course to wishlist:', err)
      });
    }
  }

  // Add to cart
  addToCart(courseId: number): void {
    if (!this.authService.isAuthenticated()) {
      alert('Please log in first to add courses to the cart.');
      this.router.navigate(['/login']);
      return;
    }
    this.cartService.addCourseToCart(courseId).subscribe({
      next: () => alert('Course added to cart successfully!'),
      error: (err) => alert(err.message)
    });
  }
}
