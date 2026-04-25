import { useState } from 'react';
import { useProducts } from './useProducts';
import Modal from '../../components/Modal';
import Pagination from '../../components/Pagination';

export default function Products() {
  const {
    data, isLoading, page, setPage, pageSize, setPageSize,
    handleDelete, handleExport, createProduct, updateProduct,
    searchTerm, setSearchTerm, isActive, setIsActive
  } = useProducts();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState({ name: '', category: '', price: '', stock: '', isActive: true });

  const totalPages = data?.totalPages || 1;

  const handleEdit = (product: any) => {
    setEditingId(product.id);
    setFormData({
      name: product.name || '',
      category: product.category || '',
      price: product.price?.toString() || '0',
      stock: product.stock?.toString() || '0',
      isActive: product.isActive ?? true
    });
    setIsModalOpen(true);
  };

  const handleOpenNew = () => {
    setEditingId(null);
    setFormData({ name: '', category: '', price: '', stock: '', isActive: true });
    setIsModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      ...formData,
      price: parseFloat(formData.price),
      stock: parseInt(formData.stock)
    };

    try {
      if (editingId) {
        await updateProduct({ id: editingId, product: payload });
      } else {
        await createProduct(payload);
      }
      setIsModalOpen(false);
      setFormData({ name: '', category: '', price: '', stock: '', isActive: true });
    } catch (error) {
      alert(`Erro ao ${editingId ? 'atualizar' : 'criar'} produto`);
    }
  };

  return (
    <div className="animate-in fade-in duration-700">
      <header className="flex justify-between items-end mb-16">
        <div>
          <h1 className="text-6xl font-black text-gray-900 tracking-tighter">Produtos</h1>
          <p className="text-gray-500 mt-4 font-medium text-xl">Catálogo completo de itens.</p>
        </div>
        <div className="flex gap-4">
          <button
            onClick={handleExport}
            className="p-4 bg-white border border-gray-100 rounded-2xl shadow-sm hover:shadow-md text-gray-400 hover:text-primary-600 transition-all active:scale-95"
            title="Exportar para Excel"
          >
            📊
          </button>
          <button
            onClick={handleOpenNew}
            className="bg-primary-600 hover:bg-primary-700 text-white font-black px-8 py-4 rounded-2xl shadow-lg shadow-primary-600/20 transition-all active:scale-95"
          >
            + Novo Produto
          </button>
        </div>
      </header>

      {/* Filter Bar */}
      <div className="flex gap-6 items-center mb-10">
        <div className="flex-1 relative group">
          <div className="absolute inset-y-0 left-6 flex items-center pointer-events-none">
            <svg className="w-5 h-5 text-gray-400 group-focus-within:text-primary-600 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <input
            type="text"
            placeholder="Buscar por nome ou categoria..."
            className="w-full pl-16 pr-8 py-5 bg-white border-none rounded-3xl shadow-xl shadow-gray-100/50 focus:ring-2 focus:ring-primary-600/20 outline-none transition-all font-bold text-gray-900 placeholder:text-gray-300"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        <div className="flex bg-white p-2 rounded-[2rem] shadow-xl shadow-gray-100/50 gap-1">
          {[
            { label: 'Todos', value: undefined },
            { label: 'Ativos', value: true },
            { label: 'Inativos', value: false }
          ].map((status) => (
            <button
              key={status.label}
              onClick={() => setIsActive(status.value)}
              className={`px-8 py-3 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${
                isActive === status.value
                ? 'bg-gray-900 text-white shadow-lg'
                : 'text-gray-400 hover:bg-gray-50'
              }`}
            >
              {status.label}
            </button>
          ))}
        </div>
      </div>

      <div className="bg-white rounded-[3rem] shadow-2xl shadow-gray-200/40 border border-gray-50 overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead className="bg-primary-600">
            <tr>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Produto</th>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Categoria</th>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Preço</th>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Estoque</th>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Status</th>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Ações</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {isLoading ? (
              <tr><td colSpan={6} className="p-20 text-center"><div className="w-10 h-10 border-4 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
            ) : data?.items.map((product: any) => (
              <tr key={product.id} className="hover:bg-gray-50/50 transition-colors group">
                <td className="px-10 py-8">
                  <div className="flex items-center gap-6">
                    <div className="w-14 h-14 bg-gray-50 rounded-2xl flex items-center justify-center text-2xl group-hover:scale-110 transition-transform">
                      📦
                    </div>
                    <span className="font-black text-gray-900 text-lg tracking-tight">
                      {product.name}
                    </span>
                  </div>
                </td>
                <td className="px-10 py-8">
                  <span className="px-4 py-2 bg-gray-50 rounded-xl text-gray-500 font-bold text-xs uppercase tracking-wider">
                    {product.category}
                  </span>
                </td>
                <td className="px-10 py-8">
                  <span className="font-black text-primary-600 text-lg">R$ {product.price.toLocaleString('pt-BR')}</span>
                </td>
                <td className="px-10 py-8">
                  <div className="flex items-baseline gap-1">
                    <span className="font-black text-gray-900 text-lg">{product.stock}</span>
                    <span className="text-[10px] font-bold text-gray-300 uppercase tracking-widest">unid</span>
                  </div>
                </td>
                <td className="px-10 py-8">
                  <span className={`px-4 py-2 rounded-xl font-black text-[10px] uppercase tracking-widest ${
                    product.isActive
                    ? 'bg-emerald-50 text-emerald-600'
                    : 'bg-red-50 text-red-600'
                  }`}>
                    {product.isActive ? 'Ativo' : 'Inativo'}
                  </span>
                </td>
                <td className="px-10 py-8">
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleEdit(product)}
                      className="w-10 h-10 flex items-center justify-center bg-gray-50 hover:bg-primary-50 rounded-xl text-primary-600 transition-all active:scale-90"
                      title="Editar"
                    >
                      ✏️
                    </button>
                    <button
                      onClick={() => handleDelete(product.id)}
                      className="w-10 h-10 flex items-center justify-center bg-gray-50 hover:bg-red-50 rounded-xl text-red-600 transition-all active:scale-90"
                      title="Excluir"
                    >
                      🗑️
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        <Pagination
          currentPage={page}
          totalPages={totalPages}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
        />
      </div>

      {/* Modal Produto */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingId ? 'Editar Produto' : 'Novo Produto'}
      >
        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label className="block text-xs font-black uppercase tracking-widest text-gray-400 mb-2">Nome do Produto</label>
            <input
              required
              className="w-full px-6 py-4 bg-gray-50 border-none rounded-2xl focus:ring-2 focus:ring-primary-600 outline-none transition-all font-bold text-gray-900"
              value={formData.name}
              onChange={e => setFormData({ ...formData, name: e.target.value })}
              placeholder="Ex: Teclado Mecânico"
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-black uppercase tracking-widest text-gray-400 mb-2">Preço (R$)</label>
              <input
                required
                type="number" step="0.01"
                className="w-full px-6 py-4 bg-gray-50 border-none rounded-2xl focus:ring-2 focus:ring-primary-600 outline-none transition-all font-bold text-gray-900"
                value={formData.price}
                onChange={e => setFormData({ ...formData, price: e.target.value })}
                placeholder="0.00"
              />
            </div>
            <div>
              <label className="block text-xs font-black uppercase tracking-widest text-gray-400 mb-2">Estoque</label>
              <input
                required
                type="number"
                className="w-full px-6 py-4 bg-gray-50 border-none rounded-2xl focus:ring-2 focus:ring-primary-600 outline-none transition-all font-bold text-gray-900"
                value={formData.stock}
                onChange={e => setFormData({ ...formData, stock: e.target.value })}
                placeholder="0"
              />
            </div>
          </div>
          <div>
            <label className="block text-xs font-black uppercase tracking-widest text-gray-400 mb-2">Categoria</label>
            <input
              required
              className="w-full px-6 py-4 bg-gray-50 border-none rounded-2xl focus:ring-2 focus:ring-primary-600 outline-none transition-all font-bold text-gray-900"
              value={formData.category}
              onChange={e => setFormData({ ...formData, category: e.target.value })}
              placeholder="Ex: Periféricos"
            />
          </div>
          <div className="flex items-center gap-3 bg-gray-50 p-4 rounded-2xl">
            <input
              type="checkbox"
              id="isActive"
              className="w-5 h-5 accent-primary-600"
              checked={formData.isActive}
              onChange={e => setFormData({ ...formData, isActive: e.target.checked })}
            />
            <label htmlFor="isActive" className="text-sm font-bold text-gray-700 cursor-pointer">Produto Ativo</label>
          </div>
          <button
            type="submit"
            className="w-full bg-primary-600 hover:bg-primary-700 text-white font-black py-5 rounded-2xl shadow-xl shadow-primary-600/20 transition-all active:scale-95"
          >
            {editingId ? 'Atualizar Produto' : 'Salvar Produto'}
          </button>
        </form>
      </Modal>
    </div>
  );
}
