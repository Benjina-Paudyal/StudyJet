import { Component } from '@angular/core';
import { ShowcaseComponent } from '../showcase/showcase.component';
import { PopularCourseComponent } from "../popular-course/popular-course.component";

@Component({
  selector: 'app-homepage',
  standalone: true,
  imports: [ShowcaseComponent, PopularCourseComponent],
  templateUrl: './homepage.component.html',
  styleUrl: './homepage.component.css'
})
export class HomepageComponent {

}
