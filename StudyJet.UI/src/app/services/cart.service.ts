import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { BehaviorSubject, catchError, map, Observable, of, switchMap, take, tap, throwError } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { WishlistService } from './wishlist.service';
import { CookieService } from 'ngx-cookie-service';
import { CartItem } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiUrl = `${environment.apiBaseUrl}/Cart`;
  private wishlistUrl = `${environment.apiBaseUrl}/Wishlist`;
  public cartSubject = new BehaviorSubject<CartItem[]>([]);

  constructor(
    private http: HttpClient,
    private wishlistService : WishlistService,
    private cookieService : CookieService
  ) { 
    this.updateCartForUser();
  }


  private getHttpOptions(): { headers: HttpHeaders } {
    const token = this.cookieService.get('authToken');
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    } else {
      console.warn('No token found! Requests may fail.');
    }
    return { headers };
  }


  getCartItems(): Observable<CartItem[]> {
    const token = this.cookieService.get('authToken');
    if (!token) {
      return of([]);
    }
    return this.http.get<CartItem[]>(`${this.apiUrl}`, this.getHttpOptions()).pipe(
      map(cart => cart.map(item => ({
        ...item,
        imageUrl: `${environment.imageBaseUrl}${item.imageUrl.replace(/^\/+/, '')}`
      }))),
      tap(cart => this.cartSubject.next(cart)),
      catchError(err => {
        console.error('Error fetching cart items:', err);
        return of([]);
      })
    );
  }


  isCourseInCart(courseId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/is-in-cart/${courseId}`, this.getHttpOptions()).pipe(
      catchError(err => {
        console.error('Error checking if course is in cart:', err);
        return of(false);
      })
    );
  }


  isCourseInWishlist(courseId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.wishlistUrl}/is-in-wishlist/${courseId}`, this.getHttpOptions()).pipe(
      catchError(err => {
        console.error('Error checking if course is in wishlist:', err);
        return of(false);
      })
    );
  }


  addCourseToCart(courseId: number): Observable<void> {
    const token = this.cookieService.get('authToken');
    if (!token) {
      console.warn('Please log in first.');
      return throwError(() => new Error('Please log in first.'));
    }


    return this.isCourseInCart(courseId).pipe(
      switchMap(isInCart => {
        if (isInCart) {
          console.warn('Course is already in the cart.');
          return throwError(() => new Error('Course is already in the cart.'));
        }
        return this.http.post<void>(`${this.apiUrl}/${courseId}/add`, {}, this.getHttpOptions()).pipe(
          tap(() => this.updateCartForUser())
        );
      }),
      catchError(err => {
        console.error('Error adding course to cart:', err);
        return throwError(() => new Error('Failed to add course to cart.'));
      })
    );
  }


  removeCourseFromCart(courseId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${courseId}/remove`, {
      ...this.getHttpOptions(),
      observe: 'body'
    }).pipe(
      tap(() => this.getCartAndEmit()),
      catchError(err => {
        console.error('Error removing course from cart:', err);
        return throwError(() => new Error('Failed to remove course from cart.'));
      })
    );
  }


  moveToWishlist(courseId: number): Observable<void> {
    return this.isCourseInWishlist(courseId).pipe(
      switchMap(isInWishlist => isInWishlist ? of(void 0) :
        this.http.post<void>(`${this.apiUrl}/move-to-wishlist/${courseId}`, {}, this.getHttpOptions())
      ),
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


  updateCartForUser(): void {
    this.getCartItems().pipe(take(1)).subscribe(cart => this.cartSubject.next(cart));
  }



  getCourseDetails(courseId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/course/${courseId}`, this.getHttpOptions()).pipe(
      catchError(err => {
        console.error('Error fetching course details:', err);
        return of(null);
      })
    );
  }


  private getCartAndEmit(): void {
    this.getCartItems().pipe(take(1)).subscribe(cart => this.cartSubject.next(cart));
  }
  get cart$(): Observable<any[]> {
    return this.cartSubject.asObservable();
  }


  createCheckoutSession(courseIds: number[]): Observable<{ url: string }> {
    const token = this.cookieService.get('authToken');
    if (!token) {
      console.error("No auth token found.");
      return throwError(() => new Error('No auth token found.'));
    }

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    return this.http.post<{ url: string }>(
      `${environment.apiBaseUrl}/user/purchases/create-checkout-session`,
      { courseIds },
      { headers }
    );
  }
}


