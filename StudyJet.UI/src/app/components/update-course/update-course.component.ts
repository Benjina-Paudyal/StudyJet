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
    this.courseId = +this.route.snapshot.paramMap.get('id')!;
    this.loadCourseData();
  
    this.updateCourseForm = this.fb.group({
      title: [''],
      description: [''],
      price: [''],
      videoUrl: [''],
      imageFile: [null] 
    });
  }

  loadCourseData() {
    this.courseService.getCourseById(this.courseId).subscribe(course => {
      this.updateCourseForm.patchValue({
        title: course.title,
        description: course.description,
        price: course.price,
        videoUrl: course.videoUrl
      });
      
      if (course.imageUrl) {
        this.imagePreview = this.imageService.getCourseImageUrl(course.imageUrl);
      }
    });
      
  }

  onFileChange(event: any) {
    const file = event.target.files.length > 0 ? event.target.files[0] : null;
  
    if (file) {
      this.selectedFile = file;
  
      // Preview Image
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string; 
      };
      reader.readAsDataURL(file); 
    }
  }

  updateCourse() {
    const formData = new FormData();
    formData.append('title', this.updateCourseForm.get('title')?.value || '');
    formData.append('description', this.updateCourseForm.get('description')?.value || '');
    formData.append('price', this.updateCourseForm.get('price')?.value || '');
    formData.append('videoUrl', this.updateCourseForm.get('videoUrl')?.value || '');
    
    if (this.selectedFile) {
      formData.append('imageFile', this.selectedFile);
    }
  
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




