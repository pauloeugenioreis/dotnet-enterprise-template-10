import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../../core/services/data-services';
import { OrderResponseDto } from '../../../shared/models/models';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders.component.html'
})
export class OrdersComponent implements OnInit {
  private orderService = inject(OrderService);
  
  orders = signal<OrderResponseDto[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.orderService.getOrders().subscribe({
      next: (res) => {
        this.orders.set(res.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  getStatusClass(status: string) {
    switch (status.toLowerCase()) {
      case 'delivered': return 'bg-green-50 text-green-600';
      case 'pending': return 'bg-amber-50 text-amber-600';
      case 'cancelled': return 'bg-red-50 text-red-600';
      default: return 'bg-gray-50 text-gray-600';
    }
  }
}
