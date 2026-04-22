import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './documents.component.html'
})
export class DocumentsComponent {
  mockDocs = [
    { name: 'Contrato_SLA_Enterprise.pdf', size: '2.4 MB', date: '22/04/2026' },
    { name: 'Relatorio_Faturamento_Q1.pdf', size: '1.8 MB', date: '20/04/2026' },
    { name: 'Manual_Usuario_v2.pdf', size: '4.2 MB', date: '15/04/2026' },
    { name: 'Termos_Privacidade.pdf', size: '0.8 MB', date: '10/04/2026' },
  ];
}
