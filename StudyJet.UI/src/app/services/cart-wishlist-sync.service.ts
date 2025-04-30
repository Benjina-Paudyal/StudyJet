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


  private moveCourse(fromService: Observable<void>, toService: Observable<void>): Observable<void> {
    return fromService.pipe(
      switchMap(() => toService),
      catchError(err => {
        console.error('Error:', err);
        return throwError(() => err);
      })
    );
  }

  moveCourseToWishlist(courseId: number): Observable<void> {
    return this.moveCourse(
      this.cartService.removeCourseFromCart(courseId),
      this.wishlistService.addCourseToWishlist(courseId)
    );
  }
  
  moveCourseToCart(courseId: number): Observable<void> {
    return this.moveCourse(
      this.wishlistService.removeCourseFromWishlist(courseId),
      this.cartService.addCourseToCart(courseId)
    );
  }
}

