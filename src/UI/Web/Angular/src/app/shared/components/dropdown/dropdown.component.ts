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
  @Input() variant: 'filter' | 'form' | 'primary' = 'filter';
  @Input() className: string = '';
  @Input() labelOverride: string | null = null;
  @Input() direction: 'up' | 'down' = 'down';
  
  @Input() loading: boolean = false;
  
  @Output() valueChange = new EventEmitter<any>();
  @Output() loadMore = new EventEmitter<void>();

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

  handleScroll(event: Event) {
    const target = event.target as HTMLElement;
    if (target.scrollTop + target.clientHeight >= target.scrollHeight - 10) {
      this.loadMore.emit();
    }
  }

  @HostListener('document:mousedown', ['$event'])
  onMouseDown(event: MouseEvent) {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  }

  get paddingClass() { return (this.variant === 'filter' || this.variant === 'primary') ? 'py-4' : 'py-3'; }
  get roundedClass() { return this.variant === 'filter' ? 'rounded-3xl' : 'rounded-2xl'; }
  get bgClass() { return this.variant === 'primary' ? 'bg-primary-600' : (this.variant === 'filter' ? 'bg-white' : 'bg-gray-50'); }
  get textClass() { return this.variant === 'primary' ? 'text-white' : 'text-gray-900'; }
  get hoverClass() { return this.variant === 'primary' ? 'hover:bg-primary-700' : 'hover:bg-gray-50'; }
  get shadowClass() { return this.variant === 'primary' ? 'shadow-xl shadow-primary-200' : (this.variant === 'filter' ? 'shadow-xl shadow-gray-100/50' : ''); }
  get iconClass() { return this.variant === 'primary' ? 'text-white/80' : 'text-gray-300'; }
}
