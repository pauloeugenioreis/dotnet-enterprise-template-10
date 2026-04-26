import { Component, inject, signal, computed, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { OrderService } from '../../../core/services/order.service';
import { ProductService } from '../../../core/services/product.service';
import { FileDownloadService } from '../../../core/services/file-download.service';
import { CreateOrderRequest, OrderItemRequest, OrderResponse, ProductResponse, OrderStatus } from '../../../shared/models';
import { NotificationService } from '../../../core/services/notification.service';
import { DropdownComponent } from '../../../shared/components/dropdown/dropdown.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { DrawerComponent } from '../../../shared/components/drawer/drawer.component';
import { OrderDetailsModalComponent } from '../components/order-details-modal/order-details-modal.component';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownComponent, PaginationComponent, DrawerComponent, OrderDetailsModalComponent],
  templateUrl: './orders.component.html'
})
export class OrdersComponent implements OnInit {
  private orderService = inject(OrderService);
  private productService = inject(ProductService);
  private notification = inject(NotificationService);
  private fileDownload = inject(FileDownloadService);
  private destroyRef = inject(DestroyRef);
  private searchSubject = new Subject<string>();

  @ViewChild('orderForm') orderForm!: NgForm;

  orders = signal<OrderResponse[]>([]);
  products = signal<ProductResponse[]>([]);
  loading = signal(true);

  // Product Infinite Scroll
  productPage = signal(1);
  productLoading = signal(false);
  hasMoreProducts = signal(true);

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

  // Modal State
  showModal = false;
  showDetailsModal = signal(false);
  selectedOrder = signal<OrderResponse | null>(null);
  modalTitle = 'Novo Pedido';
  editingOrder: OrderResponse | null = null;
  formData: CreateOrderRequest = {
    customerName: '',
    customerEmail: '',
    shippingAddress: '',
    items: [] as OrderItemRequest[]
  };

  readonly OrderStatus = OrderStatus;

  statusOptions = [
    { label: 'Todos os Status', value: '' },
    { label: 'Pendente', value: OrderStatus.Pending },
    { label: 'Enviado', value: OrderStatus.Shipped },
    { label: 'Entregue', value: OrderStatus.Delivered },
    { label: 'Cancelado', value: OrderStatus.Cancelled }
  ];

  ngOnInit() {
    this.loadOrders();
    this.loadProducts();

    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.onFilter();
    });
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

  loadProducts(isLoadMore = false) {
    if (this.productLoading() || (!this.hasMoreProducts() && isLoadMore)) return;

    this.productLoading.set(true);
    this.productService.getProducts(this.productPage(), 20, '', true).subscribe({
      next: (res) => {
        const newItems = res.items || [];
        if (isLoadMore) {
          this.products.set([...this.products(), ...newItems]);
        } else {
          this.products.set(newItems);
        }
        this.hasMoreProducts.set(newItems.length === 20);
        if (this.hasMoreProducts()) {
          this.productPage.update(p => p + 1);
        }
        this.productLoading.set(false);
      },
      error: () => this.productLoading.set(false)
    });
  }

  loadMoreProducts() {
    this.loadProducts(true);
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
        this.fileDownload.download(blob, `pedidos-${new Date().getTime()}.xlsx`);
        this.notification.success('Exportação concluída com sucesso');
      }
    });
  }

  openNew() {
    this.editingOrder = null;
    this.modalTitle = 'Novo Pedido';
    this.formData = { customerName: '', customerEmail: '', shippingAddress: '', items: [] as OrderItemRequest[] };
    this.showModal = true;
  }

  viewDetails(order: OrderResponse) {
    this.selectedOrder.set(order);
    this.showDetailsModal.set(true);
  }

  openEdit(order: OrderResponse) {
    this.editingOrder = order;
    this.modalTitle = 'Editar Pedido';
    this.formData = {
      customerName: order.customerName,
      customerEmail: order.customerEmail || '',
      shippingAddress: order.shippingAddress || '',
      items: (order.items || []).map(item => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice
      }))
    };
    this.showModal = true;
  }

  cancelOrder(order: OrderResponse) {
    if (order.status === OrderStatus.Cancelled) return;
    this.notification.confirm('Tem certeza que deseja cancelar este pedido?').subscribe(confirmed => {
      if (confirmed) {
        this.orderService.updateStatus(order.id, OrderStatus.Cancelled).subscribe({
          next: () => {
            this.notification.success('Pedido cancelado com sucesso');
            this.loadOrders();
          }
        });
      }
    });
  }

  addItem() {
    this.formData.items.push({ productId: '', quantity: 1, unitPrice: 0 } as OrderItemRequest);
  }

  removeItem(index: number) {
    this.formData.items.splice(index, 1);
  }

  onProductChange(index: number, productId: string) {
    const product = this.products().find(p => p.id === productId);
    if (product) {
      this.formData.items[index].productId = product.id;
      this.formData.items[index].unitPrice = product.price;
      this.formData.items = [...this.formData.items];
    }
  }

  save() {
    if (this.orderForm) {
      this.orderForm.form.markAllAsTouched();
      if (this.orderForm.invalid) {
        this.notification.warning('Preencha todos os campos obrigatórios');
        return;
      }
    }

    if (this.formData.items.length === 0) {
      this.notification.warning('Adicione pelo menos um item ao pedido');
      return;
    }

    const obs = this.editingOrder
      ? this.orderService.update(this.editingOrder.id, this.formData)
      : this.orderService.create(this.formData);

    obs.subscribe({
      next: () => {
        this.showModal = false;
        this.notification.success(this.editingOrder ? 'Pedido atualizado com sucesso' : 'Pedido criado com sucesso');
        this.loadOrders();
      }
    });
  }

  getStatusClass(status: string) {
    switch (status) {
      case OrderStatus.Delivered: return 'bg-emerald-50 text-emerald-600';
      case OrderStatus.Pending: return 'bg-amber-50 text-amber-600';
      case OrderStatus.Shipped: return 'bg-blue-50 text-blue-600';
      case OrderStatus.Cancelled: return 'bg-red-50 text-red-600';
      default: return 'bg-gray-50 text-gray-600';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case OrderStatus.Pending: return 'Pendente';
      case OrderStatus.Shipped: return 'Enviado';
      case OrderStatus.Delivered: return 'Entregue';
      case OrderStatus.Cancelled: return 'Cancelado';
      default: return status;
    }
  }

  productOptions = computed(() =>
    this.products().map(p => ({
      label: `${p.name} - R$ ${p.price}`,
      value: p.id.toString()
    }))
  );
}
