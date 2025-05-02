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
import { AddCourseComponent } from './components/add-course/add-course.component';
import { CourseDetailComponent } from './components/course-detail/course-detail.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
import { CartComponent } from './components/cart/cart.component';
import { WishlistComponent } from './components/wishlist/wishlist.component';



export const routes: Routes = [
    { path: '', redirectTo: '/home', pathMatch: 'full' },
    { path: 'home', component: HomepageComponent },
    { path: 'courses', component: CourseComponent },
    { path: 'courses/:id', component: CourseDetailComponent },
    { path: 'courses/category/:categoryId', component: CategoryCourseComponent },
    { path: 'register', component: RegisterComponent},
    { path: 'login' , component: LoginComponent},
    { path: 'category', component: CategoryComponent},
   


    // Admin routes
    { path: 'admin-dashboard', component: AdminDashboardComponent, canActivate: [AuthGuard], data:{ role:'Admin' }},





    // Student routes
    { path: 'student-dashboard', component: StudentDashboardComponent, canActivate: [AuthGuard], data: { role: 'Student'}},
    { path: 'cart', component: CartComponent,canActivate: [AuthGuard], data: { role: 'Student'}},
    { path: 'wishlist', component: WishlistComponent,canActivate: [AuthGuard], data: { role: 'Student'}},







    // Instructor routes
    { path: 'instructor-dashboard', component: InstructorDashboardComponent, canActivate: [AuthGuard], data: { role: 'Instructor'}},
    { path: 'add-course', component: AddCourseComponent, canActivate: [AuthGuard], data: { role: 'Instructor'}},






    // Other common routes
    { path: 'confirmation', component: EmailConfirmationComponent },
   
  
  
   
];
