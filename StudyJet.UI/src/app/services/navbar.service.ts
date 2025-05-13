import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

// Define possible values for navbar roles
export type NavbarRole = 'admin' | 'instructor' | 'student' | 'default' | 'hidden';

@Injectable({
  providedIn: 'root'
})
export class NavbarService {
  // Create a BehaviorSubject to store the current navbar type 
  private navbarTypeSubject = new BehaviorSubject<'default' | 'student' | 'admin' | 'hidden'|'instructor'>('default'); 

  // Expose the navbar type as an observable,
  navbarType$ = this.navbarTypeSubject.asObservable();

  setNavbarType(type: 'default' | 'student' | 'admin' | 'instructor' | 'hidden') {
    this.navbarTypeSubject.next(type);
    localStorage.setItem('navbarType', type);
  }

  constructor() {
    const savedNavbarType = localStorage.getItem('navbarType');
    if (savedNavbarType) {
      if (['default', 'student', 'admin', 'instructor'].includes(savedNavbarType)) {
        this.navbarTypeSubject.next(savedNavbarType as 'default' | 'student' | 'admin' |'hidden'|'instructor');
      }
    }
  }
}
