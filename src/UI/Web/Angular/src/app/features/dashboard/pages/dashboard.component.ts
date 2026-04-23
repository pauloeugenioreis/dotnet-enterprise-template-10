import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/data-services';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private orderService = inject(OrderService);
  
  loading = signal(true);
  latestOrders = signal<any[]>([]);

  ngOnInit() {
    this.loadLatestOrders();
  }

  loadLatestOrders() {
    this.loading.set(true);
    this.orderService.getOrders(1, 5).subscribe({
      next: (res) => {
        this.latestOrders.set(res.items);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        // Fallback mock
        this.latestOrders.set([
          { id: 1, orderNumber: 'ORD-2026-001', customerName: 'Rodrigo Costa', total: 1250.50 },
          { id: 2, orderNumber: 'ORD-2026-002', customerName: 'Amanda Almeida', total: 3420.00 },
          { id: 3, orderNumber: 'ORD-2026-003', customerName: 'Beatriz Oliveira', total: 890.75 }
        ]);
      }
    });
  }
}
