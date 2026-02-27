import { Component, Injector, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FeedbackService, FeedbackDto } from '../services/feedback.service';
import { CourseService, CourseDto } from '../services/course.service';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';

@Component({
  selector: 'app-feedbacks',
  templateUrl: './feedbacks.component.html'
})
export class FeedbacksComponent extends AppComponentBase implements OnInit {
  feedbacks: FeedbackDto[] = [];
  activeCourses: CourseDto[] = [];
  totalCount = 0;
  pageSize = 10;
  loading = false;
  showModal = false;
  isEdit = false;
  selectedId: number;
  filterText = '';
  courseFilter: number;
  ratingFilter: number;
  feedbackForm: FormGroup;
  uploadedFilePath = '';
  uploadedFileName = '';
  uploading = false;
  editStudentName = '';

  // Permission flags
  canCreate = false;
  canEdit = false;
  canDelete = false;

  ratingOptions = [
    { label: '1 - Poor', value: 1 },
    { label: '2 - Fair', value: 2 },
    { label: '3 - Good', value: 3 },
    { label: '4 - Very Good', value: 4 },
    { label: '5 - Excellent', value: 5 }
  ];

  constructor(
    injector: Injector,
    private feedbackService: FeedbackService,
    private courseService: CourseService,
    private fb: FormBuilder,
    private confirmationService: ConfirmationService,
    private msg: MessageService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.canCreate = this.permission.isGranted('Pages.Feedbacks.Create');
    this.canEdit = this.permission.isGranted('Pages.Feedbacks.Edit');
    this.canDelete = this.permission.isGranted('Pages.Feedbacks.Delete');
    this.buildForm();
    this.loadActiveCourses();
    this.loadFeedbacks({ first: 0, rows: this.pageSize });
  }

  buildForm(): void {
    this.feedbackForm = this.fb.group({
      courseId: [null, Validators.required],
      rating: [null, [Validators.required, Validators.min(1), Validators.max(5)]],
      comment: ['', Validators.maxLength(2000)]
    });
  }

  loadActiveCourses(): void {
    this.courseService.getActiveCourses().subscribe(res => {
      this.activeCourses = res.result.items;
    });
  }

  loadFeedbacks(event: any): void {
    this.loading = true;
    this.feedbackService.getAll(
      this.filterText, this.courseFilter, this.ratingFilter, event.first, event.rows
    ).subscribe({
      next: (res) => {
        this.feedbacks = res.result.items;
        this.totalCount = res.result.totalCount;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  onSearch(): void {
    this.loadFeedbacks({ first: 0, rows: this.pageSize });
  }

  openCreateModal(): void {
    this.isEdit = false;
    this.feedbackForm.reset();
    this.uploadedFilePath = '';
    this.uploadedFileName = '';
    this.showModal = true;
  }

  openEditModal(fb: FeedbackDto): void {
    this.isEdit = true;
    this.selectedId = fb.id;
    this.editStudentName = fb.studentName;
    this.uploadedFilePath = fb.attachmentPath || '';
    this.uploadedFileName = fb.attachmentFileName || '';
    this.feedbackForm.patchValue({
      courseId: fb.courseId,
      rating: fb.rating,
      comment: fb.comment
    });
    this.showModal = true;
  }

  onFileUpload(event: any): void {
    const file: File = event.files[0];
    if (!file) { return; }
    this.uploading = true;
    this.feedbackService.uploadAttachment(file).subscribe({
      next: (res) => {
        this.uploadedFilePath = res.result.filePath;
        this.uploadedFileName = file.name;
        this.uploading = false;
        this.msg.add({
          severity: 'info',
          summary: 'Uploaded',
          detail: 'File uploaded successfully.'
        });
      },
      error: (err) => {
        this.uploading = false;
        this.msg.add({
          severity: 'error',
          summary: 'Error',
          detail: err?.error || 'Upload failed.'
        });
      }
    });
  }

  saveFeedback(): void {
    if (this.feedbackForm.invalid) { return; }
    const input = {
      ...this.feedbackForm.value,
      attachmentPath: this.uploadedFilePath,
      attachmentFileName: this.uploadedFileName,
      ...(this.isEdit ? { id: this.selectedId } : {})
    };

    const op = this.isEdit
      ? this.feedbackService.update(input)
      : this.feedbackService.create(input);

    op.subscribe({
      next: () => {
        this.msg.add({
          severity: 'success',
          summary: 'Saved',
          detail: 'Feedback saved.'
        });
        this.showModal = false;
        this.onSearch();
      },
      error: (err) => {
        this.msg.add({
          severity: 'error',
          summary: 'Error',
          detail: err?.error?.error?.message || 'An error occurred.'
        });
      }
    });
  }

  deleteFeedback(id: number): void {
    this.confirmationService.confirm({
      message: 'Delete this feedback?',
      accept: () => {
        this.feedbackService.delete(id).subscribe({
          next: () => {
            this.msg.add({
              severity: 'success',
              summary: 'Deleted',
              detail: 'Feedback deleted.'
            });
            this.onSearch();
          }
        });
      }
    });
  }

  getFileUrl(path: string): string {
    return `${AppConsts.remoteServiceBaseUrl}${path}`;
  }
}
