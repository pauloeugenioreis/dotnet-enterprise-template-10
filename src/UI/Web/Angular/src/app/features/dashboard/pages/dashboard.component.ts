import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../../core/services/data-services';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private orderService = inject(OrderService);
  
  loading = signal(true);

  ngOnInit() {
    // Simulação de carregamento
    setTimeout(() => this.loading.set(false), 800);
  }
}
