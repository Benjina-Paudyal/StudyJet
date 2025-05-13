
import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { filter, map, Observable, Subscription } from 'rxjs';
import { ChangeDetectorRef } from '@angular/core';
import { CartItem, Category, Course, WishlistItem } from '../../models';
import { CategoryService } from '../../services/category.service';
import { CourseService } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { NavbarService } from '../../services/navbar.service';
import { PurchaseCourseService } from '../../services/purchase-course.service';
import { WishlistService } from '../../services/wishlist.service';
import { CartService } from '../../services/cart.service';
import { InactivityService } from '../../services/inactivity.service';
import { ImageService } from '../../services/image.service';
import { CookieService } from 'ngx-cookie-service';
import { NotificationService } from '../../services/notification.service';


@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {
  categories: Category[] = [];
  wishlist: WishlistItem[] = [];
  cartItems: CartItem[] = [];
  suggestions: Course[] = [];
  purchasedCourses: Course[] = [];
  isNavbarCollapsed = true;
  isDropdownOpen = false;
  showWishlistDropdown = false;
  showCartDropdown = false;
  dropdownVisible = false;
  searchPlaceholder = 'What do you want to learn?';
  searchQuery = '';
  errorMessage = '';
  loggingOutText = 'Logging out, please wait...';
  dotCount = 0;
  cartItemCount = 0;
  unreadNotificationsCount = 0;
  isLoading = false;
  cartCount = 0;
  navbarType: 'admin' | 'instructor' | 'student' | 'default' | 'hidden' = 'default';
  navbarType$: Observable<'admin' | 'instructor' | 'student' | 'default' | 'hidden'>;
  subscriptions: Subscription[] = [];
  profileImageUrl: string | null = null;
  isLoggingIn = true;
  isAuthenticated = false;

  constructor(
    private categoryService: CategoryService,
    private router: Router,
    private cookieService: CookieService,
    private courseService: CourseService,
    private purchaseCourseService: PurchaseCourseService,
    private navbarService: NavbarService,
    private userService: UserService,
    private wishlistService: WishlistService,
    private cartService: CartService,
    private authService: AuthService,
    private imageService: ImageService,
    private inactivityService: InactivityService,
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef,
  ) {
    this.navbarType$ = this.navbarService.navbarType$;
  }

  ngOnInit() {
    this.inactivityService.startMonitoring();
    this.navbarType = 'hidden';
    this.loadCategories();

    // Sync navbar type
    this.subscriptions.push(
      this.navbarService.navbarType$.subscribe((type) => {
        this.navbarType = type;
        this.cdr.detectChanges();
      })
    );

    // Sync cart items
    this.subscriptions.push(
      this.cartService.cart$.subscribe(cart => {
        this.cartItems = cart;
        this.cartItemCount = cart.length;
        this.cdr.detectChanges();
      })
    );

    // Sync wishlist
    this.subscriptions.push(
      this.wishlistService.wishlist$.subscribe((wishlist) => {
        this.wishlist = wishlist;
        this.cdr.detectChanges();
      })
    );

    // Sync profile image
    this.subscriptions.push(
      this.authService.profileImage$
        .pipe(
          filter((url): url is string => !!url),
          map(url => this.imageService.getProfileImageUrl(url)),
          map(fullUrl => `${fullUrl}?t=${Date.now()}`)
        )
        .subscribe(url => {
          this.profileImageUrl = url;
          this.cdr.detectChanges();
        })
    );

    // Handle user authentication and related data loading
    this.subscriptions.push(
      this.authService.isAuthenticated$.subscribe((isAuthenticated) => {
        this.isAuthenticated = isAuthenticated;
        if (isAuthenticated) {
          this.notificationService.getNotifications().subscribe(notifications => {
            this.notificationService.updateUnreadNotificationsCount(notifications);
          });

          const unreadSub = this.notificationService.unreadCount$.subscribe(count => {
            this.unreadNotificationsCount = count;
            this.cdr.detectChanges();
          });
          this.subscriptions.push(unreadSub);

          // Load user-related data
          this.cartService.updateCartForUser();
          this.loadWishlist();
          this.loadPurchasedCourses();
          this.updatePlaceholderText(window.innerWidth);
        }
      })
    );
  }

  ngOnDestroy() {
    this.inactivityService.stopMonitoring();
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event) {
    this.updatePlaceholderText((event.target as Window).innerWidth);
  }
  
  // UI Toggles
  toggleNavbar() {
    this.isNavbarCollapsed = !this.isNavbarCollapsed;
    console.log('isNavbarCollapsed:', this.isNavbarCollapsed);
    this.updatePlaceholderText(window.innerWidth);
  }

  toggleWishlistDropdown(): void {
    this.showWishlistDropdown = !this.showWishlistDropdown;
  }

  toggleCartDropdown(event: MouseEvent) {
    event.preventDefault();
    this.showCartDropdown = !this.showCartDropdown;
  }

  getTotalPrice(): number {
    return this.cartItems.reduce((total, item) => total + item.price, 0);
  }

  trackByCourseId(index: number, item: any): number {
    return item.courseId;
  }

  private updatePlaceholderText(width: number) {
    const searchInput = document.getElementById('search-bar') as HTMLInputElement;
    if (width > 1024) {
      this.searchPlaceholder = 'What do you want to learn?';
    } else if (width > 991) {
      this.searchPlaceholder = 'Course?';
    } else if (width > 400) {
      this.searchPlaceholder = 'What do you want to learn?';
    } else if (width > 300) {
      this.searchPlaceholder = 'Course?';
    } else {
      this.searchPlaceholder = '?';
    }
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data: Category[]) => {
        this.categories = data;
        console.log('Categories loaded:', this.categories);
      },
      error: (err) => {
        console.error('Error fetching categories', err);
      },
    });
  }

  onCategorySelected(category: Category): void {
    console.log('Selected category:', category);
  }

  goToCourse(courseID: number) {
    this.router.navigate(['/courses', courseID]);
  }

  logout(): void {
    this.authService.logout().then(() => {
      this.cookieService.delete('profilePictureUrl');
      this.cdr.detectChanges();
      this.clearState();
      this.router.navigate(['/home']);
    }).catch(error => {
      console.error('Logout failed:', error);
    });
  }

  private clearState() {
    this.userService.clearUser();
    this.wishlist = [];
    this.profileImageUrl = null;
    this.isAuthenticated = false;
    this.showWishlistDropdown = false;
  }

  navigateToStudentDashboard(): void {
    this.router.navigate(['/student-dashboard']);
  }

  navigateToInstructorDashboard(): void {
    this.router.navigate(['/instructor-dashboard']);
  }

  navigateToAdminDashboard(): void {
    this.router.navigate(['/admin-dashboard']);
  }

  // Add course to cart
  addToCart(item: WishlistItem): void {
    this.cartService.addCourseToCart(item.courseID).subscribe({
      next: () => {
        this.wishlist = this.wishlist.filter(w => w.courseID !== item.courseID);
        window.alert(`"${item.title}" has been moved to your cart.`);
      },
      error: (error) => {
        this.errorMessage = 'Failed to add course to cart.';
        console.error('Error adding course to cart:', error);
      }
    });
  }

  openDropdown() {
    this.isDropdownOpen = true;
  }

  closeDropdown() {
    this.isDropdownOpen = false;
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  private loadWishlist(): void {
    this.wishlistService.getWishlist().subscribe({
      next: (data: WishlistItem[]) => {
        this.wishlist = data;
      },
      error: (error) => {
        console.error('Error loading wishlist:', error);
      },
    });
  }

  private loadCartItems(): void {
    this.isLoading = true;
    this.cartService.getCartItems().subscribe({
      next: (cartItems: CartItem[]) => {
        this.cartItems = cartItems;
        this.cartItemCount = cartItems.length;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading cart items:', error);
        this.cartItemCount = 0;
        this.isLoading = false;
      },
    });
  }

  loadPurchasedCourses(): void {
    this.purchaseCourseService.getPurchaseCourse().subscribe({
      next: (courses: Course[]) => {
        this.purchasedCourses = courses.map(course => ({
          ...course,
          imageUrl: this.imageService.getCourseImageUrl(course.imageUrl || 'default-image.jpg')
        }));

        console.log('Purchased Courses:', this.purchasedCourses);
      },
      error: (err) => {
        console.error('Error fetching purchased courses:', err);
      }
    });
  }

  getCourseImageUrl(course: any): string {
    return this.imageService.getCourseImageUrl(course.imageUrl);
  }

  onSearchInput(): void {
    if (this.searchQuery.length > 2) {
      this.courseService.searchCourses(this.searchQuery).subscribe({
        next: (courses: Course[]) => {
          this.suggestions = courses;
          this.dropdownVisible = this.suggestions.length > 0;
        },
        error: (err) => {
          console.error('Error fetching course suggestions', err);
          this.suggestions = [];
          this.dropdownVisible = false;
        },
      });
    } else {
      this.suggestions = [];
      this.dropdownVisible = false;
    }
  }

  onSearchSubmit() {
    if (this.searchQuery.length > 0) {
      this.suggestions = [];
      this.dropdownVisible = false;
      this.router.navigate(['/search-results'], {
        queryParams: { query: this.searchQuery },
      });
    }
  }

  clearSearch() {
    this.searchQuery = '';
    this.suggestions = [];
    this.dropdownVisible = false;
  }

  isOnCourseDetailPage(): boolean {
    return this.router.url.includes('/courses/');
  }


}

