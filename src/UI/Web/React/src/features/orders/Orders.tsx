import { useState } from 'react';
import { useOrders } from './useOrders';
import { useProducts } from '../products/useProducts';
import Modal from '../../components/Modal';
import Pagination from '../../components/Pagination';
import Dropdown from '../../components/Dropdown';
import OrderDetailsModal from './components/OrderDetailsModal';
import { OrderResponseDto } from '../../api/services';

export default function Orders() {
  const {
    data, isLoading, getStatusColor, page, setPage, pageSize, setPageSize,
    handleCancel, handleExport, createOrder,
    status, setStatus, searchTerm, setSearchTerm, startDate, setStartDate, endDate, setEndDate
  } = useOrders();
  const { data: productsData } = useProducts();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<OrderResponseDto | null>(null);
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

  const handleViewDetails = (order: OrderResponseDto) => {
    setSelectedOrder(order);
    setIsDetailsOpen(true);
  };

  return (
    <div className="animate-in fade-in duration-700">
      <header className="flex justify-between items-end mb-16">
        <div>
          <h1 className="text-6xl font-black text-gray-900 tracking-tighter">Pedidos</h1>
          <p className="text-gray-500 mt-4 font-medium text-xl">Histórico completo de transações.</p>
        </div>
        <div className="flex gap-4">
          <button
            onClick={handleExport}
            className="p-5 bg-white border border-gray-100 rounded-[2rem] shadow-xl shadow-gray-200/50 text-gray-300 hover:text-primary-600 transition-all hover:scale-105 active:scale-95 group"
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

      {/* Filter Bar */}
      <div className="grid grid-cols-12 gap-6 items-center mb-10">
        <div className="col-span-3 relative group">
          <div className="absolute inset-y-0 left-6 flex items-center pointer-events-none">
            <svg className="w-5 h-5 text-gray-400 group-focus-within:text-primary-600 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <input
            type="text"
            placeholder="Buscar pedido..."
            className="w-full pl-16 pr-8 py-5 bg-white border-none rounded-3xl shadow-xl shadow-gray-100/50 focus:ring-2 focus:ring-primary-600/20 outline-none transition-all font-bold text-gray-900 placeholder:text-gray-300"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        <div className="col-span-4 flex bg-white p-2 rounded-3xl shadow-xl shadow-gray-100/50 gap-2 items-center">
          <div className="flex-1 px-4 py-1">
            <label className="block text-[8px] font-black uppercase text-gray-400 leading-none mb-1">De</label>
            <input
              type="date"
              className="w-full bg-transparent border-none outline-none font-bold text-xs text-gray-900 cursor-pointer"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
            />
          </div>
          <div className="w-px h-8 bg-gray-100"></div>
          <div className="flex-1 px-4 py-1">
            <label className="block text-[8px] font-black uppercase text-gray-400 leading-none mb-1">Até</label>
            <input
              type="date"
              className="w-full bg-transparent border-none outline-none font-bold text-xs text-gray-900 cursor-pointer"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
            />
          </div>
        </div>

        <div className="col-span-3">
          <Dropdown
            value={status}
            onChange={setStatus}
            options={[
              { label: 'Todos os Status', value: '' },
              { label: 'Pendente', value: 'Pending' },
              { label: 'Enviado', value: 'Shipped' },
              { label: 'Entregue', value: 'Delivered' },
              { label: 'Cancelado', value: 'Cancelled' }
            ]}
          />
        </div>

        <div className="col-span-2 flex justify-end">
          <button
            onClick={() => {
              setSearchTerm('');
              setStatus('');
              setStartDate('');
              setEndDate('');
            }}
            className="px-8 py-5 text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 hover:text-red-500 transition-colors flex items-center gap-2 group"
          >
            <span className="group-hover:rotate-90 transition-transform duration-300">✕</span>
            Limpar
          </button>
        </div>
      </div>

      <div className="bg-white rounded-[3rem] shadow-2xl shadow-gray-200/40 border border-gray-50 overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead className="bg-primary-600">
            <tr>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]"># Número</th>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Cliente</th>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Total</th>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Status</th>
              <th className="px-10 py-8 text-[10px] font-black text-white uppercase tracking-[0.2em]">Ações</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {isLoading ? (
              <tr><td colSpan={5} className="p-20 text-center"><div className="w-10 h-10 border-4 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
            ) : data?.items.map((order: any) => (
              <tr key={order.id} className="hover:bg-gray-50/50 transition-colors group">
                <td className="px-10 py-8">
                  <span className="font-black text-gray-900 tracking-tight">{order.orderNumber}</span>
                </td>
                <td className="px-10 py-8 text-gray-500 font-bold">
                  <div>{order.customerName}</div>
                  <div className="text-[10px] text-gray-400 uppercase tracking-tighter">{order.customerEmail}</div>
                </td>
                <td className="px-10 py-8">
                  <span className="font-black text-primary-600 text-lg">R$ {order.total.toLocaleString('pt-BR')}</span>
                </td>
                <td className="px-10 py-8">
                  <span className={`px-4 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest ${getStatusColor(order.status)}`}>
                    {order.status}
                  </span>
                </td>
                <td className="px-10 py-8">
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleViewDetails(order)}
                      className="w-10 h-10 flex items-center justify-center bg-gray-50 hover:bg-blue-50 rounded-xl text-blue-600 transition-all active:scale-90"
                      title="Ver Detalhes"
                    >
                      🔍
                    </button>
                    <button
                      onClick={() => handleEdit(order)}
                      className="w-10 h-10 flex items-center justify-center bg-gray-50 hover:bg-primary-50 rounded-xl text-primary-600 transition-all active:scale-90"
                      title="Editar"
                    >
                      ✏️
                    </button>
                    <button
                      onClick={() => handleCancel(order.id)}
                      className={`w-10 h-10 flex items-center justify-center bg-gray-50 hover:bg-rose-50 rounded-xl text-rose-600 transition-all active:scale-90 ${order.status === 'Cancelled' ? 'opacity-20 cursor-not-allowed' : ''}`}
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

        <Pagination
          currentPage={page}
          totalPages={totalPages}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
        />
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
                    <Dropdown
                      variant="form"
                      className="w-full"
                      value={item.productId?.toString()}
                      onChange={val => handleItemChange(index, 'productId', val)}
                      placeholder="Selecionar Produto"
                      options={productsData?.items.map((p: any) => ({
                        label: `${p.name} - R$ ${p.price}`,
                        value: p.id.toString()
                      })) || []}
                    />
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

      <OrderDetailsModal
        isOpen={isDetailsOpen}
        onClose={() => setIsDetailsOpen(false)}
        order={selectedOrder}
        onStatusUpdated={() => {
          setIsDetailsOpen(false);
          // The useOrders hook should provide a reload method, but usually just changing page or status reloads it.
          // Let's assume setPage(page) or similar triggers it.
          setPage(page);
        }}
      />
    </div>
  );
}
