import { Component, inject, signal, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { ProductService } from '../../../core/services/product.service';
import { FileDownloadService } from '../../../core/services/file-download.service';
import { CreateProductRequest, ProductResponse } from '../../../shared/models';
import { NotificationService } from '../../../core/services/notification.service';
import { ModalComponent } from '../../../shared/components/modal/modal.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, ModalComponent, PaginationComponent],
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
  private productService = inject(ProductService);
  private notification = inject(NotificationService);
  private fileDownload = inject(FileDownloadService);
  private destroyRef = inject(DestroyRef);
  private searchSubject = new Subject<string>();

  @ViewChild('productForm') productForm!: NgForm;

  products = signal<ProductResponse[]>([]);
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
  editingProduct: ProductResponse | null = null;
  formData: CreateProductRequest = {
    name: '',
    category: '',
    price: 0,
    stock: 0,
    isActive: true
  };

  ngOnInit() {
    this.loadProducts();

    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.onSearch();
    });
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

  onStatusFilter(val: boolean | undefined) {
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
        this.fileDownload.download(blob, `produtos-${new Date().getTime()}.xlsx`);
        this.notification.success('Exportação concluída com sucesso');
      }
    });
  }

  openNew() {
    this.editingProduct = null;
    this.modalTitle = 'Novo Produto';
    this.formData = { name: '', category: '', price: 0, stock: 0, isActive: true };
    this.showModal = true;
  }

  openEdit(product: ProductResponse) {
    this.editingProduct = product;
    this.modalTitle = 'Editar Produto';
    this.formData = {
      name: product.name,
      category: product.category,
      price: product.price,
      stock: product.stock,
      isActive: product.isActive
    };
    this.showModal = true;
  }

  save() {
    if (this.productForm) {
      this.productForm.form.markAllAsTouched();
      if (this.productForm.invalid) {
        this.notification.warning('Preencha todos os campos obrigatórios corretamente');
        return;
      }
    }

    const obs = this.editingProduct
      ? this.productService.update(this.editingProduct.id, this.formData)
      : this.productService.create(this.formData);

    obs.subscribe({
      next: () => {
        this.showModal = false;
        this.notification.success(this.editingProduct ? 'Produto atualizado com sucesso' : 'Produto criado com sucesso');
        this.loadProducts();
      }
    });
  }

  deleteProduct(id: string) {
    this.notification.confirm('Deseja excluir este produto?').subscribe(confirmed => {
      if (confirmed) {
        this.productService.delete(id).subscribe({
          next: () => {
            this.notification.success('Produto excluído com sucesso');
            this.loadProducts();
          }
        });
      }
    });
  }
}
