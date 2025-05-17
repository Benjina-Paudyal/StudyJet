import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Course, WishlistItem } from '../../models';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CourseService } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';
import { PurchaseCourseService } from '../../services/purchase-course.service';
import { WishlistService } from '../../services/wishlist.service';
import { CookieService } from 'ngx-cookie-service';
import { ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';


@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './course-detail.component.html',
  styleUrl: './course-detail.component.css'
})
export class CourseDetailComponent implements OnInit, OnDestroy {
  courseId = 0;
  course: Course | null = null;
  purchasedCourses: Course[] = [];
  videoUrl: SafeResourceUrl | null = null;
  wishlist: number[] = [];
  isAuthenticated = false;
  isAdmin = false;
  isInstructor = false;
  isApproving = false;
  isRejecting = false;
  pendingCourses: Course[] = [];
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private courseService: CourseService,
    private wishlistService: WishlistService,
    private sanitizer: DomSanitizer,
    private authService: AuthService,
    private cartService: CartService,
    private purchaseCourseService: PurchaseCourseService,
    private router: Router,
    private cdr: ChangeDetectorRef

  ) { }

  ngOnInit(): void {
    // Get course ID from route params and load the course details
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const idParam = params.get('id');
        this.courseId = idParam ? +idParam : 0;
        if (this.courseId) {
          this.loadCourseDetails();
        } else {
          console.error('No course ID provided in the route.');
        }
      });

    // Check if the user is authenticated and load additional data
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      this.loadWishlist();
      this.purchaseCourseService.fetchPurchaseCourse();
      this.authService.getRoles().subscribe((roles) => {
        this.isAdmin = roles.includes('Admin');
        this.isInstructor = roles.includes('Instructor');
      });
    }

    // Subscribe to wishlist updates
    this.wishlistService.wishlist$.subscribe((updatedWishlist: WishlistItem[]) => {
      this.wishlist = updatedWishlist.map(item => item.courseID);
      this.cdr.detectChanges();
    });

  }

  ngOnDestroy(): void {
    // Clean up observables to prevent memory leaks
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Get a safe video URL for embedding in the component
  getSafeVideoUrl(url?: string): SafeResourceUrl | null {
    if (url) {
      const videoId = this.extractYouTubeVideoId(url);
      if (videoId) {
        const embedUrl = `https://www.youtube.com/embed/${videoId}`;
        return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
      }
    }
    return null;
  }

  // Extract the YouTube video ID from a given URL
  extractYouTubeVideoId(url: string): string {
    const regExp = /(?:https?:\/\/)?(?:www\.)?youtube\.com\/(?:v\/|embed\/|watch\?v=|watch\?.+&v=|user\/\w+\/\w+\/|playlist\?list=)([a-zA-Z0-9_-]{11})/;
    const match = url.match(regExp);
    return match ? match[1] : '';
  }

  // Add a course to the shopping cart
  addToCart(course: any): void {
    if (!this.isAuthenticated) {
      alert('Please log in first to add courses to the cart.');
      this.router.navigate(['/login']);
      return;
    }
    if (this.isPurchased(course.courseID)) {
      alert('You have already purchased this course.');
      return;
    }
    this.cartService.addCourseToCart(course.courseID).subscribe({
      next: () => {
        alert('Course added to cart successfully!');
        this.wishlistService.getWishlistAndEmit();
      },
      error: (err) => {
        alert(err.message || 'Error adding course to cart!');
      }
    });
  }

  // Load the courses the user has purchased
  loadPurchasedCourses(): void {
    this.purchaseCourseService.purchasedCourses$.subscribe((courses) => {
      this.purchasedCourses = courses;
    });
  }

  // Check if the user has already purchased a specific course
  isPurchased(courseId: number): boolean {
    return this.purchaseCourseService.isCoursePurchased(courseId);
  }

  // Load the user's wishlist
  loadWishlist(): void {
    this.wishlistService.getWishlist().subscribe({
      next: (wishlist) => {
        this.wishlist = wishlist.map((course: any) => course.courseID);
      },
      error: (err) => console.error('Error fetching wishlist:', err)
    });
  }

  // Toggle a course in the wishlist (add/remove)
  toggleWishlist(courseId: number, event: MouseEvent): void {
    if (!this.isAuthenticated) {
      this.router.navigate(['/login']);
      alert('Please login to manage your wishlist.');
      return;
    }

    if (this.isPurchased(courseId)) {
      alert('You have already purchased this course.');
      return;
    }

    if (this.isInWishlist(courseId)) {
      this.wishlistService.removeCourseFromWishlist(courseId).subscribe({
        next: () => {
          this.wishlist = this.wishlist.filter(id => id !== courseId);
          alert('Course removed from wishlist!');
        },
        error: (err) => console.error('Error removing from wishlist:', err)
      });
    } else {
      this.wishlistService.addCourseToWishlist(courseId).subscribe({
        next: () => {
          this.wishlist.push(courseId);
          this.cartService.updateCartForUser();
          this.wishlistService.getWishlistAndEmit();
          alert('Course moved to wishlist and removed from cart!');
        },
        error: (err) => console.error('Error adding to wishlist:', err)
      });
    }
  }


  // Load the course details from the backend
  loadCourseDetails(): void {
    this.courseService.getCourseById(this.courseId).subscribe({
      next: (course: Course) => {
        switch (course.status) {
          case 0:
            course.status = 'Pending';
            break;
          case 1:
            course.status = 'Approved';
            break;
          case 2:
            course.status = 'Rejected';
            break;
        }
        if (course.isUpdate) {
          course.status = 'Pending';
        }
        this.course = course;
        this.videoUrl = this.getSafeVideoUrl(course.videoUrl);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading course:', err);
        alert('Error loading course details. Please try again later.');
      }
    });
  }

  // Approve a course (or its update)
  approveCourse(): void {
    if (!this.course || this.isApproving) return;
    const message = this.course.isUpdate
      ? `Are you sure you want to approve these updates for the course: ${this.course.title}?`
      : `Are you sure you want to approve the course: ${this.course.title}?`;

    if (!window.confirm(message)) return;
    this.isApproving = true;
    const approval$ = this.course.isUpdate
      ? this.courseService.approveCourseUpdate(this.course.courseID)
      : this.courseService.approveCourse(this.course.courseID);

    approval$.subscribe({
      next: () => {
        if (this.course) {
          this.course.status = 'Approved';
          this.course.isUpdate = false;
        }
        this.cdr.detectChanges(); // Force UI update
      },
      error: (err) => console.error('Approval failed:', err),
      complete: () => this.isApproving = false
    });
  }


  // Reject a course (or its update)
  rejectCourse(): void {
    if (!this.course || this.isRejecting) return;
    const message = this.course.isUpdate
      ? `Are you sure you want to reject the updates for the course: ${this.course.title}?`
      : `Are you sure you want to reject the course: ${this.course.title}?`;

    if (!window.confirm(message)) return;
    this.isRejecting = true;
    const rejection$ = this.course.isUpdate
      ? this.courseService.rejectCourseUpdate(this.course.courseID)
      : this.courseService.rejectCourse(this.course.courseID);

    rejection$.subscribe({
      next: () => {
        if (this.course) {
          this.course.status = 'Rejected';
        }
        this.loadCourseDetails();
      },
      error: (err) => console.error('Rejection failed:', err),
      complete: () => this.isRejecting = false
    });
  }

  // Display course status as a string
  getStatusDisplay(status: number | string): string {
    const statusMap: Record<number | string, string> = {
      0: '⏳ Pending',
      1: '✅ Approved',
      2: '❌ Rejected',
      'Pending': '⏳ Pending',
      'Approved': '✅ Approved',
      'Rejected': '❌ Rejected'
    };
    return statusMap[status] || 'Unknown';
  }

  // Check if a course is in the wishlist
  isInWishlist(courseId: number): boolean {
    return this.wishlist.includes(courseId);
  }
}






