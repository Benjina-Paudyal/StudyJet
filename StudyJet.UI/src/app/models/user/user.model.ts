import { Course } from "../course/course.model";
import { UserRole } from "./user-role-enum";

export interface User {
    userID: string;                   
    userName: string;  
    fullName: string;             
    email: string;                    
    password: string;                
    role: UserRole;                   
    profilePictureUrl?: string;    
    purchasedCourses?: Course[];
    createdCourses?: Course[]; 
  }