import React from 'react';

interface ConfirmModalProps {
  isOpen: boolean;
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmModal({ isOpen, message, onConfirm, onCancel }: ConfirmModalProps) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-in fade-in duration-300">
      <div className="bg-white rounded-[2.5rem] shadow-2xl w-full max-w-sm overflow-hidden p-10 animate-in zoom-in duration-300">
        <div className="flex flex-col items-center text-center">
          {/* Icon */}
          <div className="w-16 h-16 bg-red-50 rounded-2xl flex items-center justify-center mb-6">
            <svg className="w-8 h-8 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
          
          {/* Title */}
          <h3 className="text-xl font-black text-gray-900 mb-8 tracking-tight">{message}</h3>
          
          {/* Actions */}
          <div className="flex gap-4 w-full">
            <button 
              onClick={onCancel}
              className="flex-1 px-6 py-4 rounded-2xl border border-gray-100 text-gray-400 font-black text-xs uppercase tracking-widest hover:bg-gray-50 transition-all active:scale-95"
            >
              Cancelar
            </button>
            <button 
              onClick={onConfirm}
              className="flex-1 px-6 py-4 rounded-2xl bg-red-600 text-white font-black text-xs uppercase tracking-widest shadow-lg shadow-red-600/20 hover:bg-red-700 transition-all active:scale-95"
            >
              Confirmar
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
