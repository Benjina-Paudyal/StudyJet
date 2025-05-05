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
  private subscriptions = new Subscription();
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private categoryService: CategoryService,
    private imageService: ImageService,
    private authService: AuthService,
    private wishlistService: WishlistService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      this.categoryId = Number(params.get('categoryId'));
      if (this.categoryId !== null) {
        this.loadCategoryName(this.categoryId);
        this.loadCourses(this.categoryId);
      }
    });

    // Track auth status
    this.subscriptions.add(
      this.authService.isAuthenticated$.subscribe((status) => {
        this.isAuthenticated = status;
        if (status) {
          this.loadWishlist();
        }
      })
    );

    // Track wishlist changes
    this.subscriptions.add(
      this.wishlistService.wishlist$.subscribe((items) => {
        this.wishlist = items;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadCourses(categoryId: number): void {
    console.log('Calling loadCourses for categoryID:', categoryId);
    this.categoryService.getCoursesByCategory(categoryId).subscribe({
      next: (response: any) => {
        const courses = response.courses;
        this.courses = courses.map((course: Course) => {
          return course;
        });
      },
      error: (err) => {
        console.error('Error fetching courses:', err);
      },
    });
  }


  loadCategoryName(categoryId: number): void {
    this.categoryService.getCategoryById(categoryId).subscribe({
      next: (cat: Category) => this.categoryName = cat.name,
      error: (err) => console.error('Error fetching category name', err),
    });
  }

  loadWishlist(): void {
    this.wishlistService.getWishlist().subscribe({
      next: () => { },
      error: (err) => console.error('Error loading wishlist:', err)
    });
  }

  getCourseImageUrl(imageFilename: string): string {
    return this.imageService.getCourseImageUrl(imageFilename);
  }

  isInWishlist(courseId: number): boolean {
    return this.wishlist.some(item => item.courseID === courseId);
  }


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

  navigateToDetail(courseId: number): void {
    this.router.navigate(['/courses', courseId]);
  }
}



