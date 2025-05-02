import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { WishlistItem } from '../models';
import { BehaviorSubject, catchError, map, Observable, of, take, tap, throwError } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';
import { ImageService } from './image.service';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {

  private apiUrl = `${environment.apiBaseUrl}/Wishlist`;
  public wishlistSubject = new BehaviorSubject<WishlistItem[]>([]);

  constructor(
    private http: HttpClient,
    private imageService: ImageService,
    private authService: AuthService
  ) {
    if (this.authService.isAuthenticated()) {
      this.getWishlistAndEmit();
    }
  }

  // Fetch the wishlist
  getWishlist(): Observable<WishlistItem[]> {
    return this.http.get<WishlistItem[]>(`${this.apiUrl}`).pipe(
      map(wishlist => wishlist.map(item => ({
        ...item,
        imageUrl: this.imageService.getCourseImageUrl(item.imageUrl)
      }))),
      catchError(err => {
        console.error('Error fetching wishlist:', err);
        return of([]);
      })
    );
  }

  // Get and emit the wishlist
  getWishlistAndEmit(): void {
    this.getWishlist()
      .pipe(take(1))
      .subscribe(wishlist => this.wishlistSubject.next(wishlist));
  }


  // Add a course to wishlist
  addCourseToWishlist(courseId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${courseId}`, {}).pipe(
      tap(() => this.getWishlistAndEmit()),
      catchError((err) => {
        console.error('Error adding course to wishlist:', err);
        return of();
      })
    );
  }



  // Remove a course from wishlist and update the local state
  removeCourseFromWishlist(courseId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${courseId}`).pipe(
      tap(() => this.getWishlistAndEmit()),
      catchError((err) => {
        console.error('Error removing course from wishlist:', err);
        return of();
      })
    );
  }


  // Move a course to the cart
  moveToCart(courseId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/move-to-cart/${courseId}`, {}).pipe(
      tap(() => this.getWishlistAndEmit()),
      catchError(err => {
        console.error('Error moving course to cart:', err);
        return throwError(() => new Error('Failed to move course to cart.'));
      })
    );
  }

  // Update wishlist manually (external trigger)
  updateWishlistForUser(): void {
    this.getWishlistAndEmit();
  }

  // Observable to Expose 
  get wishlist$(): Observable<WishlistItem[]> {
    return this.wishlistSubject.asObservable();
  }
}



