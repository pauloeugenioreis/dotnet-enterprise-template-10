import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditService } from '../../../core/services/data-services';
import { DomainEvent } from '../../../shared/models/models';
import { DropdownComponent } from '../../../shared/components/dropdown/dropdown.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ModalComponent } from '../../../shared/components/modal/modal.component';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
  selector: 'app-audit',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownComponent, PaginationComponent, ModalComponent],
  templateUrl: './audit.component.html'
})
export class AuditComponent implements OnInit, OnDestroy {
  private auditService = inject(AuditService);
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<void>();
  
  logs = signal<DomainEvent[]>([]);
  loading = signal(true);
  
  // Filters
  entityType = '';
  eventType = '';
  userId = '';
  fromDate: string = '';
  toDate: string = '';
  
  // Pagination
  page = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Detail Modal
  showModal = false;
  selectedLog: any = null;

  entityOptions = [
    { label: 'Todos', value: '' },
    { label: 'Pedidos', value: 'Order' },
    { label: 'Produtos', value: 'Product' }
  ];

  ngOnInit() {
    this.loadLogs();

    // Debounced filter logic
    this.searchSubject.pipe(
      debounceTime(400),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.onFilter();
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLogs() {
    this.loading.set(true);
    const from = this.fromDate ? new Date(this.fromDate) : undefined;
    const to = this.toDate ? new Date(this.toDate) : undefined;

    this.auditService.getAuditLogs(this.page, this.pageSize, this.entityType, this.eventType, this.userId, from, to).subscribe({
      next: (res) => {
        this.logs.set(res.items);
        this.totalItems = res.totalCount;
        this.totalPages = res.totalPages;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onFilterChange() {
    this.searchSubject.next();
  }

  onFilter() {
    this.page = 1;
    this.loadLogs();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadLogs();
  }

  onPageSizeChange(newSize: number) {
    this.pageSize = newSize;
    this.page = 1;
    this.loadLogs();
  }

  clearFilters() {
    this.entityType = '';
    this.eventType = '';
    this.userId = '';
    this.fromDate = '';
    this.toDate = '';
    this.page = 1;
    this.loadLogs();
  }

  openDetails(log: any) {
    this.selectedLog = log;
    this.showModal = true;
  }

  formatJson(data: any) {
    return JSON.stringify(data, null, 2);
  }
}
