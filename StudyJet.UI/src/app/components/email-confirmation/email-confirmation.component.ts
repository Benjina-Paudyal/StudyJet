import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-email-confirmation',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './email-confirmation.component.html',
  styleUrl: './email-confirmation.component.css'
})
export class EmailConfirmationComponent implements OnInit{
  confirmationSuccess = false ;
  confirmationMessage = '';

  constructor(
    private route: ActivatedRoute, 
    private router: Router
  ) {}

  ngOnInit(): void {
    // Subscribe to query parameters from the URL
    this.route.queryParams.subscribe(params => {
      if (params['confirmed'] === 'true') {
        this.confirmationSuccess = true;
        this.confirmationMessage = "Email confirmed! Redirecting to login…";
        setTimeout(() => {
          this.router.navigate(['/home']);
        }, 5000);
      } else {
        this.confirmationMessage = params['error'] || "Email confirmation failed. Please try again.";
      }
    });
  }
}


