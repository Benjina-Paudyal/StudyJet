import { Component, OnInit } from '@angular/core';
import { NavbarComponent } from './components/navbar/navbar.component';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from './components/footer/footer.component';
import { NavbarService } from './services/navbar.service';
import { DecodedToken, decodeToken } from './models';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavbarComponent, RouterOutlet, FooterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit{
  title = 'StudyJet.UI';
  navbarType= 'default'; 

  constructor(
    private navbarService: NavbarService,
    private cookieService: CookieService
  ) {}

  ngOnInit(): void {
    const token = this.cookieService.get('authToken');
  
    if (token) {
      // Temporarily hide navbar to prevent flicker (user already logged in)
      this.navbarService.setNavbarType('hidden');
  
      setTimeout(() => {
        const decoded: DecodedToken | null = decodeToken(token);
        const role = decoded?.role?.toLowerCase();
  
        const allowedRoles = ['admin', 'instructor', 'student'] as const;
  
        if (role && allowedRoles.includes(role as typeof allowedRoles[number])) {
          this.navbarService.setNavbarType(role as typeof allowedRoles[number]);
        } else {
          this.navbarService.setNavbarType('default');
        }
      }, 100); 
    } else {
      this.navbarService.setNavbarType('default');
    }
  }
  
  
}  
























 // ngOnInit(): void {
  //   this.navbarService.navbarType$.subscribe(type => {
  //     this.navbarType = type;
  //   });
  // }
