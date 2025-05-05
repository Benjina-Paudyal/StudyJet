import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { BehaviorSubject, catchError, map, Observable, of, switchMap, take, tap, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { WishlistService } from './wishlist.service';
import { CartItem } from '../models';
import { ImageService } from './image.service';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartUrl = `${environment.apiBaseUrl}/Cart`;
  private wishlistUrl = `${environment.apiBaseUrl}/Wishlist`;
  public cartSubject = new BehaviorSubject<CartItem[]>([]);

  constructor(
    private http: HttpClient,
    private wishlistService : WishlistService,
    private imageService: ImageService,
  ) { 
    this.getCartAndEmit();
  }

// Get the cart items 
  getCartItems(): Observable<CartItem[]> {
    return this.http.get<CartItem[]>(`${this.cartUrl}`).pipe(
      map(cart => cart.map(item => ({
        ...item,
        imageUrl: this.imageService.getCourseImageUrl(item.imageUrl)
      }))),
      catchError(err => {
        console.error('Error fetching cart items:', err);
        return of([]);
      })
    );
  }


// Check if the course is already in the cart
  isCourseInCart(courseId: number): Observable<boolean> {
    console.log('Checking if course is in cart:', courseId);
    return this.http.get<boolean>(`${this.cartUrl}/is-in-cart/${courseId}`).pipe(
      catchError(err => {
        console.error('Error checking if course is in cart:', err);
        return of(false);
      })
    );
  }

  
// Check if the course is already in the wishlist
isCourseInWishlist(courseId: number): Observable<boolean> {
  return this.http.get<any>(`${this.wishlistUrl}/is-in-wishlist/${courseId}`).pipe(
    map(response => response.ObjectisInWishlist), 
    catchError(err => {
      console.error('Error checking if course is in wishlist:', err);
      return of(false); 
    })
  );
}



  // Add a course to the cart if it isn't already present
  addCourseToCart(courseId: number): Observable<void> {
    return this.isCourseInCart(courseId).pipe(
      switchMap(isInCart => {
        if (isInCart) {
          console.warn('Course is already in the cart.');
          return of(undefined);
        }
        return this.http.post<void>(`${this.cartUrl}/${courseId}/add`, {}).pipe(
          tap(() => this.getCartAndEmit()) 
        );
      }),
      catchError(err => {
        console.error('Error adding course to cart:', err);
        return throwError(() => new Error('Failed to add course to cart.'));
      })
    );
  }

  
  //fetch the cart items and update the cartSubject observable
  private getCartAndEmit(): void {
    this.getCartItems()
    .pipe(take(1))
    .subscribe(cart => {
      this.cartSubject.next(cart)
  });
  }



  // Remove a course from the cart and update the cart state
  removeCourseFromCart(courseId: number): Observable<void> {
    return this.http.delete<void>(`${this.cartUrl}/${courseId}/remove`).pipe(
      tap(() => this.getCartAndEmit()),
      catchError(err => {
        console.error('Error removing course from cart:', err);
        return throwError(() => new Error('Failed to remove course from cart.'));
      })
    );
  }


// Move a course from the cart to the wishlist
moveToWishlist(courseId: number): Observable<void> {
  return this.isCourseInWishlist(courseId).pipe(
    switchMap(isInWishlist => {
      console.log('Is course in wishlist:', isInWishlist); // Log the result
      if (isInWishlist) {
        return of(void 0);
      } else {
        console.log('Sending POST request to move course to wishlist');
        return this.http.post<void>(`${this.cartUrl}/move-to-wishlist/${courseId}`, {});
      }
    }),
    tap(() => {
      this.getCartAndEmit();
      this.wishlistService.getWishlistAndEmit(); 
    }),
    catchError(err => {
      console.error('Error moving course to wishlist:', err);
      return throwError(() => new Error('Failed to move course to wishlist.'));
    })
  );
}


// Fetch and update the cart for the user
  updateCartForUser(): void {
    this.getCartItems().pipe(take(1)).subscribe(cart => this.cartSubject.next(cart));
  }


// Get the course details
  getCourseDetails(courseId: number): Observable<any> {
    return this.http.get(`${this.cartUrl}/course/${courseId}`).pipe(
      catchError(err => {
        console.error('Error fetching course details:', err);
        return of(null);
      })
    );
  }


// Get the observable for the cart items
  get cart$(): Observable<any[]> {
    return this.cartSubject.asObservable(); // Expose the cartSubject as an observable for subscribers
  }


 // Create a checkout session for the selected courses
createCheckoutSession(courseIds: number[]): Observable<{ url: string }> {
  return this.http.post<{ url: string }>(
    `${environment.apiBaseUrl}/user/purchases/create-checkout-session`,
    { courseIds }
  );
}

}


