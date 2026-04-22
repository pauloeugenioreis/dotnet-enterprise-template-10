import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../../core/services/data-services';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
  private productService = inject(ProductService);
  
  products = signal<any[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.productService.getProducts().subscribe({
      next: (res) => {
        this.products.set(res.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
