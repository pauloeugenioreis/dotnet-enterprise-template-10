import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FormsModule } from '@angular/forms';
import { DocumentService } from '../../../core/services/document.service';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documents.component.html'
})
export class DocumentsComponent {
  private documentService = inject(DocumentService);
  
  fileName = signal('');
  uploading = signal(false);
  processing = signal(false);

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.uploadFile(file);
    }
  }

  uploadFile(file: File) {
    this.uploading.set(true);
    this.documentService.upload(file).subscribe({
      next: (res) => {
        this.fileName.set(res.fileName);
        this.uploading.set(false);
        // Toast success logic could go here
      },
      error: () => this.uploading.set(false)
    });
  }

  downloadFile() {
    if (!this.fileName()) return;
    this.processing.set(true);
    this.documentService.download(this.fileName()).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = this.fileName();
        a.click();
        window.URL.revokeObjectURL(url);
        this.processing.set(false);
      },
      error: () => this.processing.set(false)
    });
  }

  deleteFile() {
    if (!this.fileName()) return;
    this.processing.set(true);
    this.documentService.delete(this.fileName()).subscribe({
      next: () => {
        this.fileName.set('');
        this.processing.set(false);
      },
      error: () => this.processing.set(false)
    });
  }
}
