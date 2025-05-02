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
    private purchaseCourseService: PurchaseCourseService,
    private cookieService: CookieService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    const navbarType = this.cookieService.get('navbarType');
    const validNavbarTypes = ['admin', 'instructor', 'student', 'default', 'hidden'] as const;
    if (validNavbarTypes.includes(navbarType as typeof validNavbarTypes[number])) {
      this.navbarService.setNavbarType(navbarType as typeof validNavbarTypes[number]);
    }

    
    // Subscribe to wishlist updates
    this.wishlistService.wishlist$
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (updatedWishlist) => {
        this.wishlist = updatedWishlist;
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

       // Purchased courses
       this.purchaseCourseService.purchasedCourses$
       .pipe(takeUntil(this.destroy$))
       .subscribe((courses) => {
         this.purchasedCourses = courses;
       });
 
     // Initial load
     this.loadWishlist();
   }

   ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadWishlist(): void {
    this.isLoading = true;
    this.wishlistService.getWishlist().subscribe(); 
  }

  isPurchased(courseId: number): boolean {
    return this.purchasedCourses.some(course => course.courseID === courseId);
  }

  isInWishlist(courseId: number): boolean {
    return this.wishlist.some(item => item.courseID === courseId);
  }

  toggleWishlist(course: WishlistItem): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.isInWishlist(course.courseID)) {
      this.removeFromWishlist(course.courseID);
    } else {
      this.addToWishlist(course.courseID);
    }
  }

  addToWishlist(courseId: number): void {
    if (this.isPurchased(courseId)) {
      alert("You already own this course. No need to add it to the wishlist!");
      return;
    }

    this.isLoading = true;
    this.wishlistService.addCourseToWishlist(courseId).subscribe({
      next: () => (this.isLoading = false),
      error: (error) => {
        console.error('Failed to add course to wishlist.', error);
        this.errorMessage = 'Could not add course to wishlist. Please try again.';
        this.isLoading = false;
      }
    });
  }

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

  trackByCourseId(index: number, item: WishlistItem): number {
    return item.courseID;
  }
}























