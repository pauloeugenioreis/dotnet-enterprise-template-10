import { useState } from 'react';
import { useOrders } from './useOrders';
import { useProducts } from '../products/useProducts';
import Modal from '../../components/Modal';

export default function Orders() {
  const { data, isLoading, getStatusColor, page, setPage, handleCancel, handleExport, createOrder } = useOrders();
  const { data: productsData } = useProducts();
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  
  const [formData, setFormData] = useState({
    customerName: '',
    customerEmail: '',
    shippingAddress: '',
    items: [] as any[]
  });

  const handleSubmit = async () => {
    try {
      await createOrder(formData);
      setIsModalOpen(false);
      setFormData({ customerName: '', customerEmail: '', shippingAddress: '', items: [] });
    } catch (error) {
      alert('Erro ao criar pedido. Verifique se todos os campos estão preenchidos.');
    }
  };

  const handleEdit = (order: any) => {
    setEditingId(order.id);
    setFormData({
      customerName: order.customerName || '',
      customerEmail: order.customerEmail || '',
      shippingAddress: order.shippingAddress || '',
      items: order.items || []
    });
    setIsModalOpen(true);
  };

  const totalPages = data?.totalPages || 1;

  const handleAddItem = () => {
    setFormData({
      ...formData,
      items: [...formData.items, { productId: '', quantity: 1, unitPrice: 0 }]
    });
  };

  const handleRemoveItem = (index: number) => {
    setFormData({
      ...formData,
      items: formData.items.filter((_, i) => i !== index)
    });
  };

  const handleItemChange = (index: number, field: string, value: any) => {
    const newItems = [...formData.items];
    if (field === 'productId') {
      const product = productsData?.items.find((p: any) => p.id === parseInt(value));
      newItems[index] = { 
        ...newItems[index], 
        productId: parseInt(value), 
        unitPrice: product?.price || 0 
      };
    } else {
      newItems[index] = { ...newItems[index], [field]: value };
    }
    setFormData({ ...formData, items: newItems });
  };

  const handleOpenNew = () => {
    setEditingId(null);
    setFormData({ customerName: '', customerEmail: '', shippingAddress: '', items: [] });
    setIsModalOpen(true);
  };

  return (
    <div className="p-10 max-w-7xl mx-auto space-y-10 animate-in fade-in duration-500">
      <header className="flex justify-between items-end">
        <div>
          <h1 className="text-5xl font-black text-gray-900 tracking-tighter">Pedidos</h1>
          <p className="text-gray-500 mt-3 font-medium text-lg">Histórico completo de transações.</p>
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
            + Novo Pedido
          </button>
        </div>
      </header>

      <div className="bg-white rounded-[3rem] border border-gray-50 shadow-2xl overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400"># Número</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Cliente</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Status</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Total</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Ações</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {isLoading ? (
              <tr><td colSpan={5} className="p-20 text-center"><div className="w-10 h-10 border-4 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
            ) : data?.items.map((order: any) => (
              <tr key={order.id} className="hover:bg-gray-50/50 transition-colors group">
                <td className="px-10 py-8 font-black text-gray-900 group-hover:text-primary-600 transition-colors">
                  {order.orderNumber}
                </td>
                <td className="px-10 py-8 text-gray-500 font-bold">
                  <div>{order.customerName}</div>
                  <div className="text-[10px] text-gray-300 uppercase tracking-tighter">{order.customerEmail}</div>
                </td>
                <td className="px-10 py-8">
                  <span className={`px-4 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest ${getStatusColor(order.status)}`}>
                    {order.status}
                  </span>
                </td>
                <td className="px-10 py-8 text-xl font-black text-gray-900">
                  R$ {order.total.toLocaleString()}
                </td>
                <td className="px-10 py-8">
                  <div className="flex gap-2">
                    <button 
                      onClick={() => handleEdit(order)}
                      className="p-2 hover:bg-blue-50 text-blue-600 rounded-lg transition-colors" 
                      title="Editar"
                    >
                      ✏️
                    </button>
                    <button 
                      onClick={() => handleCancel(order.id)}
                      className={`p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors ${order.status === 'Cancelled' ? 'opacity-20 cursor-not-allowed' : ''}`} 
                      disabled={order.status === 'Cancelled'}
                      title="Cancelar"
                    >
                      🗑️
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className="bg-gray-50 px-10 py-6 flex justify-between items-center border-t border-gray-100">
          <p className="text-sm text-gray-500 font-bold">
            Página <span className="text-gray-900">{page}</span> de <span className="text-gray-900">{totalPages}</span>
          </p>
          <div className="flex gap-3">
            <button
              onClick={() => setPage(p => Math.max(1, p - 1))}
              disabled={page === 1}
              className="px-6 py-3 bg-white border border-gray-200 rounded-xl font-black text-xs uppercase tracking-widest hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
            >
              Anterior
            </button>
            <button
              onClick={() => setPage(p => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="px-6 py-3 bg-primary-600 text-white rounded-xl font-black text-xs uppercase tracking-widest hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
            >
              Próxima
            </button>
          </div>
        </div>
      </div>

      <Modal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
        title={editingId ? 'Editar Pedido' : 'Novo Pedido'}
      >
        <div className="space-y-6 max-h-[70vh] overflow-y-auto px-2">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-black uppercase tracking-widest text-gray-400 mb-2">Cliente</label>
              <input 
                className="w-full px-6 py-4 bg-gray-50 border-none rounded-2xl outline-none font-bold text-gray-900"
                value={formData.customerName}
                onChange={e => setFormData({ ...formData, customerName: e.target.value })}
                placeholder="Nome"
              />
            </div>
            <div>
              <label className="block text-xs font-black uppercase tracking-widest text-gray-400 mb-2">E-mail</label>
              <input 
                className="w-full px-6 py-4 bg-gray-50 border-none rounded-2xl outline-none font-bold text-gray-900"
                value={formData.customerEmail}
                onChange={e => setFormData({ ...formData, customerEmail: e.target.value })}
                placeholder="email@exemplo.com"
              />
            </div>
          </div>
          
          <div>
            <label className="block text-xs font-black uppercase tracking-widest text-gray-400 mb-2">Endereço de Entrega</label>
            <input 
              className="w-full px-6 py-4 bg-gray-50 border-none rounded-2xl outline-none font-bold text-gray-900"
              value={formData.shippingAddress}
              onChange={e => setFormData({ ...formData, shippingAddress: e.target.value })}
              placeholder="Rua, Número, Cidade - UF"
            />
          </div>

          <div className="border-t border-gray-50 pt-6">
            <div className="flex justify-between items-center mb-4">
              <h3 className="font-black text-gray-900 uppercase tracking-widest text-xs">Itens do Pedido</h3>
              <button 
                onClick={handleAddItem}
                className="text-primary-600 font-black text-xs uppercase tracking-widest hover:underline"
              >
                + Adicionar Item
              </button>
            </div>
            
            <div className="space-y-4">
              {formData.items.map((item, index) => (
                <div key={index} className="flex gap-3 items-end">
                  <div className="flex-1">
                    <select 
                      className="w-full px-4 py-3 bg-gray-50 border-none rounded-xl outline-none font-bold text-sm text-gray-900"
                      value={item.productId}
                      onChange={e => handleItemChange(index, 'productId', e.target.value)}
                    >
                      <option value="">Selecionar Produto</option>
                      {productsData?.items.map((p: any) => (
                        <option key={p.id} value={p.id}>{p.name} - R$ {p.price}</option>
                      ))}
                    </select>
                  </div>
                  <div className="w-20">
                    <input 
                      type="number"
                      className="w-full px-4 py-3 bg-gray-50 border-none rounded-xl outline-none font-bold text-sm text-gray-900"
                      value={item.quantity}
                      onChange={e => handleItemChange(index, 'quantity', parseInt(e.target.value))}
                    />
                  </div>
                  <button 
                    onClick={() => handleRemoveItem(index)}
                    className="p-3 text-red-400 hover:text-red-600"
                  >
                    ✕
                  </button>
                </div>
              ))}
            </div>
          </div>

          <button 
            onClick={handleSubmit}
            disabled={formData.items.length === 0}
            className="w-full bg-primary-600 hover:bg-primary-700 disabled:opacity-50 text-white font-black py-5 rounded-2xl shadow-xl shadow-primary-600/20 transition-all active:scale-95 mt-6"
          >
            Finalizar Pedido
          </button>
        </div>
      </Modal>
    </div>
  );
}
