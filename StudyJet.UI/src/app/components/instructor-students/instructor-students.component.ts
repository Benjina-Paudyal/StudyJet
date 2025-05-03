import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { environment } from '../../../environments/environment';
import { UserService } from '../../services/user.service';
import { CourseWithStudents } from '../../models';

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
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadCoursesWithStudents();
  }

  loadCoursesWithStudents(): void {
    this.userService.getCoursesWithStudentsForInstructor().subscribe(
      (response) => {
        this.coursesWithStudents = response.map(course => {
          return {
            ...course,
            courseImageUrl: course.imageUrl 
              ? `${environment.imageBaseUrl}${course.imageUrl.startsWith('/') ? '' : '/'}${course.imageUrl}`
              : undefined,
            
            students: course.students.map(student => {
              return {
                ...student,
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


