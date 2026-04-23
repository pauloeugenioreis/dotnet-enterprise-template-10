import React from 'react';
import { OrderResponseDto, orderService } from '../../../api/services';
import Dropdown from '../../../components/Dropdown';

interface OrderDetailsModalProps {
  order: OrderResponseDto | null;
  isOpen: boolean;
  onClose: () => void;
  onStatusUpdated: () => void;
}

const OrderDetailsModal: React.FC<OrderDetailsModalProps> = ({ order, isOpen, onClose, onStatusUpdated }) => {
  if (!isOpen || !order) return null;

  const handleStatusChange = async (newStatus: string) => {
    if (!order || newStatus === order.status) return;

    try {
      await orderService.updateStatus(order.id, newStatus);
      onStatusUpdated();
    } catch (error) {
      alert('Erro ao atualizar status');
    }
  };

  const getStatusClass = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending': return 'bg-amber-50 text-amber-600';
      case 'completed': return 'bg-emerald-50 text-emerald-600';
      case 'cancelled': return 'bg-rose-50 text-rose-600';
      case 'shipped': return 'bg-indigo-50 text-indigo-600';
      default: return 'bg-gray-50 text-gray-600';
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-end">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-gray-900/40 backdrop-blur-sm transition-opacity" 
        onClick={onClose}
      />

      {/* Modal Content */}
      <div className="relative h-full w-full max-w-2xl bg-white shadow-2xl flex flex-col animate-in slide-in-from-right duration-300 print-area">
        {/* Header */}
        <div className="p-8 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
          <div>
            <h2 className="text-2xl font-black text-gray-900 tracking-tight">Pedido #{order.orderNumber}</h2>
            <p className="text-gray-500 text-sm mt-1 italic font-medium">
              Realizado em {new Date(order.createdAt).toLocaleString('pt-BR')}
            </p>
          </div>
          <button 
            onClick={onClose}
            className="p-3 hover:bg-gray-100 rounded-2xl transition-all active:scale-90 no-print"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-8 space-y-10">
          {/* Status & Info */}
          <div className="grid grid-cols-2 gap-8">
            <div className="space-y-3">
              <p className="text-[10px] uppercase tracking-[0.2em] text-gray-400 font-black">Status do Pedido</p>
              <span className={`px-5 py-2.5 rounded-2xl text-[10px] font-black uppercase tracking-[0.1em] shadow-sm ${getStatusClass(order.status)}`}>
                {order.status}
              </span>
            </div>
            <div className="space-y-1">
              <p className="text-[10px] uppercase tracking-[0.2em] text-gray-400 font-black mb-3">Cliente</p>
              <p className="font-black text-gray-900 text-lg leading-none tracking-tight">{order.customerName}</p>
              <p className="text-xs text-gray-400 font-bold uppercase tracking-widest">{order.customerEmail}</p>
            </div>
          </div>

          {/* Items Table */}
          <div className="space-y-4">
            <p className="text-[10px] uppercase tracking-[0.2em] text-gray-400 font-black">Itens do Pedido</p>
            <div className="bg-gray-50 rounded-[2.5rem] overflow-hidden border border-gray-100 shadow-xl shadow-gray-100/20">
              <table className="w-full text-left">
                <thead className="bg-gray-100/50 text-[10px] font-black uppercase tracking-widest text-gray-400">
                  <tr>
                    <th className="px-8 py-5">Produto</th>
                    <th className="px-8 py-5 text-center">Qtd</th>
                    <th className="px-8 py-5 text-right">Preço</th>
                    <th className="px-8 py-5 text-right">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {order.items.map((item, index) => (
                    <tr key={index} className="hover:bg-white transition-colors group">
                      <td className="px-8 py-6">
                        <p className="text-sm font-black text-gray-800 tracking-tight">{item.productName}</p>
                        <p className="text-[10px] font-bold text-gray-400 uppercase tracking-widest mt-0.5">ID: {item.productId}</p>
                      </td>
                      <td className="px-8 py-6 text-center">
                        <span className="inline-flex items-center justify-center w-8 h-8 bg-white rounded-lg text-sm font-black text-gray-600 shadow-sm border border-gray-50">
                          {item.quantity}
                        </span>
                      </td>
                      <td className="px-8 py-6 text-right text-sm font-medium text-gray-500 italic">{formatCurrency(item.unitPrice)}</td>
                      <td className="px-8 py-6 text-right text-sm font-black text-gray-900">{formatCurrency(item.subtotal)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Shipping Address */}
          <div className="space-y-4">
            <p className="text-[10px] uppercase tracking-[0.2em] text-gray-400 font-black">Endereço de Entrega</p>
            <div className="p-6 bg-blue-50/40 rounded-[2rem] border border-blue-100/50 flex gap-5 items-start">
              <div className="w-12 h-12 bg-white rounded-2xl flex items-center justify-center shadow-sm shrink-0 border border-blue-50">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
              </div>
              <p className="text-sm text-gray-700 leading-relaxed font-bold italic pt-1">{order.shippingAddress}</p>
            </div>
          </div>
        </div>

        {/* Footer / Totals */}
        <div className="p-8 border-t border-gray-100 bg-gray-50/50">
          <div className="space-y-4 px-4">
            <div className="flex justify-between text-sm">
              <span className="text-gray-400 font-black uppercase tracking-[0.2em] text-[10px]">Subtotal</span>
              <span className="font-black text-gray-900 tracking-tight">{formatCurrency(order.subtotal)}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-400 font-black uppercase tracking-[0.2em] text-[10px]">Frete</span>
              <span className="font-black text-gray-900 tracking-tight">{formatCurrency(order.shippingCost)}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-400 font-black uppercase tracking-[0.2em] text-[10px]">Impostos</span>
              <span className="font-black text-gray-900 tracking-tight">{formatCurrency(order.tax)}</span>
            </div>
            <div className="pt-6 mt-2 border-t border-gray-200 flex justify-between items-end">
              <span className="text-lg font-black text-gray-900 uppercase tracking-tighter">Total</span>
              <span className="text-5xl font-black text-blue-600 tracking-tighter tabular-nums drop-shadow-sm">
                {formatCurrency(order.total)}
              </span>
            </div>
          </div>
          
          <div className="mt-10 flex gap-4 no-print">
            <button 
              onClick={() => window.print()}
              className="flex-1 bg-gradient-to-br from-gray-50 to-white border border-gray-200 py-4 rounded-2xl text-[11px] font-bold text-gray-700 uppercase tracking-widest hover:bg-white hover:border-gray-300 hover:shadow-lg hover:shadow-gray-100 transition-all active:scale-95 flex items-center justify-center gap-3"
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
              </svg>
              Imprimir Pedido
            </button>
            <div className="flex-1">
              <Dropdown 
                variant="primary"
                value={order.status}
                onChange={handleStatusChange}
                placeholder="Atualizar Status"
                labelOverride="Atualizar Status"
                direction="up"
                options={[
                  { label: 'Pendente', value: 'Pending' },
                  { label: 'Enviado', value: 'Shipped' },
                  { label: 'Entregue', value: 'Delivered' },
                  { label: 'Cancelado', value: 'Cancelled' }
                ]}
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default OrderDetailsModal;
