import { Component } from '@angular/core';
import { NavbarComponent } from './navbar/navbar.component';
import { ShowcaseComponent } from './showcase/showcase.component';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from './footer/footer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NavbarComponent, RouterOutlet, FooterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'StudyJet.UI';
}

