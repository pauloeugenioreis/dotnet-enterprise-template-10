import { Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
}

export interface ConfirmState {
  message: string;
  resolve: (confirmed: boolean) => void;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 0;

  toasts = signal<Toast[]>([]);
  confirmState = signal<ConfirmState | null>(null);

  success(message: string): void { this.show('success', message); }
  error(message: string): void { this.show('error', message); }
  warning(message: string): void { this.show('warning', message); }
  info(message: string): void { this.show('info', message); }

  dismiss(id: number): void {
    this.toasts.update(ts => ts.filter(t => t.id !== id));
  }

  confirm(message: string): Observable<boolean> {
    return new Observable(observer => {
      this.confirmState.set({
        message,
        resolve: (confirmed: boolean) => {
          this.confirmState.set(null);
          observer.next(confirmed);
          observer.complete();
        }
      });
    });
  }

  private show(type: ToastType, message: string): void {
    const id = ++this.nextId;
    this.toasts.update(ts => [...ts, { id, type, message }]);
    setTimeout(() => this.dismiss(id), 4500);
  }
}
