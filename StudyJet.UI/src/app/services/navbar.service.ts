import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NavbarService {
  private navbarTypeSubject = new BehaviorSubject<string>('default'); // default, student, admin, instructor
  navbarType$ = this.navbarTypeSubject.asObservable();

  setNavbarType(type: string) {
    this.navbarTypeSubject.next(type);

  }

  constructor() { }
}
