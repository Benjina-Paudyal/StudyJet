import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {
  constructor() {}

  ngOnInit() {
    this.updatePlaceholderText(window.innerWidth);
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event) {
    this.updatePlaceholderText((event.target as Window).innerWidth);
  }

  private updatePlaceholderText(width: number) {
    const searchInput = document.getElementById('search-bar') as HTMLInputElement;
    if (width <= 250) {
      searchInput.placeholder = '?'; 
    } else if (width <= 395) {
      searchInput.placeholder = 'Courses'; 
    } else {
      searchInput.placeholder = 'What do you want to learn?'; 
    }
  }
 
  isNavbarCollapsed = true;

   toggleNavbar() {
    this.isNavbarCollapsed = !this.isNavbarCollapsed;
  }
}



