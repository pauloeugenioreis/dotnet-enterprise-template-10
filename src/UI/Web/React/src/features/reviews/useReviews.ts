import { useState, useEffect } from 'react';
import { customerReviewService } from '../../api/services';

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
      const data = await customerReviewService.getReviews(page, pageSize, productName, minRating, isApproved);
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
    await customerReviewService.approve(id, approve);
    loadReviews();
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Tem certeza que deseja excluir esta avaliação?')) {
      await customerReviewService.delete(id);
      loadReviews();
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
    handleDelete,
    pageSize
  };
}
