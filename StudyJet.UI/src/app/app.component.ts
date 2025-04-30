import { Component, OnInit } from '@angular/core';
import { NavbarComponent } from './components/navbar/navbar.component';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from './components/footer/footer.component';
import { CookieService } from 'ngx-cookie-service';
import { NavbarService } from './services/navbar.service';
import { DecodedToken, decodeToken } from './models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavbarComponent, RouterOutlet, FooterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit{
  title = 'StudyJet.UI';

  constructor(
    private cookieService: CookieService,
    private navbarService: NavbarService
  ) {}

  ngOnInit() : void{
    const token = this.cookieService.get('authToken');
    if (token) {
      const decoded: DecodedToken | null = decodeToken(token);
      if (decoded?.role) {
        this.navbarService.setNavbarType(decoded.role.toLowerCase());
      }
    }
  }
}

