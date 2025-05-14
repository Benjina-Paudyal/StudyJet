# StudyJet - A Course Management System

**StudyJet** is a course management system built with **Angular** (frontend) and **ASP.NET Core Web API** (backend). It allows students to browse, enroll, and manage courses, while instructors can create and manage courses. The project uses **ASP.NET Identity** for authentication, **Stripe (sandbox)** for payments, and **JWT** for secure API access.

##  Key Features

- **User Authentication**: Register, login, and role management with ASP.NET Identity.
- **Role-based Authorization**: Student, Instructor, and Admin roles with proper permissions.
- **Course Management**: Instructors can create, update, and manage courses.
- **Payments**: Integrated with Stripe (Sandbox) for course purchases.
- **JWT Authentication**: Secured API access using JWT tokens.
- **Email Support**:
  - **SMTP** for email confirmation and password reset.
  - **EmailJS** in the frontend for sending CVs to apply as an instructor.

##  Technologies Used

- **Frontend**: Angular  
- **Backend**: ASP.NET Core Web API  
- **Authentication**: ASP.NET Identity, JWT  
- **Payments**: Stripe (Sandbox)  
- **Emails**: SMTP (backend), EmailJS (frontend)


## Environment Variables

This project follows **secure development practices** by keeping sensitive information (like database passwords, API keys, and SMTP credentials) outside of the source code using environment variables.

Environment variables are referenced in `appsettings.json` like:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Database=...;Password=${DB_PASSWORD};"
}
```

#Required Environment Variables:

DB_PASSWORD - Database connection password

JWT_KEY - Secret key for JWT token generation

DEFAULT_PASSWORD - Default user password

INSTRUCTOR_PASSWORD - Default instructor password

SMTP_PASSWORD - Email service password

STRIPE_SECRET_KEY - Stripe API secret key

STRIPE_PUBLISHABLE_KEY - Stripe client-side key

Set these variables in your system environment before running the project.
