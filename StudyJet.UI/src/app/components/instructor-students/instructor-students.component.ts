import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { environment } from '../../../environments/environment';
import { UserService } from '../../services/user.service';
import { CourseWithStudents } from '../../models';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-instructor-students',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './instructor-students.component.html',
  styleUrl: './instructor-students.component.css'
})
export class InstructorStudentsComponent implements OnInit {
  coursesWithStudents: CourseWithStudents[] = []; 
  error = '';
  showCourseList = false;

  constructor(
    private userService: UserService,
    private cdr: ChangeDetectorRef,
    private imageService: ImageService,
  ) {}

  ngOnInit(): void {
    this.loadCoursesWithStudents();
  }

  // Fetch courses with students 
  loadCoursesWithStudents(): void {
    this.userService.getCoursesWithStudentsForInstructor().subscribe(
      (response) => {
        this.coursesWithStudents = response.map(course => {
          return {
            ...course,
            courseImageUrl: course.imageUrl 
              ? this.imageService.getCourseImageUrl(course.imageUrl) 
              : undefined,
            
            students: course.students.map(student => {
              return {
                ...student,
                 // Get the student profile image URL through the ImageService
                profilePictureUrl: student.profilePictureUrl 
                  ? `${environment.imageBaseUrl}${student.profilePictureUrl.startsWith('/') ? '' : '/'}${student.profilePictureUrl}`
                  : undefined
              };
            })
          };
        });
        this.cdr.detectChanges();
        this.showCourseList = true;
      },
      (error) => {
        console.error('Error fetching courses with students:', error);
        this.error = 'Failed to fetch courses with students.';
      }
    );
  }
}


