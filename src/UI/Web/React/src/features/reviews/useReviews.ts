import { useState, useEffect } from 'react';
import { customerReviewService } from '../../api/services/customer-review.service';
import { notify } from '../../utils/toast';

export function useReviews() {
  const [reviews, setReviews] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const pageSize = 8;

  // Filters
  const [productName, setProductName] = useState('');
  const [minRating, setMinRating] = useState<number | undefined>(undefined);
  const [isApproved, setIsApproved] = useState<boolean | undefined>(undefined);

  const loadReviews = async () => {
    setLoading(true);
    try {
      const data = await customerReviewService.list(page, pageSize, { productName, minRating, isApproved });
      setReviews(data.items);
      setTotalCount(data.totalCount);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadReviews();
  }, [page, isApproved, minRating]);

  const handleApprove = async (id: string, approve: boolean) => {
    try {
      await customerReviewService.approve(id, approve);
      notify.success('Avaliação Atualizada', approve ? 'A avaliação foi aprovada.' : 'A aprovação foi removida.');
      loadReviews();
    } catch (error) {
      notify.error('Erro na Operação', 'Não foi possível atualizar a avaliação.');
    }
  };

  const deleteReview = async (id: string) => {
    try {
      await customerReviewService.delete(id);
      notify.success('Avaliação Excluída', 'A avaliação foi removida permanentemente.');
      loadReviews();
    } catch (error) {
      notify.error('Erro na Operação', 'Não foi possível excluir a avaliação.');
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return {
    reviews,
    loading,
    totalCount,
    totalPages,
    page,
    setPage,
    productName,
    setProductName,
    minRating,
    setMinRating,
    isApproved,
    setIsApproved,
    loadReviews,
    handleApprove,
    deleteReview,
    pageSize
  };
}
