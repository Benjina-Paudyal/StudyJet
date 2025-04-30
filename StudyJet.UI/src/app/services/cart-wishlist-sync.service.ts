import { Injectable } from '@angular/core';
import { CartService } from './cart.service';
import { WishlistService } from './wishlist.service';
import { catchError, Observable, switchMap, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CartWishlistSyncService {
  
  constructor(
    private cartService: CartService,
    private wishlistService: WishlistService
  ) {}

  moveCourseToWishlist(courseId: number): Observable<void> {
    return this.cartService.removeCourseFromCart(courseId).pipe(
      switchMap(() => this.wishlistService.addCourseToWishlist(courseId)),
      catchError(err => {
        console.error('Error moving course to wishlist:', err);
        return throwError(() => err);

      })
    );
  }

  moveCourseToCart(courseId: number): Observable<void> {
    return this.wishlistService.removeCourseFromWishlist(courseId).pipe(
      switchMap(() => this.cartService.addCourseToCart(courseId)),
      catchError(err => {
        console.error('Error moving course to cart:', err);
        return throwError(() => err);

      })
    );
  }
}


