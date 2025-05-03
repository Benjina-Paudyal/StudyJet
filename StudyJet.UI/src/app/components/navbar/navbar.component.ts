import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CartItem, Category, Course, WishlistItem } from '../../models';
import { CategoryService } from '../../services/category.service';
import { CourseService } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { FormsModule } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';
import { NavbarService } from '../../services/navbar.service';
import { PurchaseCourseService } from '../../services/purchase-course.service';
import { WishlistService } from '../../services/wishlist.service';
import { CartService } from '../../services/cart.service';
import { InactivityService } from '../../services/inactivity.service';
import { ImageService } from '../../services/image.service';
import { CookieService } from 'ngx-cookie-service';
import { ChangeDetectorRef } from '@angular/core';


@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {
   categories: Category[] = [];
  isNavbarCollapsed = true;
  isDropdownOpen = false;
  searchPlaceholder = 'What do you want to learn?';
  searchQuery = '';
  suggestions: Course[] = [];
  wishlist: WishlistItem[] = [];
  showWishlistDropdown = false;
  cartItems: CartItem[] = [];
  showCartDropdown = false;
  cartItemCount = 0;
  isLoading = false;
  profileImageUrl: string | null = null;
  isLoggingIn = true;
  loggingOutText = 'Logging out, please wait...';
  dotCount = 0;
  purchasedCourses: Course[] = [];
  isAuthenticated = false;
  subscriptions: Subscription[] = [];
  unreadNotificationsCount = 0;
  cartCount: number = 0;
  // Default value for navbarType
  navbarType: 'admin' | 'instructor' | 'student' | 'default' | 'hidden' = 'default'; 

  // Initialize navbarType$ inside the constructor
  navbarType$: Observable<'admin' | 'instructor' | 'student' | 'default' | 'hidden'>;
  
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
    private cdr: ChangeDetectorRef,
  ) { 
    this.navbarType$ = this.navbarService.navbarType$;
  }

  ngOnInit() {
    this.inactivityService.startMonitoring();
    this.navbarType = 'hidden';
    this.loadCategories();

  // Subscribe to navbar type changes
  this.subscriptions.push(
    this.navbarService.navbarType$.subscribe((type) => {
      this.navbarType = type;
       this.cdr.detectChanges();
    })
  );

    // Subscribe to cart updates
    this.subscriptions.push(
      this.cartService.cart$.subscribe(cart => {
        this.cartItems = cart;
        this.cartItemCount = cart.length;
      })
    );

    // Subscribe to wishlist updates
    this.subscriptions.push(
      this.wishlistService.wishlist$.subscribe((wishlist) => {
        this.wishlist = wishlist;
      })
    );

    this.subscriptions.push(
      this.authService.isAuthenticated$.subscribe((isAuthenticated) => {
        this.isAuthenticated = isAuthenticated;
        if (isAuthenticated) {
          const rawProfileImage = this.authService.getProfileImage();
          this.profileImageUrl = `${this.imageService.getProfileImageUrl(rawProfileImage)}?t=${new Date().getTime()}`;
          //this.profileImageUrl = this.imageService.getProfileImageUrl(rawProfileImage);
          this.cdr.detectChanges();

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

  toggleNavbar() {
    this.isNavbarCollapsed = !this.isNavbarCollapsed;
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

  onSearchInput(): void {
    if (this.searchQuery.length > 2) {
      this.courseService.searchCourses(this.searchQuery).subscribe({
        next: (courses: Course[]) => {
          this.suggestions = courses;
        },
        error: (err) => {
          console.error('Error fetching course suggestions', err);
          this.suggestions = [];
        },
      });
    } else {
      this.suggestions = [];
    }
  }

  goToCourse(courseID: number) {
    this.router.navigate(['/courses', courseID]);
  }

  onSearchSubmit() {
    if (this.searchQuery.length > 0) {
      this.suggestions = [];
      this.router.navigate(['/search-results'], {
        queryParams: { query: this.searchQuery },
      });
    }
  }

  logout(): void {
    this.authService.logout().then(() => {
      // Clear the profile image and other states
      this.clearState();
      this.clearProfileImage();

      // Redirect to home page
      this.router.navigate(['/home']);
    }).catch(error => {
      console.error('Logout failed:', error);
    });
  }

  // Clear profile image and reset UI
  clearProfileImage(): void {
    // Delete the profile image cookie
    this.cookieService.delete('profilePictureUrl');

    // Reset profile image to default
    this.profileImageUrl = '/images/profiles/default.png';

    // Manually trigger change detection to update the UI
    this.cdr.detectChanges();
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

  addToCart(course: any): void {
    console.log('Added to the cart:', course);
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
}



