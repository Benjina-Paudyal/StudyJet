import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CartItem } from '../../models';
import { CookieService } from 'ngx-cookie-service';
import { AuthService } from '../../services/auth.service';
import { CartService } from '../../services/cart.service';
import { NavbarService } from '../../services/navbar.service';
import { Subject, takeUntil } from 'rxjs';


@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent implements OnInit, OnDestroy {
  cart: CartItem[] = [];
  wishlist = [];
  errorMessage = '';
  isLoading = false;

  private destroy$ = new Subject<void>(); // to clean up subscription

  constructor(
    private cartService: CartService,
    private navbarService: NavbarService,
    private authService: AuthService,
    private cookieService: CookieService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

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
    this.cartService.cart$
      .pipe(takeUntil(this.destroy$))
      .subscribe(cartItems => {
        this.cart = cartItems;
        this.cdr.detectChanges();
      });
    this.cartService.updateCartForUser(); // intial fetch and update
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  removeFromCart(courseID: number, title: string): void {
    this.isLoading = true;
    this.cartService.removeCourseFromCart(courseID).subscribe({
      next: () => {
        window.alert(`"${title}" has been removed from your cart.`);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to remove course from cart.', error);
        this.errorMessage = 'Could not remove course from cart. Please try again.';
        this.isLoading = false;
      }
    });
  }

  moveToWishlist(courseID: number, title: string): void {
    this.isLoading = true;
    this.cartService.moveToWishlist(courseID).subscribe({
      next: () => {
        this.cart = this.cart.filter(item => item.courseID !== courseID);
        window.alert(`"${title}" has been moved to your wishlist.`);

        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to move course to wishlist.', error);
        this.errorMessage = 'Could not move course to wishlist. Please try again.';
        this.isLoading = false;
      }
    });
  }

  checkout(): void {
    if (this.cart.length === 0) {
      this.errorMessage = "Your cart is empty!";
      return;
    }
    this.isLoading = true;
    const courseIds = this.cart.map(item => item.courseID);
    
    this.cartService.createCheckoutSession(courseIds).subscribe({
      next: (response) => {
        if (response.url) {
          window.location.href = response.url; // Redirect to Stripe checkout
        } else {
          this.errorMessage = "Failed to initiate checkout.";
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error("Error during checkout:", err);
        this.errorMessage = "Checkout failed. Please try again.";
        this.isLoading = false;
      }
    });

  }

  getTotalPrice(): number {
    return this.cart.reduce((total, item) => total + item.price, 0);
  }

  trackByCartId(index: number, item: any): number {
    return item.courseID;
  }
}
