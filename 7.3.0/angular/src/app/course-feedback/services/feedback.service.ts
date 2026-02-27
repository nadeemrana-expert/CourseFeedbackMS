import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';

export interface FeedbackDto {
  id: number;
  studentName: string;
  courseId: number;
  courseName: string;
  comment: string;
  rating: number;
  createdDate: Date;
  attachmentPath?: string;
  attachmentFileName?: string;
}

export interface DashboardDto {
  totalFeedbackCount: number;
  totalCourseCount: number;
  averageRating: number;
  userRole: string;
  topCoursesByRating: {
    courseName: string;
    averageRating: number;
    feedbackCount: number;
  }[];
  recentFeedbacks: {
    studentName: string;
    courseName: string;
    rating: number;
    createdDate: Date;
  }[];
}

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private readonly baseUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app/Feedback`;

  constructor(private http: HttpClient) {}

  getAll(
    filter?: string,
    courseId?: number,
    rating?: number,
    skipCount = 0,
    maxResultCount = 10
  ): Observable<any> {
    let params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString());

    if (filter) { params = params.set('Filter', filter); }
    if (courseId) { params = params.set('CourseId', courseId.toString()); }
    if (rating) { params = params.set('Rating', rating.toString()); }

    return this.http.get<any>(`${this.baseUrl}/GetAll`, { params });
  }

  create(input: Partial<FeedbackDto>): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Create`, input);
  }

  update(input: FeedbackDto): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/Update`, input);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Delete`, { params: { Id: id.toString() } });
  }

  getDashboardData(): Observable<{ result: DashboardDto }> {
    return this.http.get<any>(`${this.baseUrl}/GetDashboardData`);
  }

  uploadAttachment(file: File): Observable<{ result: { filePath: string; fileName: string } }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<any>(
      `${AppConsts.remoteServiceBaseUrl}/api/FileUpload/upload-feedback-attachment`,
      formData
    );
  }
}
