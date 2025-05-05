import { Component } from '@angular/core';
import { ShowcaseComponent } from '../showcase/showcase.component';
import { PopularCourseComponent } from "../popular-course/popular-course.component";
import { TestimonialComponent } from '../testimonial/testimonial.component';
import { MissionComponent } from '../mission/mission.component';
import { ReactPromoComponent } from '../react-promo/react-promo.component';
import { BecomeInstructorComponent } from '../become-instructor/become-instructor.component';

@Component({
  selector: 'app-homepage',
  standalone: true,
  imports: [
    ShowcaseComponent, 
    PopularCourseComponent, 
    TestimonialComponent, 
    ReactPromoComponent,
    MissionComponent,
    BecomeInstructorComponent
  ],
  templateUrl: './homepage.component.html',
  styleUrl: './homepage.component.css'
})
export class HomepageComponent {
}

