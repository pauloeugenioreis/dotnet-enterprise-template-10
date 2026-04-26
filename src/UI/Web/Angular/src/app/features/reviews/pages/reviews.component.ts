import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomerReviewService } from '../../../core/services/customer-review.service';
import { CustomerReviewResponse } from '../../../shared/models';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reviews.component.html'
})
export class ReviewsComponent implements OnInit {
  private reviewService = inject(CustomerReviewService);
  private notification = inject(NotificationService);

  reviews = signal<CustomerReviewResponse[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  page = signal(1);
  pageSize = signal(8);

  // Filters
  productName = signal('');
  minRating = signal<number | undefined>(undefined);
  isApproved = signal<boolean | undefined>(undefined);

  ngOnInit() {
    this.loadReviews();
  }

  loadReviews() {
    this.loading.set(true);
    this.reviewService.getReviews(
      this.page(), 
      this.pageSize(), 
      this.productName(), 
      this.minRating(), 
      this.isApproved()
    ).subscribe({
      next: (res) => {
        this.reviews.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  handleApprove(id: string, approve: boolean) {
    this.reviewService.approve(id, approve).subscribe(() => {
      this.loadReviews();
    });
  }

  handleDelete(id: string) {
    this.notification.confirm('Tem certeza que deseja excluir esta avaliação?').subscribe(confirmed => {
      if (confirmed) {
        this.reviewService.delete(id).subscribe(() => {
          this.notification.success('Avaliação excluída com sucesso');
          this.loadReviews();
        });
      }
    });
  }

  getStars(rating: number): number[] {
    return Array(rating).fill(0);
  }

  getEmptyStars(rating: number): number[] {
    return Array(5 - rating).fill(0);
  }
}
