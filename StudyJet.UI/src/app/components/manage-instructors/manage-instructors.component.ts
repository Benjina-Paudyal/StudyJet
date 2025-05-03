import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { User } from '../../models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-manage-instructors',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manage-instructors.component.html',
  styleUrl: './manage-instructors.component.css'
})
export class ManageInstructorsComponent implements OnInit{

  instructors: User[] = [];
  profileImageUrl: string = ''; 
  loading: boolean = true;  
  errorMessage: string | null = null;

  constructor(
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userService.getInstructors().subscribe({
      next: (instructors) => {
        if (Array.isArray(instructors)) {
          this.instructors = instructors;
        } else {
          console.error("API returned a non-array response", instructors);
        }
      },
      error: (err) => {
        console.error("Error fetching instructors:", err);
      }
    });
  }


  navigateToRegisterInstructor() {
    this.router.navigate(['/register-instructor']); 
  }
}




