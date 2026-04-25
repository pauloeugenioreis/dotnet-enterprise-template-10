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
  variant?: 'filter' | 'form' | 'primary';
  labelOverride?: string;
  direction?: 'up' | 'down';
  onLoadMore?: () => void;
  loading?: boolean;
}

export default function Dropdown({ 
  options, 
  value, 
  onChange, 
  placeholder = 'Selecionar...', 
  className = '', 
  variant = 'filter',
  labelOverride,
  direction = 'down',
  onLoadMore,
  loading = false
}: DropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const selectedOption = options.find(opt => opt.value === value);

  const handleScroll = (e: React.UIEvent<HTMLDivElement>) => {
    const target = e.currentTarget;
    if (target.scrollTop + target.clientHeight >= target.scrollHeight - 10) {
      onLoadMore?.();
    }
  };

  const paddingClass = (variant === 'filter' || variant === 'primary') ? 'py-4' : 'py-3';
  const roundedClass = variant === 'filter' ? 'rounded-3xl' : 'rounded-2xl';
  const bgClass = variant === 'primary' ? 'bg-primary-600' : (variant === 'filter' ? 'bg-white' : 'bg-gray-50');
  const textClass = variant === 'primary' ? 'text-white' : 'text-gray-900';
  const hoverClass = variant === 'primary' ? 'hover:bg-primary-700' : 'hover:bg-gray-50';
  const shadowClass = variant === 'primary' ? 'shadow-xl shadow-primary-200' : (variant === 'filter' ? 'shadow-xl shadow-gray-100/50' : '');
  const iconColorClass = variant === 'primary' ? 'text-white/80' : 'text-gray-300';

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
        className={`w-full pl-8 pr-12 ${paddingClass} ${bgClass} ${textClass} border-none ${roundedClass} ${shadowClass} outline-none font-black text-xs uppercase tracking-widest flex justify-between items-center ${hoverClass} transition-all active:scale-[0.98]`}
      >
        <span className="truncate">{labelOverride ?? (selectedOption ? selectedOption.label : placeholder)}</span>
        <svg 
          className={`w-5 h-5 ${iconColorClass} transition-transform duration-300 ${isOpen ? 'rotate-180' : ''}`} 
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {isOpen && (
        <div className={`absolute z-50 w-full ${direction === 'up' ? 'bottom-full mb-3' : 'mt-3'} bg-white border border-gray-50 rounded-[2rem] shadow-2xl shadow-gray-200/50 py-3 overflow-hidden animate-in fade-in zoom-in-95 duration-200`}>
          <div className="max-h-60 overflow-y-auto" onScroll={handleScroll}>
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
            
            {loading && (
              <div className="px-8 py-4 flex justify-center">
                <div className="w-4 h-4 border-2 border-primary-600 border-t-transparent rounded-full animate-spin"></div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
