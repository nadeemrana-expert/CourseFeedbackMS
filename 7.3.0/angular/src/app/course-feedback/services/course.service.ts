import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';

export interface CourseDto {
  id: number;
  courseName: string;
  instructorName: string;
  isActive: boolean;
  creationTime: Date;
  feedbackCount: number;
  averageRating?: number;
}

export interface PagedResult<T> {
  totalCount: number;
  items: T[];
}

@Injectable({ providedIn: 'root' })
export class CourseService {
  private readonly baseUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app/Course`;

  constructor(private http: HttpClient) {}

  getAll(
    filter?: string,
    isActive?: boolean,
    skipCount = 0,
    maxResultCount = 10,
    sorting = 'CourseName'
  ): Observable<{ result: PagedResult<CourseDto> }> {
    let params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString())
      .set('Sorting', sorting);

    if (filter) { params = params.set('Filter', filter); }
    if (isActive !== undefined && isActive !== null) {
      params = params.set('IsActive', isActive.toString());
    }

    return this.http.get<any>(`${this.baseUrl}/GetAll`, { params });
  }

  get(id: number): Observable<{ result: CourseDto }> {
    return this.http.get<any>(`${this.baseUrl}/Get`, { params: { Id: id.toString() } });
  }

  create(input: Partial<CourseDto>): Observable<{ result: CourseDto }> {
    return this.http.post<any>(`${this.baseUrl}/Create`, input);
  }

  update(input: CourseDto): Observable<{ result: CourseDto }> {
    return this.http.put<any>(`${this.baseUrl}/Update`, input);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Delete`, { params: { Id: id.toString() } });
  }

  getActiveCourses(): Observable<{ result: { items: CourseDto[] } }> {
    return this.http.get<any>(`${this.baseUrl}/GetActiveCourses`);
  }
}
