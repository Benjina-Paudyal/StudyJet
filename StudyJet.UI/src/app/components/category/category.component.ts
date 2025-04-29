import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Category } from '../../models/category.model';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-category',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './category.component.html',
  styleUrl: './category.component.css'
})
export class CategoryComponent implements OnInit{
  categories: Category[] = [];

  constructor(
    private categoryService : CategoryService,
    private route: ActivatedRoute
    ) {}

  ngOnInit(): void {
    if(this.categories.length === 0) {
      this.loadCategories();
    }
    const categoryId = this.route.snapshot.paramMap.get('categoryId');
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data: Category[]) => {
        this.categories = data; 
      },
      error: (err) => {
        console.error('Error fetching categories', err);
      }
    });
  }

}
