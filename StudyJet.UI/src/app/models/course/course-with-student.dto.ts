import { Student } from "../user/student.dto";

export interface CourseWithStudents {
  courseId: number;
  title: string;
  imageUrl: string;
  students: Student[];
  courseImageUrl?: string;
}
