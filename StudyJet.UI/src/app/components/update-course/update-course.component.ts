import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormGroup, FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CourseService } from '../../services/course.service';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-update-course',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './update-course.component.html',
  styleUrl: './update-course.component.css'
})
export class UpdateCourseComponent implements OnInit {
  loading = false;
  updateCourseForm!: FormGroup;
  courseId!: number;
  selectedFile: File | null = null;
  imagePreview: string | ArrayBuffer | null = null;

  constructor(
    private fb: FormBuilder,
    private courseService: CourseService,
    private route: ActivatedRoute,
    private router: Router,
    private imageService: ImageService
  ) {}

  ngOnInit(): void {
    // Extract course ID from the route parameters
    this.courseId = +this.route.snapshot.paramMap.get('id')!;
    this.loadCourseData();
  
    // Initialize the form group with default values
    this.updateCourseForm = this.fb.group({
      title: [''],
      description: [''],
      price: [''],
      videoUrl: [''],
      imageFile: [null] 
    });
  }

  // Method to load course data from backend and populate the form
  loadCourseData() {
    this.courseService.getCourseById(this.courseId).subscribe(course => {

      // Populate form with the course data
      this.updateCourseForm.patchValue({
        title: course.title,
        description: course.description,
        price: course.price,
        videoUrl: course.videoUrl
      });
      
      // Set image preview if a course image exists
      if (course.imageUrl) {
        this.imagePreview = this.imageService.getCourseImageUrl(course.imageUrl);
      }
    });
  }

  onFileChange(event: any) {
    const file = event.target.files.length > 0 ? event.target.files[0] : null;
  
    if (file) {
      this.selectedFile = file;
  
      // Create a FileReader to show the selected image as a preview
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string; 
      };
      reader.readAsDataURL(file); 
    }
  }

  // Method to handle the form submission for updating the course
  updateCourse() {
    const formData = new FormData();

    // Append form values to form data
    formData.append('title', this.updateCourseForm.get('title')?.value || '');
    formData.append('description', this.updateCourseForm.get('description')?.value || '');
    formData.append('price', this.updateCourseForm.get('price')?.value || '');
    formData.append('videoUrl', this.updateCourseForm.get('videoUrl')?.value || '');
    
    // Append selected image file if available
    if (this.selectedFile) {
      formData.append('imageFile', this.selectedFile);
    }
  
    // Call the service to update the course with the form data
    this.courseService.updateCourse(this.courseId, formData).subscribe({
      next: () => {
        alert('Course updated successfully');
        this.router.navigate(['/instructor-dashboard']);
      },
      error: (err) => {
        console.error('Update failed:', err);
        alert('Update failed: ' + err.error);
      }
    });
  }
}




