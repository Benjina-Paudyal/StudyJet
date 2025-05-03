import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { decodeToken } from '../models';

export type NavbarRole = 'admin' | 'instructor' | 'student' | 'default' | 'hidden';

@Injectable({
  providedIn: 'root'
})
export class NavbarService {
  private navbarTypeSubject = new BehaviorSubject<'default' | 'student' | 'admin' | 'hidden'|'instructor'>('default'); // default, student, admin, instructor
  navbarType$ = this.navbarTypeSubject.asObservable();

  setNavbarType(type: 'default' | 'student' | 'admin' | 'instructor' | 'hidden') {
    this.navbarTypeSubject.next(type);
    localStorage.setItem('navbarType', type);
  }
  constructor() {
    const savedNavbarType = localStorage.getItem('navbarType');
    if (savedNavbarType) {
      // Check if savedNavbarType is valid before setting it
      if (['default', 'student', 'admin', 'instructor'].includes(savedNavbarType)) {
        this.navbarTypeSubject.next(savedNavbarType as 'default' | 'student' | 'admin' |'hidden'|'instructor');
      }
    }
  }
}
