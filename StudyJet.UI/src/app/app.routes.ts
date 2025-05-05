import { Routes } from '@angular/router';
import { HomepageComponent } from './components/homepage/homepage.component';
import { CourseComponent } from './components/course/course.component';
import { RegisterComponent } from './components/register/register.component';
import { EmailConfirmationComponent } from './components/email-confirmation/email-confirmation.component';
import { CategoryComponent } from './components/category/category.component';
import { CategoryCourseComponent } from './components/category-course/category-course.component';
import { LoginComponent } from './components/login/login.component';
import { StudentDashboardComponent } from './components/student-dashboard/student-dashboard.component';
import { authGuard } from './guards/auth.guard';
import { InstructorDashboardComponent } from './components/instructor-dashboard/instructor-dashboard.component';
import { AddCourseComponent } from './components/add-course/add-course.component';
import { CourseDetailComponent } from './components/course-detail/course-detail.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
import { CartComponent } from './components/cart/cart.component';
import { WishlistComponent } from './components/wishlist/wishlist.component';
import { SecuritySettingsComponent } from './components/security-settings/security-settings.component';
import { ChangePasswordComponent } from './components/change-password/change-password.component';
import { Enable2faComponent } from './components/enable2fa/enable2fa.component';
import { Confirm2faComponent } from './components/confirm2fa/confirm2fa.component';
import { Verify2faLoginComponent } from './components/verify2fa-login/verify2fa-login.component';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { InstructorCoursesComponent } from './components/instructor-courses/instructor-courses.component';
import { InstructorStudentsComponent } from './components/instructor-students/instructor-students.component';
import { ManageStudentsComponent } from './components/manage-students/manage-students.component';
import { ManageInstructorsComponent } from './components/manage-instructors/manage-instructors.component';
import { ManageCoursesComponent } from './components/manage-courses/manage-courses.component';
import { RegisterInstructorComponent } from './components/register-instructor/register-instructor.component';
import { PurchaseCourseComponent } from './components/purchase-course/purchase-course.component';
import { MissionComponent } from './components/mission/mission.component';
import { SuccessComponent } from './components/success/success.component';
import { PurchaseHistoryComponent } from './components/purchase-history/purchase-history.component';
import { SearchResultComponent } from './components/search-result/search-result.component';
import { NotificationComponent } from './components/notification/notification.component';
import { UpdateCourseComponent } from './components/update-course/update-course.component';



export const routes: Routes = [
    { path: '', redirectTo: '/home', pathMatch: 'full' },
    { path: 'home', component: HomepageComponent },
    { path: 'courses', component: CourseComponent },
    { path: 'courses/:id', component: CourseDetailComponent },
    { path: 'courses/category/:categoryId', component: CategoryCourseComponent },
    { path: 'register', component: RegisterComponent},
    { path: 'login' , component: LoginComponent},
    { path: 'category', component: CategoryComponent},
    { path: 'about', component: MissionComponent},
    { path: 'search-result', component: SearchResultComponent},
   

    // Admin routes
    { path: 'admin-dashboard', component: AdminDashboardComponent, canActivate: [authGuard], data:{ role:'Admin' }},
    { path: 'manage-students', component: ManageStudentsComponent, canActivate: [authGuard], data: { role:'Admin'}},
    { path: 'manage-instructors', component: ManageInstructorsComponent, canActivate: [authGuard], data:{role:'Admin'}},
    { path: 'manage-courses', component: ManageCoursesComponent, canActivate: [authGuard], data: { role: 'Admin'}},
    { path: 'register-instructor', component: RegisterInstructorComponent, canActivate: [authGuard], data: { role: 'Admin'}},

    // Student routes
    { path: 'student-dashboard', component: StudentDashboardComponent, canActivate: [authGuard], data: { role: 'Student'}},
    { path: 'cart', component: CartComponent,canActivate: [authGuard], data: { role: 'Student',message: 'Please login first to add items to your cart.'}},
    { path: 'wishlist', component: WishlistComponent,canActivate: [authGuard], data: { role: 'Student',message: 'Please login first to add items to your wishlist.' }},
    { path: 'purchase-history', component: PurchaseHistoryComponent, canActivate: [authGuard], data: { role: 'Student'}},
    { path: 'my-learning', component: PurchaseCourseComponent, canActivate: [authGuard], data: { role: 'Student'}},
    { path: 'success',component: SuccessComponent },

    // Instructor routes
    { path: 'instructor-dashboard', component: InstructorDashboardComponent, canActivate: [authGuard], data: { role: 'Instructor'}},
    { path: 'add-course', component: AddCourseComponent, canActivate: [authGuard], data: { role: 'Instructor'}},
    { path: 'update-course/:id', component: UpdateCourseComponent, canActivate: [authGuard], data: { role: 'Instructor'}},
    { path: 'instructor-courses', component: InstructorCoursesComponent, canActivate: [authGuard], data: { role: 'Instructor'}},
    { path: 'instructor-students', component: InstructorStudentsComponent,canActivate: [authGuard], data: { role: 'Instructor'}},
   
    // Other common routes
    { path: 'confirmation', component: EmailConfirmationComponent },
    { path: 'verify2fa-login', component: Verify2faLoginComponent },
    { path: 'confirm-2fa', component: Confirm2faComponent},
    { path: 'forgot-password', component: ForgotPasswordComponent},
    { path: 'reset-password', component: ResetPasswordComponent},
    { path: 'notifications', component: NotificationComponent },
    { path: 'security-settings', component: SecuritySettingsComponent, canActivate: [authGuard],
        children: [
          { path: '', redirectTo: 'change-password', pathMatch: 'full' },
          { path: 'change-password', component: ChangePasswordComponent },
          { path: 'enable-2fa', component: Enable2faComponent },
        ],
      },
      { path: '**', redirectTo: '/home' }
   
];
