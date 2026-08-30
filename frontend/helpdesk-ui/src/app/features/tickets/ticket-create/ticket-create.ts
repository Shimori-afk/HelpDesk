import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Ticket as TicketApi } from '../../../core/services/ticket';
import { Category as CategoryApi } from '../../../core/services/category';
import { Department as DepartmentApi } from '../../../core/services/department';
import { Category, Department } from '../../../core/models/lookup.model';
import { TicketPriority } from '../../../core/models/ticket.model';
import { ApiError } from '../../../core/models/api-error.model';

const PRIORITIES: TicketPriority[] = ['Low', 'Medium', 'High', 'Critical'];

@Component({
  selector: 'app-ticket-create',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './ticket-create.html',
  styleUrl: './ticket-create.scss'
})
export class TicketCreate implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly ticketApi = inject(TicketApi);
  private readonly categoryApi = inject(CategoryApi);
  private readonly departmentApi = inject(DepartmentApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly priorities = PRIORITIES;
  protected readonly categories = signal<Category[]>([]);
  protected readonly departments = signal<Department[]>([]);
  protected readonly submitting = signal(false);
  protected readonly loading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  private readonly editId = signal<number | null>(null);
  protected readonly isEditMode = computed(() => this.editId() !== null);

  readonly form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    categoryId: [null as number | null, [Validators.required]],
    priority: ['Medium' as TicketPriority, [Validators.required]],
    departmentId: [null as number | null],
    location: ['', [Validators.maxLength(200)]]
  });

  ngOnInit(): void {
    this.categoryApi.getAll().subscribe((categories) => this.categories.set(categories));
    this.departmentApi.getAll().subscribe((departments) => this.departments.set(departments));

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      this.editId.set(id);
      this.form.controls.priority.disable();
      this.loading.set(true);
      this.ticketApi.getTicket(id).subscribe({
        next: (ticket) => {
          this.form.patchValue({
            title: ticket.title,
            description: ticket.description,
            categoryId: ticket.categoryId,
            priority: ticket.priority,
            departmentId: ticket.departmentId,
            location: ticket.location
          });
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('Could not load this ticket for editing.');
        }
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const raw = this.form.getRawValue();
    const id = this.editId();

    const request$ = id
      ? this.ticketApi.updateTicket(id, {
          title: raw.title!,
          description: raw.description!,
          categoryId: raw.categoryId!,
          departmentId: raw.departmentId,
          location: raw.location || null
        })
      : this.ticketApi.createTicket({
          title: raw.title!,
          description: raw.description!,
          categoryId: raw.categoryId!,
          priority: raw.priority!,
          departmentId: raw.departmentId,
          location: raw.location || null
        });

    request$.subscribe({
      next: (ticket) => this.router.navigate(['/tickets', ticket.id]),
      error: (error: unknown) => {
        this.submitting.set(false);
        const apiError = error instanceof HttpErrorResponse ? (error.error as ApiError) : undefined;
        this.errorMessage.set(apiError?.title ?? 'Could not save the ticket.');
      }
    });
  }
}
