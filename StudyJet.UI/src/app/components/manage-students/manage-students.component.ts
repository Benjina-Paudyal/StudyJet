import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { User } from '../../models';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-manage-students',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manage-students.component.html',
  styleUrl: './manage-students.component.css'
})
export class ManageStudentsComponent implements OnInit{

  students: User[] = [];

  constructor(
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.userService.getStudents().subscribe({
      next: (students) => {
       if(Array.isArray(students)) {
        this.students = students; 
      } else {
        console.error("API returned a non arrayresponse", students);
      }
      },
      error: (err) => {
        console.error("Error fetching students:", err);
      }

    });
  }
}



