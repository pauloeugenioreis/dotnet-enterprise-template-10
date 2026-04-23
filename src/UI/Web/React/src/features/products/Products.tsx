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
    <div className="p-10 max-w-7xl mx-auto space-y-10 animate-in fade-in duration-500">
      <header className="flex justify-between items-end">
        <div>
          <h1 className="text-5xl font-black text-gray-900 tracking-tighter">Produtos</h1>
          <p className="text-gray-500 mt-3 font-medium text-lg">Catálogo completo de itens.</p>
        </div>
        <div className="flex gap-4">
          <button 
            onClick={handleExport}
            className="p-4 bg-white border border-gray-100 rounded-2xl shadow-sm hover:shadow-md text-gray-400 hover:text-primary-600 transition-all active:scale-95 group"
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
      <div className="flex gap-6 items-center">
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

      <div className="bg-white rounded-[3rem] border border-gray-50 shadow-2xl overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Produto</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Categoria</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Preço</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Estoque</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Status</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Ações</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {isLoading ? (
              <tr><td colSpan={6} className="p-20 text-center"><div className="w-10 h-10 border-4 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
            ) : data?.items.map((product: any) => (
              <tr key={product.id} className="hover:bg-gray-50/50 transition-colors group">
                <td className="px-10 py-8">
                  <div className="flex items-center gap-4">
                    <div className="w-12 h-12 bg-gray-50 rounded-xl flex items-center justify-center text-2xl">
                      📦
                    </div>
                    <span className="font-black text-gray-900 group-hover:text-primary-600 transition-colors">
                      {product.name}
                    </span>
                  </div>
                </td>
                <td className="px-10 py-8 text-gray-500 font-bold">{product.category}</td>
                <td className="px-10 py-8 text-xl font-black text-primary-600">
                  R$ {product.price.toLocaleString()}
                </td>
                <td className="px-10 py-8 font-black text-gray-900">
                  {product.stock} <span className="text-gray-300 font-medium text-xs">unid</span>
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
                      className="p-2 hover:bg-blue-50 text-blue-600 rounded-lg transition-colors" 
                      title="Editar"
                    >
                      ✏️
                    </button>
                    <button 
                      onClick={() => handleDelete(product.id)}
                      className="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors" 
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
