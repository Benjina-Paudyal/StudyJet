import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NavbarService {
  private navbarTypeSubject = new BehaviorSubject<'admin' | 'instructor' | 'student' | 'default'| 'hidden'>('default');
  navbarType$ = this.navbarTypeSubject.asObservable();

  setNavbarType(type: 'admin' | 'instructor' | 'student' | 'default') {
    this.navbarTypeSubject.next(type);

  }

  constructor() { }
}
