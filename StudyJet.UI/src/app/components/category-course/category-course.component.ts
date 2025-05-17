import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Category, Course, WishlistItem } from '../../models';
import { AuthService } from '../../services/auth.service';
import { CategoryService } from '../../services/category.service';
import { ImageService } from '../../services/image.service';
import { WishlistService } from '../../services/wishlist.service';
import { Subject, Subscription } from 'rxjs';

@Component({
  selector: 'app-category-course',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './category-course.component.html',
  styleUrl: './category-course.component.css'
})
export class CategoryCourseComponent implements OnInit {
  categoryId: number | null = null;
  categoryName = '';
  courses: Course[] = [];
  wishlist: WishlistItem[] = [];
  isAuthenticated = false;
  private subscriptions = new Subscription(); // To manage multiple subscriptions

  constructor(
    private route: ActivatedRoute,
    private categoryService: CategoryService,
    private imageService: ImageService,
    private authService: AuthService,
    private wishlistService: WishlistService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Subscribe to route param changes to load relevant category and its courses
    this.route.paramMap.subscribe((params) => {
      this.categoryId = Number(params.get('categoryId'));
      if (this.categoryId !== null) {
        this.loadCategoryName(this.categoryId);
        this.loadCourses(this.categoryId);
      }
    });

     // Track user authentication status and fetch wishlist if logged in
    this.subscriptions.add(
      this.authService.isAuthenticated$.subscribe((status) => {
        this.isAuthenticated = status;
        if (status) {
          this.loadWishlist();
        }
      })
    );

     // Keep local wishlist updated in real-time
    this.subscriptions.add(
      this.wishlistService.wishlist$.subscribe((items) => {
        this.wishlist = items;
      })
    );
  }

  ngOnDestroy(): void {
    // Clean up all active subscriptions
    this.subscriptions.unsubscribe();
  }

  // Fetch courses associated with the current category
  loadCourses(categoryId: number): void {
    this.categoryService.getCoursesByCategory(categoryId).subscribe({
      next: (response: any) => {
        const courses = response.courses;
        this.courses = courses.map((course: Course) => course );
      },
      error: (err) => {
        console.error('Error fetching courses:', err);
      },
    });
  }

  // Fetch category name for display based on ID
  loadCategoryName(categoryId: number): void {
    this.categoryService.getCategoryById(categoryId).subscribe({
      next: (cat: Category) => this.categoryName = cat.name,
      error: (err) => console.error('Error fetching category name', err),
    });
  }

  // Fetch user's wishlist (after confirming authentication)
  loadWishlist(): void {
    this.wishlistService.getWishlist().subscribe({
      next: () => { },
      error: (err) => console.error('Error loading wishlist:', err)
    });
  }

   // Generate course image URL from filename
  getCourseImageUrl(imageFilename: string): string {
    return this.imageService.getCourseImageUrl(imageFilename);
  }

    // Check if course is in wishlist
  isInWishlist(courseId: number): boolean {
    return this.wishlist?.some(item => item.courseID === courseId) ?? false;
  }

// Add or remove course from wishlist (based on its current state)
  toggleWishlist(courseId: number, event: MouseEvent): void {
    event.stopPropagation(); // Prevent card click

    if (!this.isAuthenticated) {
      alert('Please log in first to use the wishlist.');
      this.router.navigate(['/login']);
      return;
    }

    if (this.isInWishlist(courseId)) {
      this.wishlistService.removeCourseFromWishlist(courseId).subscribe({
        next: () => alert('Course removed from wishlist.'),
        error: (err) => console.error('Error removing course from wishlist:', err)
      });
    } else {
      this.wishlistService.addCourseToWishlist(courseId).subscribe({
        next: () => alert('Course added to wishlist!'),
        error: (err) => console.error('Error adding course to wishlist:', err)
      });
    }
  }

   // Navigate to the course detail page
  navigateToDetail(courseId: number): void {
    this.router.navigate(['/courses', courseId]);
  }
}



