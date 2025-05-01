import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { WishlistItem } from '../models';
import { BehaviorSubject, catchError, map, Observable, of, take, tap } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';
import { ImageService } from './image.service';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  
  private apiUrl = `${environment.apiBaseUrl}/Wishlist`;
  public wishlistSubject = new BehaviorSubject<WishlistItem[]>([]);

  constructor(
    private http: HttpClient,
    private cookieService: CookieService,
    private imageService: ImageService
  ) { }

   // Fetch the wishlist
   getWishlist(): Observable<WishlistItem[]> {
    return this.http.get<WishlistItem[]>(`${this.apiUrl}`).pipe(
      map(wishlist => {
        const updatedWishlist = wishlist.map(item => ({
          ...item,
          imageUrl: this.imageService.getCourseImageUrl(item.imageUrl) 
        }));
        this.wishlistSubject.next(updatedWishlist); 
        return updatedWishlist;
      }),
      catchError(err => {
        console.error('Error fetching wishlist:', err);
        return of([]); 
      })
    );
  }

  // Add a course to wishlist and update the local state
  addCourseToWishlist(courseId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${courseId}`, {}).pipe(
      tap(() => this.getWishlistAndEmit()), 
      catchError((err) => {
        console.error('Error adding course to wishlist:', err);
        return of(null); 
      })
    );
  }

  // Remove a course from wishlist and update the local state
  removeCourseFromWishlist(courseId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${courseId}`).pipe(
      tap(() => this.getWishlistAndEmit()), 
      catchError((err) => {
        console.error('Error removing course from wishlist:', err);
        return of(null); 
      })
    );
  }

  // Helper method to fetch the wishlist and update the subject
  getWishlistAndEmit(): void {
    this.getWishlist().pipe(take(1)).subscribe(updatedWishlist => {
      this.wishlistSubject.next(updatedWishlist);
    });
  }

  moveToCart(courseId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/move-to-cart/${courseId}`, {}).pipe(
      tap(() => {
        this.removeCourseFromWishlist(courseId).subscribe({
          next: () => {
            console.log('Course removed from wishlist and added to cart');
          },
          error: (err) => {
            console.error('Error removing course from wishlist after adding to cart:', err);
          }
        });
      }),
      catchError((err) => {
        console.error('Error moving course to cart:', err);
        return of(null); 
      })
    );
  }

  // Expose the wishlist state to other components
  get wishlist$(): Observable<WishlistItem[]> {
    return this.wishlistSubject.asObservable();
  }
}



