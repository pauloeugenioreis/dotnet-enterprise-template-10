import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pagination.component.html'
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
