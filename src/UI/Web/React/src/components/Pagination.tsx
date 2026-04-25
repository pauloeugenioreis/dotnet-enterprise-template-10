import React from 'react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}

const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  pageSize,
  onPageChange,
  onPageSizeChange,
}) => {
  return (
    <div className="bg-gray-50 px-10 py-6 flex justify-between items-center border-t border-gray-100">
      <div className="flex flex-col sm:flex-row sm:items-center gap-4 sm:gap-10">
        <p className="text-sm text-gray-500 font-bold">
          Página <span className="text-gray-900">{currentPage}</span> de <span className="text-gray-900">{totalPages || 1}</span>
        </p>
        
        <div className="flex items-center gap-3">
          <span className="text-[10px] font-black uppercase tracking-widest text-gray-400">Itens por página:</span>
          <select 
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className="bg-white border border-gray-200 rounded-xl px-4 py-2 text-xs font-black text-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-600 transition-all cursor-pointer shadow-sm"
          >
            {[5, 10, 20, 50, 100].map(size => (
              <option key={size} value={size}>{size}</option>
            ))}
          </select>
        </div>
      </div>

      <div className="flex gap-3">
        <button
          onClick={() => onPageChange(Math.max(1, currentPage - 1))}
          disabled={currentPage === 1}
          className="px-6 py-3 bg-white border border-gray-200 rounded-xl font-black text-xs uppercase tracking-widest hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-sm active:scale-95"
        >
          Anterior
        </button>
        <button
          onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
          disabled={currentPage === totalPages || totalPages === 0}
          className="px-6 py-3 bg-primary-600 text-white rounded-xl font-black text-xs uppercase tracking-widest hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-lg shadow-primary-600/20 active:scale-95"
        >
          Próxima
        </button>
      </div>
    </div>
  );
};

export default Pagination;
