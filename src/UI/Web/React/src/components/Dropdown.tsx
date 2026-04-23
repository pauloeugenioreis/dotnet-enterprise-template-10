import { useState, useRef, useEffect } from 'react';

interface Option {
  label: string;
  value: string;
}

interface DropdownProps {
  options: Option[];
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
  variant?: 'filter' | 'form';
}

export default function Dropdown({ options, value, onChange, placeholder = 'Selecionar...', className = '', variant = 'filter' }: DropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find(opt => opt.value === value);

  const paddingClass = variant === 'filter' ? 'py-5' : 'py-3';
  const roundedClass = variant === 'filter' ? 'rounded-3xl' : 'rounded-2xl';
  const bgClass = variant === 'filter' ? 'bg-white' : 'bg-gray-50';
  const shadowClass = variant === 'filter' ? 'shadow-xl shadow-gray-100/50' : '';

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div ref={dropdownRef} className={`relative ${className}`}>
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className={`w-full pl-8 pr-12 ${paddingClass} ${bgClass} border-none ${roundedClass} ${shadowClass} outline-none font-black text-xs uppercase tracking-widest text-gray-900 flex justify-between items-center hover:bg-gray-50 transition-all active:scale-[0.98]`}
      >
        <span className="truncate">{selectedOption ? selectedOption.label : placeholder}</span>
        <svg 
          className={`w-5 h-5 text-gray-300 transition-transform duration-300 ${isOpen ? 'rotate-180' : ''}`} 
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {isOpen && (
        <div className="absolute z-50 w-full mt-3 bg-white border border-gray-50 rounded-[2rem] shadow-2xl shadow-gray-200/50 py-3 overflow-hidden animate-in fade-in zoom-in-95 duration-200">
          <div className="max-h-60 overflow-y-auto">
            {options.map((option) => (
              <button
                key={option.value}
                type="button"
                onClick={() => {
                  onChange(option.value);
                  setIsOpen(false);
                }}
                className={`w-full text-left px-8 py-4 text-[10px] font-black uppercase tracking-widest transition-colors ${
                  value === option.value 
                  ? 'bg-primary-50 text-primary-600' 
                  : 'text-gray-500 hover:bg-gray-50'
                }`}
              >
                {option.label}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
