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

  getCategories() : Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }

  getCoursesByCategory(categoryId: number): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.apiUrl}/${categoryId}`);
  }

  getCategoryById(categoryId: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${categoryId}`);
  }


}
