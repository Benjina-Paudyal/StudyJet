import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Category, Course } from '../../models';
import { AuthService } from '../../services/auth.service';
import { CategoryService } from '../../services/category.service';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-category-course',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './category-course.component.html',
  styleUrl: './category-course.component.css'
})
export class CategoryCourseComponent implements OnInit{
  categoryId : number | null = null;
  categoryName = '';
  courses: Course[] = [];

  constructor (
    private route: ActivatedRoute,
    private categoryService: CategoryService,
    private imageService: ImageService,
    
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      this.categoryId = Number(params.get('categoryId'));
      if (this.categoryId !== null) {
        this.loadCategoryName(this.categoryId);
        this.loadCourses(this.categoryId);
      }
    });
  }
  

  loadCourses(categoryId: number): void {
    console.log('Calling loadCourses for categoryID:', categoryId);
    this.categoryService.getCoursesByCategory(categoryId).subscribe({
      next: (response: any) => {
        const courses = response.courses;
        this.courses = courses.map((course: Course) => {
          return course;
        });
      },
      error: (err) => {
        console.error('Error fetching courses:', err);
      },
    });
  }

// Fetch course image URL
getCourseImageUrl(imageFilename: string): string {
  return this.imageService.getCourseImageUrl(imageFilename);
}


loadCategoryName(categoryId: number): void {
  this.categoryService.getCategoryById(categoryId).subscribe({
    next: (cat: Category) => this.categoryName = cat.name,
    error: (err)   => console.error('Error fetching category name', err),
  });
}

  
}



