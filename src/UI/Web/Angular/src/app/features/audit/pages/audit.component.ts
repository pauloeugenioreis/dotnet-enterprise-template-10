import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditService } from '../../../core/services/data-services';
import { DomainEvent } from '../../../shared/models/models';

@Component({
  selector: 'app-audit',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './audit.component.html'
})
export class AuditComponent implements OnInit {
  private auditService = inject(AuditService);
  
  logs = signal<DomainEvent[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.auditService.getAuditLogs('Order').subscribe({
      next: (res) => {
        this.logs.set(res.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
