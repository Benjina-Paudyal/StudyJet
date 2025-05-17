import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { Subject, takeUntil } from 'rxjs';
import { WishlistItem, Course } from '../../models';
import { AuthService } from '../../services/auth.service';
import { NavbarService } from '../../services/navbar.service';
import { PurchaseCourseService } from '../../services/purchase-course.service';
import { WishlistService } from '../../services/wishlist.service';
import { CommonModule } from '@angular/common';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './wishlist.component.html',
  styleUrl: './wishlist.component.css'
})
export class WishlistComponent implements OnInit, OnDestroy {

  wishlist: WishlistItem[] = [];
  purchasedCourses: Course[] = [];
  errorMessage = '';
  isLoading = false;

  private destroy$ = new Subject<void>();

  constructor(
    private wishlistService: WishlistService,
    private navbarService: NavbarService,
    private authService: AuthService,
    private imageService: ImageService,
    private purchaseCourseService: PurchaseCourseService,
    private cookieService: CookieService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Redirect to login if user is not authenticated
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    // Set navbar type based on localStorage value
    const navbarType = localStorage.getItem('navbarType');
    const validNavbarTypes = ['admin', 'instructor', 'student', 'default', 'hidden'] as const;
    if (navbarType && validNavbarTypes.includes(navbarType as typeof validNavbarTypes[number])) {
      this.navbarService.setNavbarType(navbarType as typeof validNavbarTypes[number]);
    }
    
    // Subscribe to wishlist updates and update component state
    this.wishlistService.wishlist$
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (updatedWishlist) => {
        this.wishlist = updatedWishlist.map(item => ({
          ...item,
          imageUrl: this.imageService.getCourseImageUrl(item.imageUrl)
        }));
        this.errorMessage = '';
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.errorMessage = 'Failed to load wishlist. Please try again later.';
        console.error('Error loading wishlist:', error);
        this.isLoading = false;
      }
    });
      // Subscribe to purchased courses and update state
       this.purchaseCourseService.purchasedCourses$
       .pipe(takeUntil(this.destroy$))
       .subscribe((courses) => {
         this.purchasedCourses = courses;
       });
       
     this.loadWishlist();
   }

   
   ngOnDestroy(): void {
    // Clean up subscriptions to prevent memory leaks
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Method to load the wishlist data
  loadWishlist(): void {
    this.isLoading = true;
    this.wishlistService.getWishlist().subscribe(); 
  }

  // Check if a course is already purchased
  isPurchased(courseId: number): boolean {
    return this.purchasedCourses.some(course => course.courseID === courseId);
  }

  // Check if a course is already in the wishlist
  isInWishlist(courseId: number): boolean {
    return this.wishlist.some(item => item.courseID === courseId);
  }

  // Toggle course between wishlist and cart
  toggleWishlist(course: WishlistItem): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    // Add or remove course based on its current state in the wishlist
    if (this.isInWishlist(course.courseID)) {
      this.removeFromWishlist(course.courseID);
    } else {
      this.addToWishlist(course.courseID);
    }
  }

  // Add course to wishlist
  addToWishlist(courseId: number): void {
    if (this.isPurchased(courseId)) {
      alert("You already own this course. No need to add it to the wishlist!");
      return;
    }

    this.isLoading = true;
    // Call service to add course to wishlist
    this.wishlistService.addCourseToWishlist(courseId).subscribe({
      next: () => (this.isLoading = false),
      error: (error) => {
        console.error('Failed to add course to wishlist.', error);
        this.errorMessage = 'Could not add course to wishlist. Please try again.';
        this.isLoading = false;
      }
    });
  }

  // Remove course from wishlist
  removeFromWishlist(courseId: number): void {
    this.isLoading = true;
    this.wishlistService.removeCourseFromWishlist(courseId).subscribe({
      next: () => (this.isLoading = false),
      error: (error) => {
        console.error('Failed to remove course from wishlist.', error);
        this.errorMessage = 'Could not remove course from wishlist. Please try again.';
        this.isLoading = false;
      }
    });
  }

  // Move course from wishlist to cart
  addToCart(course: WishlistItem): void {
    this.wishlistService.moveToCart(course.courseID).subscribe({
      next: () => {
        console.log(`${course.courseID} has been moved to the cart.`);
      },
      error: (error) => {
        console.error('Error moving course to cart:', error);
        this.errorMessage = 'Failed to add course to cart. Please try again later.';
      }
    });
  }

  // Track courses by their unique ID for efficient rendering
  trackByCourseId(index: number, item: WishlistItem): number {
    return item.courseID;
  }
}