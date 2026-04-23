import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex items-center justify-between mt-10 px-10 py-8 bg-white rounded-[3rem] shadow-2xl shadow-gray-200/40 border border-gray-50">
      <div class="flex items-center gap-10">
        <div class="flex items-center gap-2 text-sm font-medium text-gray-500">
          <span>Página</span>
          <span class="font-black text-gray-900 text-lg">{{ page }}</span>
          <span>de</span>
          <span class="font-black text-gray-900 text-lg">{{ totalPages }}</span>
        </div>

        <div class="flex items-center gap-4 border-l border-gray-100 pl-10">
          <span class="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em]">Itens por página:</span>
          <select 
            [ngModel]="pageSize" 
            (ngModelChange)="onPageSizeChange($event)"
            class="bg-gray-50 border-none rounded-2xl px-6 py-3 font-black text-gray-900 text-xs outline-none focus:ring-2 focus:ring-primary-600/10 transition-all cursor-pointer appearance-none"
          >
            <option [value]="5">5</option>
            <option [value]="10">10</option>
            <option [value]="20">20</option>
            <option [value]="50">50</option>
          </select>
        </div>
      </div>

      <div class="flex gap-4">
        <button 
          (click)="onPageChange(page - 1)"
          [disabled]="page === 1"
          class="px-10 py-4 rounded-2xl font-black text-[10px] uppercase tracking-[0.2em] transition-all active:scale-95 disabled:opacity-30
                 bg-white border-2 border-gray-100 text-gray-400 hover:border-primary-100 hover:text-primary-600 disabled:hover:border-gray-100 disabled:hover:text-gray-400 shadow-xl shadow-gray-100/20"
        >
          Anterior
        </button>
        <button 
          (click)="onPageChange(page + 1)"
          [disabled]="page === totalPages || totalPages === 0"
          class="px-10 py-4 rounded-2xl font-black text-[10px] uppercase tracking-[0.2em] transition-all active:scale-95 disabled:opacity-30
                 bg-primary-600 text-white shadow-lg shadow-primary-600/20 hover:bg-primary-700 disabled:bg-gray-200 disabled:text-gray-400 disabled:shadow-none"
        >
          Próxima
        </button>
      </div>
    </div>
  `
})
export class PaginationComponent {
  @Input() page = 1;
  @Input() pageSize = 10;
  @Input() totalPages = 1;
  @Input() totalItems = 0;
  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  onPageChange(newPage: number) {
    if (newPage >= 1 && (this.totalPages === 0 || newPage <= this.totalPages)) {
      this.pageChange.emit(newPage);
    }
  }

  onPageSizeChange(newSize: number) {
    this.pageSizeChange.emit(Number(newSize));
  }
}
