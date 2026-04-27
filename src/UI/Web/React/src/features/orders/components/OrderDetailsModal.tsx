import React from 'react';
import { OrderResponse } from '../../../types';
import { orderService } from '../../../api/services/order.service';
import Dropdown from '../../../components/Dropdown';

interface OrderDetailsModalProps {
  order: OrderResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onStatusUpdated: () => void;
}

import { useOrderDetails } from './useOrderDetails';
import Drawer from '../../../components/Drawer';

const OrderDetailsModal: React.FC<OrderDetailsModalProps> = ({ order, isOpen, onClose, onStatusUpdated }) => {
  const { handleStatusChange, getStatusClass, formatCurrency, formatDate } = useOrderDetails(onStatusUpdated);

  return (
    <Drawer
      isOpen={isOpen}
      onClose={onClose}
      title={`Pedido #${order?.orderNumber}`}
      subtitle={order ? `Realizado em ${formatDate(order.createdAt)}` : ''}
      footer={
        <div className="flex gap-4 no-print">
          <button 
            onClick={() => window.print()}
            className="flex-1 bg-gradient-to-br from-primary-50/50 to-white border border-primary-100 py-4 rounded-2xl text-[11px] font-bold text-primary-700 uppercase tracking-widest hover:bg-white hover:border-primary-200 hover:shadow-lg hover:shadow-primary-100/50 transition-all active:scale-95 flex items-center justify-center gap-3"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-primary-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
            </svg>
            Imprimir Pedido
          </button>
          <div className="flex-1">
            <Dropdown 
              variant="primary"
              value={order?.status || ''}
              onChange={(status) => order && handleStatusChange(order.id, status)}
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
      }
    >
      {order && (
        <div className="space-y-10 print-area">
          {/* Header para Impressão (Apenas número) */}
          <div className="hidden print:flex justify-end border-b-2 border-gray-900 pb-4 mb-8">
            <div className="text-right">
              <p className="text-[10px] font-black uppercase tracking-widest text-gray-400">Pedido Número</p>
              <p className="text-2xl font-black text-gray-900">#{order.orderNumber}</p>
            </div>
          </div>
          {/* Status & Info */}
          <div className="grid grid-cols-2 gap-8">
            <div>
              <p className="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Status do Pedido</p>
              <span className={`px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest ${getStatusClass(order.status)}`}>
                {order.status}
              </span>
            </div>
            <div>
              <p className="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Cliente</p>
              <p className="font-bold text-gray-900">{order.customerName}</p>
              <p className="text-xs text-gray-500">{order.customerEmail}</p>
            </div>
          </div>

          {/* Items Table */}
          <div>
            <p className="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-4">Itens do Pedido</p>
            <div className="bg-gray-50 rounded-3xl overflow-hidden border border-gray-100">
              <table className="w-full text-left">
                <thead className="bg-gray-100/50 text-[10px] uppercase text-gray-400">
                  <tr>
                    <th className="px-6 py-4">Produto</th>
                    <th className="px-6 py-4 text-center">Qtd</th>
                    <th className="px-6 py-4 text-right">Preço</th>
                    <th className="px-6 py-4 text-right">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {order.items.map((item, index) => (
                    <tr key={index}>
                      <td className="px-6 py-4">
                        <p className="text-sm font-bold text-gray-800">{item.productName}</p>
                        <p className="text-[10px] text-gray-400">ID: {item.productId}</p>
                      </td>
                      <td className="px-6 py-4 text-center text-sm font-medium text-gray-600">{item.quantity}</td>
                      <td className="px-6 py-4 text-right text-sm text-gray-600">{formatCurrency(item.unitPrice)}</td>
                      <td className="px-6 py-4 text-right text-sm font-bold text-gray-900">{formatCurrency(item.subtotal)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Shipping Address */}
          <div>
            <p className="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Endereço de Entrega</p>
            <div className="p-5 bg-primary-50/30 rounded-2xl border border-primary-100/50 flex gap-4">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-primary-600 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <p className="text-sm text-gray-700 leading-relaxed">{order.shippingAddress}</p>
            </div>
          </div>

          {/* Totals Summary */}
          <div className="space-y-3 pt-6 border-t border-gray-100">
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Subtotal</span>
              <span className="font-medium text-gray-900">{formatCurrency(order.subtotal)}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Frete</span>
              <span className="font-medium text-gray-900">{formatCurrency(order.shippingCost)}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Impostos</span>
              <span className="font-medium text-gray-900">{formatCurrency(order.tax)}</span>
            </div>
            <div className="pt-4 border-t border-gray-200 flex justify-between items-end">
              <span className="text-lg font-bold text-gray-900">Total</span>
              <span className="text-3xl font-black text-primary-600">{formatCurrency(order.total)}</span>
            </div>
          </div>
        </div>
      )}
    </Drawer>
  );
};

export default OrderDetailsModal;
