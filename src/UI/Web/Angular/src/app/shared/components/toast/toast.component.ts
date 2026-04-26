import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Toast } from '../../../core/services/notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html'
})
export class ToastComponent {
  notification = inject(NotificationService);

  dismiss(id: number): void {
    this.notification.dismiss(id);
  }

  trackById(_: number, toast: Toast): number {
    return toast.id;
  }

  toastClass(type: Toast['type']): string {
    const map: Record<Toast['type'], string> = {
      success: 'bg-emerald-50 border-emerald-200 text-emerald-800',
      error: 'bg-red-50 border-red-200 text-red-800',
      warning: 'bg-amber-50 border-amber-200 text-amber-800',
      info: 'bg-blue-50 border-blue-200 text-blue-800'
    };
    return map[type];
  }
}
