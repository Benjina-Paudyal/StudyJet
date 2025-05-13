import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category, Course } from '../models';


@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private apiUrl = `${environment.apiBaseUrl}/category`;

  constructor(
    private http: HttpClient
  ) { }

  // Fetch all categories
  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }

  // Fetch the courses for the specific category
  getCoursesByCategory(categoryId: number): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.apiUrl}/${categoryId}`);
  }

  // Fetch the category by its ID
  getCategoryById(categoryId: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${categoryId}`);
  }

  // Add category
  addCategory(category: { name: string }): Observable<number> {
    return this.http.post<number>(this.apiUrl, category);
  }
}
