import { Component } from '@angular/core';
import { Category } from '../../models';
import { CategoryService } from '../../services/category.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manage-categories',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './manage-categories.component.html',
  styleUrl: './manage-categories.component.css'
})
export class ManageCategoriesComponent {
  categories: Category[] = [];
  newCategory = '';

  constructor(
    private categoryService: CategoryService,
  ) { }

   ngOnInit(): void {
   this.loadCategories();  
   }

  // Fetch Categories
private loadCategories(): void {
  this.categoryService.getCategories().subscribe({
    next: cats => {
      console.log('API Response:', cats); 
      this.categories = cats;
    },
    error: err => console.error('Error loading categories', err)
  });
}

  // Add new category
  addCategory(): void {
    const trimmedName = this.newCategory.trim();
    if (!trimmedName) {
      alert('Category name is required');
      return;
    }

    this.categoryService.addCategory({ name: trimmedName }).subscribe({
      next: (categoryId) => {
        alert('Category added successfully with ID: ' + categoryId);
        this.newCategory = '';
      },
      error: (err) => {
        alert('Error adding category: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }
}
