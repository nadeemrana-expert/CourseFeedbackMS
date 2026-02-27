import { Component, OnInit } from '@angular/core';
import { FeedbackService, DashboardDto } from '../services/feedback.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  dashboardData: DashboardDto;
  loading = true;

  constructor(private feedbackService: FeedbackService) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.feedbackService.getDashboardData().subscribe({
      next: (res) => {
        this.dashboardData = res.result;
        this.loading = false;
      },
      error: (err) => {
        console.error('Dashboard API error:', err);
        this.dashboardData = {
          totalFeedbackCount: 0,
          totalCourseCount: 0,
          averageRating: null,
          userRole: '',
          topCoursesByRating: [],
          recentFeedbacks: []
        };
        this.loading = false;
      }
    });
  }

  get courseLabel(): string {
    switch (this.dashboardData?.userRole) {
      case 'Student': return 'Courses Reviewed';
      case 'Teacher': return 'My Courses';
      default: return 'Total Courses';
    }
  }

  get feedbackLabel(): string {
    switch (this.dashboardData?.userRole) {
      case 'Student': return 'My Feedbacks';
      case 'Teacher': return 'Course Feedbacks';
      default: return 'Total Feedbacks';
    }
  }

  get ratingLabel(): string {
    switch (this.dashboardData?.userRole) {
      case 'Student': return 'My Avg Rating';
      case 'Teacher': return 'My Courses Avg';
      default: return 'Overall Avg Rating';
    }
  }

  get topCoursesTitle(): string {
    switch (this.dashboardData?.userRole) {
      case 'Student': return 'Courses I Reviewed';
      case 'Teacher': return 'My Courses by Rating';
      default: return 'Top Courses by Rating';
    }
  }

  get recentFeedbacksTitle(): string {
    switch (this.dashboardData?.userRole) {
      case 'Student': return 'My Recent Feedbacks';
      case 'Teacher': return 'Recent Student Feedbacks';
      default: return 'Recent Feedbacks';
    }
  }
}
