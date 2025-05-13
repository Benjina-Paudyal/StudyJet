import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { User } from '../../models';

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
    // Fetch all instructors on component initialization
    this.userService.getInstructors().subscribe({
      next: (instructors) => {
        if (Array.isArray(instructors)) {
          this.instructors = instructors;
        } else {
          console.error("API returned a non-array response", instructors);
        }
        this.loading = false;
      },
      error: (err) => {
        console.error("Error fetching instructors:", err);
        this.errorMessage = 'Failed to load instructors.';
        this.loading = false;
      }
    });
  }

 // Navigate to instructor registration form
  navigateToRegisterInstructor() {
    this.router.navigate(['/register-instructor']); 
  }
}




