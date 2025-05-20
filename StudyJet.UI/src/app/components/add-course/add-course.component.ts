import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CourseService } from '../../services/course.service';
import { CategoryService } from '../../services/category.service';
import { Router } from '@angular/router';
import { Category } from '../../models';
import { CookieService } from 'ngx-cookie-service';
import { youtubeUrlValidator } from '../../validators/custom.validator';

@Component({
  selector: 'app-add-course',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './add-course.component.html',
  styleUrl: './add-course.component.css'
})
export class AddCourseComponent implements OnInit {

  courseForm!: FormGroup;
  loading = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  categories: Category[] = [];
  selectedCategoryId: number | null = null;
  isDropdownOpen = false;
  profileImageUrl: string = '';

  constructor(
    private formBuilder: FormBuilder,
    private courseService: CourseService,
    private categoryService: CategoryService,
    private cookieService: CookieService,
    private router: Router,
  ) { }

  // Toggles the dropdown visibility
  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  // Sets the selected category when a category is chosen
  selectCategory(event: any): void {
    const categoryId = (event.target as HTMLSelectElement).value;
    this.selectedCategoryId = Number(categoryId);
    this.courseForm.patchValue({ categoryID: this.selectedCategoryId });
    this.courseForm.get('categoryID')?.updateValueAndValidity();
    this.isDropdownOpen = false;
  }

  // Fetches category name based on category ID
  getCategoryNameById(categoryId: number): string {
    const category = this.categories.find(c => c.categoryID === categoryId);
    return category ? category.name : 'Select a category';
  }

  // Component initialization, setting up form and fetching categories
  ngOnInit(): void {
    this.courseForm = this.formBuilder.group({
      title: ['', [Validators.required]],
      description: ['', [Validators.required]],
      imageUrl: ['', [Validators.required]],
      price: [0, [Validators.required, Validators.min(1)]],
      instructorID: ['', [Validators.required]],
      categoryID: [null, [Validators.required]],
      creationDate: [new Date(), [Validators.required]],
      lastUpdatedDate: [new Date(), [Validators.required]],
      videoUrl: ['', [Validators.required, youtubeUrlValidator()]],
      isPopular: [false],
      totalPrice: [0, [Validators.required]],
      isApproved: [false],
      status: ['Pending', [Validators.required]]
    });
    this.fetchCategories();
  }

  // Fetches the list of categories from the category service
  fetchCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (categories: Category[]) => {
        this.categories = categories;
      },
      error: (error) => {
        console.error('Error fetching categories:', error);
      }
    });
  }

  // Handles the form submission
  onSubmit(): void {
    const userId = this.cookieService.get('userId');
    const username = this.cookieService.get('username');
    this.courseForm.patchValue({
      instructorID: userId,
      instructorName: username
    });
    if (this.courseForm.invalid) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    // Prepare FormData to send the form along with the file
    const formData = new FormData();
    formData.append('title', this.courseForm.get('title')?.value);
    formData.append('description', this.courseForm.get('description')?.value);
    formData.append('price', this.courseForm.get('price')?.value);
    formData.append('instructorID', this.courseForm.get('instructorID')?.value);
    formData.append('categoryID', this.selectedCategoryId?.toString() || '');
    formData.append('imageFile', this.courseForm.get('imageUrl')?.value);
    formData.append('videoUrl', this.courseForm.get('videoUrl')?.value);
    formData.append('isPopular', this.courseForm.get('isPopular')?.value.toString());
    formData.append('instructorName', this.courseForm.get('instructorName')?.value);
    formData.append('yotalPrice', this.courseForm.get('totalPrice')?.value.toString());
    formData.append('status', "Pending");

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    // Call courseService to create the course with the form data
    this.courseService.createCourse(formData).subscribe({
      next: () => {
        this.successMessage = 'Course added successfully!';
        setTimeout(() => {
          this.router.navigate(['/instructor-courses']);
        }, 2000);
      },
      error: (err) => {
        console.error('Error adding course:', err);
        this.errorMessage = 'Error adding course.';
      },
      complete: () => (this.loading = false)
    });
  }

  // Handles file input change for image and video
  onFileChange(event: any, field: 'imageUrl'): void {
    const file = event.target.files[0];
    if (file) {
      if (!file.type.startsWith('image/')) {
        this.errorMessage = 'Please upload a valid image.';
        return;
      }
      this.courseForm.patchValue({ imageUrl: file });
    }
  }
}