import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { Category } from '../../models';
import { CategoryService } from '../../services/category.service';
import { CourseService } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {
  categories: Category[] = [];
  constructor(
    private categoryService: CategoryService,
    private router: Router,
    private courseService: CourseService,
    private authService: AuthService,
    private userService: UserService,
    
  ) {}

  ngOnInit() {
    this.updatePlaceholderText(window.innerWidth);
    this.loadCategories();
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

   // Load categories from the category service
   loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data: Category[]) => {
        this.categories = data;
      },
      error: (err) => {
        console.error('Error fetching categories', err);
      },
    });
  }

  // Handle category selection
  onCategorySelected(category: Category): void {
    console.log('Selected category:', category);
  }

}



