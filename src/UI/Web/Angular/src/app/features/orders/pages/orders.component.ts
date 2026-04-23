import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderService, ProductService } from '../../../core/services/data-services';
import { OrderResponseDto, ProductResponseDto } from '../../../shared/models/models';
import { DropdownComponent } from '../../../shared/components/dropdown/dropdown.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ModalComponent } from '../../../shared/components/modal/modal.component';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownComponent, PaginationComponent, ModalComponent],
  templateUrl: './orders.component.html'
})
export class OrdersComponent implements OnInit, OnDestroy {
  private orderService = inject(OrderService);
  private productService = inject(ProductService);
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();
  
  orders = signal<OrderResponseDto[]>([]);
  products = signal<ProductResponseDto[]>([]);
  loading = signal(true);
  
  // Filters
  searchTerm = '';
  status = '';
  fromDate: string = '';
  toDate: string = '';
  
  // Pagination
  page = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Modal
  showModal = false;
  modalTitle = 'Novo Pedido';
  editingOrder: any = null;
  formData = {
    customerName: '',
    customerEmail: '',
    shippingAddress: '',
    items: [] as any[]
  };

  statusOptions = [
    { label: 'Todos os Status', value: '' },
    { label: 'Pendente', value: 'Pending' },
    { label: 'Enviado', value: 'Shipped' },
    { label: 'Entregue', value: 'Delivered' },
    { label: 'Cancelado', value: 'Cancelled' }
  ];

  ngOnInit() {
    this.loadOrders();
    this.loadProducts();

    // Debounced search logic
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.onFilter();
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrders() {
    this.loading.set(true);
    const from = this.fromDate ? new Date(this.fromDate) : undefined;
    const to = this.toDate ? new Date(this.toDate) : undefined;

    this.orderService.getOrders(this.page, this.pageSize, this.searchTerm, this.status, from, to).subscribe({
      next: (res) => {
        this.orders.set(res.items);
        this.totalItems = res.totalCount;
        this.totalPages = res.totalPages;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadProducts() {
    this.productService.getProducts(1, 100).subscribe({
      next: (res) => this.products.set(res.items)
    });
  }

  onSearchChange(value: string) {
    this.searchTerm = value;
    this.searchSubject.next(value);
  }

  onFilter() {
    this.page = 1;
    this.loadOrders();
  }

  clearFilters() {
    this.searchTerm = '';
    this.status = '';
    this.fromDate = '';
    this.toDate = '';
    this.page = 1;
    this.loadOrders();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadOrders();
  }

  onPageSizeChange(newSize: number) {
    this.pageSize = newSize;
    this.page = 1;
    this.loadOrders();
  }

  exportExcel() {
    const from = this.fromDate ? new Date(this.fromDate) : undefined;
    const to = this.toDate ? new Date(this.toDate) : undefined;
    
    this.orderService.exportToExcel(this.searchTerm, this.status, from, to).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `pedidos-${new Date().getTime()}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => alert('Erro ao exportar pedidos')
    });
  }

  openNew() {
    this.editingOrder = null;
    this.modalTitle = 'Novo Pedido';
    this.formData = { customerName: '', customerEmail: '', shippingAddress: '', items: [] };
    this.showModal = true;
  }

  viewDetails(order: any) {
    this.editingOrder = order;
    this.modalTitle = 'Detalhes do Pedido';
    this.formData = { 
      customerName: order.customerName || '',
      customerEmail: order.customerEmail || '',
      shippingAddress: order.shippingAddress || '',
      items: order.items || []
    };
    this.showModal = true;
  }

  addItem() {
    this.formData.items.push({ productId: '', quantity: 1, unitPrice: 0 });
  }

  removeItem(index: number) {
    this.formData.items.splice(index, 1);
  }

  onProductChange(index: number, productId: any) {
    const product = this.products().find(p => p.id.toString() === productId.toString());
    if (product) {
      this.formData.items[index].productId = product.id;
      this.formData.items[index].unitPrice = product.price;
    }
  }

  save() {
    if (this.formData.items.length === 0) {
      alert('Adicione pelo menos um item ao pedido');
      return;
    }

    const obs = this.editingOrder 
      ? this.orderService.updateOrder(this.editingOrder.id, this.formData)
      : this.orderService.createOrder(this.formData);

    obs.subscribe({
      next: () => {
        this.showModal = false;
        this.loadOrders();
      },
    });
  }

  getStatusClass(status: string) {
    switch (status.toLowerCase()) {
      case 'delivered': case 'entregue': return 'bg-emerald-50 text-emerald-600';
      case 'pending': case 'pendente': return 'bg-amber-50 text-amber-600';
      case 'shipped': case 'enviado': return 'bg-blue-50 text-blue-600';
      case 'cancelled': case 'cancelado': return 'bg-red-50 text-red-600';
      default: return 'bg-gray-50 text-gray-600';
    }
  }

  get productOptions() {
    return this.products().map(p => ({
      label: `${p.name} - R$ ${p.price}`,
      value: p.id.toString()
    }));
  }
}
