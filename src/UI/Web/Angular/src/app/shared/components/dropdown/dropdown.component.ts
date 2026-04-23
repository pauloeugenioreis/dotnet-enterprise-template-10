import { Component, Input, Output, EventEmitter, ElementRef, HostListener, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DropdownOption {
  label: string;
  value: any;
}

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dropdown.component.html'
})
export class DropdownComponent {
  @Input() options: DropdownOption[] = [];
  @Input() value: any;
  @Input() placeholder: string = 'Selecionar...';
  @Input() variant: 'filter' | 'form' = 'filter';
  @Input() className: string = '';
  
  @Output() valueChange = new EventEmitter<any>();

  isOpen = signal(false);

  constructor(private elementRef: ElementRef) {}

  get selectedOption() {
    return this.options.find(opt => opt.value === this.value);
  }

  toggle() {
    this.isOpen.update(v => !v);
  }

  select(option: DropdownOption) {
    this.valueChange.emit(option.value);
    this.isOpen.set(false);
  }

  @HostListener('document:mousedown', ['$event'])
  onMouseDown(event: MouseEvent) {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  }

  get paddingClass() { return this.variant === 'filter' ? 'py-5' : 'py-3'; }
  get roundedClass() { return this.variant === 'filter' ? 'rounded-3xl' : 'rounded-2xl'; }
  get bgClass() { return this.variant === 'filter' ? 'bg-white' : 'bg-gray-50'; }
  get shadowClass() { return this.variant === 'filter' ? 'shadow-xl shadow-gray-100/50' : ''; }
}
