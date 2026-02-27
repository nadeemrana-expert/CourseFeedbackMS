import { Component, Injector, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CourseService, CourseDto } from '../services/course.service';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-courses',
  templateUrl: './courses.component.html'
})
export class CoursesComponent extends AppComponentBase implements OnInit {
  courses: CourseDto[] = [];
  totalCount = 0;
  pageSize = 10;
  loading = false;
  showModal = false;
  isEdit = false;
  selectedId: number;
  filterText = '';
  isActiveFilter: boolean;
  sortField = 'CourseName';
  sortOrder = 1;
  courseForm: FormGroup;

  // Permission flags
  canCreate = false;
  canEdit = false;
  canDelete = false;

  activeFilterOptions = [
    { label: 'Active', value: true },
    { label: 'Inactive', value: false }
  ];

  constructor(
    injector: Injector,
    private courseService: CourseService,
    private fb: FormBuilder,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.canCreate = this.permission.isGranted('Pages.Courses.Create');
    this.canEdit = this.permission.isGranted('Pages.Courses.Edit');
    this.canDelete = this.permission.isGranted('Pages.Courses.Delete');
    this.buildForm();
    this.loadCourses({ first: 0, rows: this.pageSize, sortField: 'CourseName', sortOrder: 1 });
  }

  buildForm(): void {
    this.courseForm = this.fb.group({
      courseName: ['', [Validators.required, Validators.maxLength(200)]],
      instructorName: ['', [Validators.required, Validators.maxLength(200)]],
      isActive: [true]
    });
  }

  loadCourses(event: any): void {
    this.loading = true;
    const sorting = event.sortField
      ? `${event.sortField} ${event.sortOrder === 1 ? 'asc' : 'desc'}`
      : 'CourseName';

    this.courseService.getAll(
      this.filterText, this.isActiveFilter, event.first, event.rows, sorting
    ).subscribe({
      next: (res) => {
        this.courses = res.result.items;
        this.totalCount = res.result.totalCount;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  onSearch(): void {
    this.loadCourses({ first: 0, rows: this.pageSize, sortField: this.sortField, sortOrder: this.sortOrder });
  }

  openCreateModal(): void {
    this.isEdit = false;
    this.courseForm.reset({ isActive: true });
    this.showModal = true;
  }

  openEditModal(course: CourseDto): void {
    this.isEdit = true;
    this.selectedId = course.id;
    this.courseForm.patchValue({
      courseName: course.courseName,
      instructorName: course.instructorName,
      isActive: course.isActive
    });
    this.showModal = true;
  }

  saveCourse(): void {
    if (this.courseForm.invalid) { return; }

    const formValue = this.courseForm.value;

    const operation = this.isEdit
      ? this.courseService.update({ id: this.selectedId, ...formValue })
      : this.courseService.create(formValue);

    operation.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Course ${this.isEdit ? 'updated' : 'created'} successfully.`
        });
        this.showModal = false;
        this.onSearch();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err?.error?.error?.message || 'An error occurred.'
        });
      }
    });
  }

  deleteCourse(id: number): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this course?',
      accept: () => {
        this.courseService.delete(id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Deleted',
              detail: 'Course deleted.'
            });
            this.onSearch();
          }
        });
      }
    });
  }
}
