import { Routes } from '@angular/router';
import { HomepageComponent } from './components/homepage/homepage.component';
import { CourseComponent } from './components/course/course.component';
import { RegisterComponent } from './components/register/register.component';
import { EmailConfirmationComponent } from './components/email-confirmation/email-confirmation.component';
import { CategoryComponent } from './components/category/category.component';
import { CategoryCourseComponent } from './components/category-course/category-course.component';
import { LoginComponent } from './components/login/login.component';
import { StudentDashboardComponent } from './components/student-dashboard/student-dashboard.component';
import { AuthGuard } from './guards/auth.guard';
import { InstructorDashboardComponent } from './components/instructor-dashboard/instructor-dashboard.component';



export const routes: Routes = [
    { path: '', redirectTo: '/home', pathMatch: 'full' },
    { path: 'home', component: HomepageComponent },
    { path: 'courses', component: CourseComponent },
    { path: 'register', component: RegisterComponent},
    { path: 'confirmation', component: EmailConfirmationComponent },
    { path: 'category', component: CategoryComponent},
    { path: 'courses/category/:categoryId', component: CategoryCourseComponent },
    { path: 'login' , component: LoginComponent},
    { path: 'student-dashboard', component: StudentDashboardComponent, canActivate: [AuthGuard], data: { role: 'Student'}},
    { path: 'instructor-dashboard', component: InstructorDashboardComponent, canActivate: [AuthGuard], data: { role: 'Instructor'}},

];
