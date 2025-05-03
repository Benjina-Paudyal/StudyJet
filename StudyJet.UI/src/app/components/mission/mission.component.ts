import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-mission',
  standalone: true,
  imports: [],
  templateUrl: './mission.component.html',
  styleUrl: './mission.component.css'
})
export class MissionComponent {

  constructor(
    private router: Router
  ) {}

  handleJoinClick(): void {
    alert('Please login first to join our courses.');
    this.router.navigate(['/login']);
  }
}


