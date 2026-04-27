import { useReviews } from './useReviews';
import { Star, Search, Trash2, CheckCircle, XCircle, ChevronLeft, ChevronRight } from 'lucide-react';
import { useState } from 'react';
import ConfirmModal from '../../components/ConfirmModal';

export default function Reviews() {
  const {
    reviews, loading, totalPages, page, setPage,
    productName, setProductName, minRating, setMinRating,
    isApproved, setIsApproved,
    loadReviews, handleApprove, deleteReview, pageSize
  } = useReviews();

  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [reviewToDelete, setReviewToDelete] = useState<string | null>(null);

  const handleDelete = (id: string) => {
    setReviewToDelete(id);
    setIsDeleteModalOpen(true);
  };

  const confirmDelete = async () => {
    if (reviewToDelete) {
      await deleteReview(reviewToDelete);
      setIsDeleteModalOpen(false);
      setReviewToDelete(null);
    }
  };

  return (
    <div className="p-10 max-w-7xl mx-auto space-y-10 animate-in fade-in duration-700">
      {/* Header */}
      <header className="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
        <div>
          <h1 className="text-6xl font-black text-gray-900 tracking-tighter">Customer Reviews</h1>
          <p className="text-gray-500 mt-4 font-medium text-xl">Gerenciamento e moderação de avaliações (MongoDB).</p>
        </div>
        <div className="flex gap-4">
          <div className="bg-white p-2 rounded-2xl shadow-sm border border-gray-100 flex gap-2">
            <button
              onClick={() => setIsApproved(undefined)}
              className={`px-6 py-2 rounded-xl font-bold transition-all ${isApproved === undefined ? 'bg-gray-900 text-white' : 'text-gray-500 hover:bg-gray-50'}`}
            >
              Todos
            </button>
            <button
              onClick={() => setIsApproved(true)}
              className={`px-6 py-2 rounded-xl font-bold transition-all ${isApproved === true ? 'bg-green-600 text-white' : 'text-gray-500 hover:bg-gray-50'}`}
            >
              Aprovados
            </button>
            <button
              onClick={() => setIsApproved(false)}
              className={`px-6 py-2 rounded-xl font-bold transition-all ${isApproved === false ? 'bg-amber-500 text-white' : 'text-gray-500 hover:bg-gray-50'}`}
            >
              Pendentes
            </button>
          </div>
        </div>
      </header>

      {/* Filters */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="relative group">
          <input
            type="text"
            value={productName}
            onChange={(e) => setProductName(e.target.value)}
            onKeyUp={(e) => e.key === 'Enter' && loadReviews()}
            placeholder="Buscar por produto..."
            className="w-full pl-14 pr-6 py-5 bg-white border-none rounded-3xl shadow-xl shadow-gray-200/50 font-bold text-gray-700 focus:ring-2 focus:ring-primary-500 transition-all outline-none"
          />
          <Search className="absolute left-6 top-1/2 -translate-y-1/2 text-gray-300 group-focus-within:text-primary-500 transition-colors" size={24} />
        </div>

        <div className="relative group">
          <select
            value={minRating || ''}
            onChange={(e) => setMinRating(e.target.value ? Number(e.target.value) : undefined)}
            className="w-full pl-14 pr-6 py-5 bg-white border-none rounded-3xl shadow-xl shadow-gray-200/50 font-bold text-gray-700 focus:ring-2 focus:ring-primary-500 transition-all outline-none appearance-none"
          >
            <option value="">Mínimo de Estrelas</option>
            <option value="1">1+ Estrela</option>
            <option value="2">2+ Estrelas</option>
            <option value="3">3+ Estrelas</option>
            <option value="4">4+ Estrelas</option>
            <option value="5">5 Estrelas</option>
          </select>
          <Star className="absolute left-6 top-1/2 -translate-y-1/2 text-gray-300 group-focus-within:text-primary-500 transition-colors" size={24} />
        </div>
      </div>

      {/* Content */}
      {loading ? (
        <div className="flex flex-col items-center justify-center py-32 space-y-6">
          <div className="w-16 h-16 border-4 border-primary-100 border-t-primary-600 rounded-full animate-spin" />
          <p className="text-gray-400 font-black animate-pulse">CARREGANDO AVALIAÇÕES...</p>
        </div>
      ) : reviews.length === 0 ? (
        <div className="bg-white rounded-[3rem] p-32 text-center shadow-2xl shadow-gray-100 border border-gray-50">
          <div className="text-8xl mb-8">📭</div>
          <h3 className="text-3xl font-black text-gray-900">Nenhuma avaliação encontrada</h3>
          <p className="text-gray-400 mt-4 font-medium">Tente ajustar seus filtros para encontrar o que procura.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8">
          {reviews.map((review) => (
            <div key={review.id} className="bg-white p-8 rounded-[2.5rem] shadow-xl hover:shadow-2xl transition-all border border-gray-50 flex flex-col justify-between group">
              <div>
                <div className="flex justify-between items-start mb-6">
                  <div className="flex gap-0.5">
                    {[...Array(5)].map((_, i) => (
                      <Star
                        key={i}
                        size={18}
                        fill={i < review.rating ? '#fbbf24' : 'transparent'}
                        className={i < review.rating ? 'text-amber-400' : 'text-gray-100'}
                      />
                    ))}
                  </div>
                  {review.isVerifiedPurchase && (
                    <span className="bg-green-50 text-green-600 text-[10px] font-black px-3 py-1 rounded-full uppercase tracking-widest">
                      Verificado
                    </span>
                  )}
                </div>

                <h4 className="text-xl font-black text-gray-900 mb-2 leading-tight group-hover:text-primary-600 transition-colors">{review.title}</h4>
                <p className="text-gray-500 text-sm font-medium line-clamp-3 mb-6">{review.comment}</p>
              </div>

              <div className="space-y-6">
                <div className="flex items-center gap-4 py-4 border-t border-gray-50">
                  <div className="w-10 h-10 bg-primary-50 rounded-xl flex items-center justify-center text-primary-600 font-black text-xs uppercase">
                    {review.customerName[0]}
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm font-black text-gray-900 truncate tracking-tight">{review.customerName}</p>
                    <p className="text-[10px] text-gray-400 font-bold uppercase truncate tracking-widest">{review.productName}</p>
                  </div>
                </div>

                <div className="flex gap-3">
                  {!review.isApproved ? (
                    <button
                      onClick={() => handleApprove(review.id, true)}
                      className="flex-1 py-3 bg-green-50 text-green-600 rounded-xl font-black text-[10px] uppercase tracking-widest hover:bg-green-600 hover:text-white transition-all shadow-sm flex items-center justify-center gap-2"
                    >
                      <CheckCircle size={14} /> Aprovar
                    </button>
                  ) : (
                    <button
                      onClick={() => handleApprove(review.id, false)}
                      className="flex-1 py-3 bg-amber-50 text-amber-600 rounded-xl font-black text-[10px] uppercase tracking-widest hover:bg-amber-500 hover:text-white transition-all shadow-sm flex items-center justify-center gap-2"
                    >
                      <XCircle size={14} /> Rejeitar
                    </button>
                  )}
                  <button
                    onClick={() => handleDelete(review.id)}
                    className="p-3 bg-rose-50 text-rose-600 rounded-xl font-black hover:bg-rose-600 hover:text-white transition-all shadow-sm"
                  >
                    <Trash2 size={18} />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {!loading && totalPages > 1 && (
        <footer className="flex justify-center pt-10">
          <div className="bg-white p-2 rounded-2xl shadow-xl border border-gray-50 flex gap-2">
            <button
              disabled={page === 1}
              onClick={() => setPage(page - 1)}
              className="px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest disabled:opacity-30 enabled:hover:bg-gray-50 transition-all flex items-center gap-2"
            >
              <ChevronLeft size={16} /> Anterior
            </button>
            <div className="px-6 py-3 flex items-center font-black text-primary-600 text-xs uppercase tracking-tighter">
              Página {page} de {totalPages}
            </div>
            <button
              disabled={page >= totalPages}
              onClick={() => setPage(page + 1)}
              className="px-6 py-3 rounded-xl font-black text-xs uppercase tracking-widest disabled:opacity-30 enabled:hover:bg-gray-50 transition-all flex items-center gap-2"
            >
              Próxima <ChevronRight size={16} />
            </button>
          </div>
        </footer>
      )}

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        message="Deseja excluir esta avaliação?"
        onConfirm={confirmDelete}
        onCancel={() => setIsDeleteModalOpen(false)}
      />
    </div>
  );
}
