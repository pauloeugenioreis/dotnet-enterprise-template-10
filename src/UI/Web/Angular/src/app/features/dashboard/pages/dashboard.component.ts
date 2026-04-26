import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';

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
  statistics = signal<any>(null);

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.loading.set(true);
    
    forkJoin({
      orders: this.orderService.getOrders(1, 5),
      stats: this.orderService.getStatistics()
    }).subscribe({
      next: (res) => {
        this.latestOrders.set(res.orders.items);
        this.statistics.set(res.stats);
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
