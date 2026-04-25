import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { OrderResponse } from '../../../../shared/models/models';
import { DropdownComponent } from '../../../../shared/components/dropdown/dropdown.component';
import { OrderService } from '../../../../core/services/data-services';
import { inject } from '@angular/core';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';

@Component({
  selector: 'app-order-details-modal',
  standalone: true,
  imports: [CommonModule, DatePipe, DropdownComponent, DrawerComponent],
  templateUrl: './order-details-modal.component.html'
})
export class OrderDetailsModalComponent {
  private orderService = inject(OrderService);

  @Input() order: OrderResponse | null = null;
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();
  @Output() statusUpdated = new EventEmitter<void>();

  statusOptions = [
    { label: 'Pendente', value: 'Pending' },
    { label: 'Enviado', value: 'Shipped' },
    { label: 'Entregue', value: 'Delivered' },
    { label: 'Cancelado', value: 'Cancelled' }
  ];

  onClose() {
    this.close.emit();
  }

  print() {
    window.print();
  }

  onStatusChange(newStatus: any) {
    if (!this.order || !newStatus) return;
    if (this.order.status === newStatus) return;

    this.orderService.updateStatus(this.order.id, newStatus, 'Atualizado via Dashboard Angular').subscribe({
      next: () => {
        this.statusUpdated.emit();
      }
    });
  }

  getStatusClass(status: string | undefined): string {
    if (!status) return 'bg-gray-50 text-gray-600';
    
    switch (status.toLowerCase()) {
      case 'pending': return 'bg-amber-50 text-amber-600';
      case 'completed': return 'bg-emerald-50 text-emerald-600';
      case 'cancelled': return 'bg-rose-50 text-rose-600';
      case 'shipped': return 'bg-indigo-50 text-indigo-600';
      default: return 'bg-gray-50 text-gray-600';
    }
  }
}
