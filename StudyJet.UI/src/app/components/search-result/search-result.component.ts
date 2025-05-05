import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { Course } from '../../models';
import { CourseService } from '../../services/course.service';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-search-result',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './search-result.component.html',
  styleUrl: './search-result.component.css'
})
export class SearchResultComponent implements OnInit{
  searchQuery = '';
    courses: Course[] = [];
    dropdownVisible = false;

    constructor(
      private route: ActivatedRoute, 
      private courseService: CourseService,
      private imageService: ImageService) { }

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            this.searchQuery = params['query'];
            this.fetchCourses(this.searchQuery);
        });
    }

    fetchCourses(query: string) {
      this.courses = [];
      this.courseService.searchCourses(query).subscribe({
        next: (courses) => {
          this.courses = courses.map(course => {
            course.imageUrl = this.imageService.getCourseImageUrl(course.imageUrl);
            return course;
          });
        },
        error: (err) => {
          console.error('Error fetching search results:', err);
          this.courses = []; // Show "no results" message
        }
      });
    }

}




