import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/data-services';
import { ProductResponseDto } from '../../../shared/models/models';
import { ModalComponent } from '../../../shared/components/modal/modal.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, ModalComponent, PaginationComponent],
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit, OnDestroy {
  private productService = inject(ProductService);
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();
  
  products = signal<ProductResponseDto[]>([]);
  loading = signal(true);
  
  // Filters
  searchTerm = '';
  isActive: boolean | undefined = undefined;
  
  // Pagination
  page = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Modal
  showModal = false;
  modalTitle = 'Novo Produto';
  editingProduct: any = null;
  formData = {
    name: '',
    category: '',
    price: 0,
    stock: 0,
    isActive: true
  };

  ngOnInit() {
    this.loadProducts();

    // Debounced search logic
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.onSearch();
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProducts() {
    this.loading.set(true);
    this.productService.getProducts(this.page, this.pageSize, this.searchTerm, this.isActive).subscribe({
      next: (res) => {
        this.products.set(res.items);
        this.totalItems = res.totalCount;
        this.totalPages = res.totalPages;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearchChange(value: string) {
    this.searchTerm = value;
    this.searchSubject.next(value);
  }

  onSearch() {
    this.page = 1;
    this.loadProducts();
  }

  onStatusFilter(val: any) {
    this.isActive = val;
    this.page = 1;
    this.loadProducts();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadProducts();
  }

  onPageSizeChange(newSize: number) {
    this.pageSize = newSize;
    this.page = 1;
    this.loadProducts();
  }

  exportExcel() {
    this.productService.exportToExcel(this.searchTerm, this.isActive).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `produtos-${new Date().getTime()}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => alert('Erro ao exportar produtos')
    });
  }

  openNew() {
    this.editingProduct = null;
    this.modalTitle = 'Novo Produto';
    this.formData = { name: '', category: '', price: 0, stock: 0, isActive: true };
    this.showModal = true;
  }

  openEdit(product: ProductResponseDto) {
    this.editingProduct = product;
    this.modalTitle = 'Editar Produto';
    this.formData = { ...product };
    this.showModal = true;
  }

  save() {
    const obs = this.editingProduct 
      ? this.productService.updateProduct(this.editingProduct.id, this.formData)
      : this.productService.createProduct(this.formData);

    obs.subscribe({
      next: () => {
        this.showModal = false;
        this.loadProducts();
      },
      error: (err) => alert('Erro ao salvar produto: ' + (err.error?.message || 'Erro desconhecido'))
    });
  }

  deleteProduct(id: string) {
    if (confirm('Deseja excluir este produto?')) {
      this.productService.deleteProduct(id).subscribe({
        next: () => this.loadProducts(),
        error: () => alert('Erro ao excluir produto')
      });
    }
  }
}
