import { useOrders } from './useOrders';

export default function Orders() {
  const { data, isLoading, getStatusColor } = useOrders();

  return (
    <div className="p-10 max-w-7xl mx-auto space-y-10 animate-in fade-in duration-500">
      <header>
        <h1 className="text-5xl font-black text-gray-900 tracking-tighter">Pedidos</h1>
        <p className="text-gray-500 mt-3 font-medium text-lg">Histórico completo de transações.</p>
      </header>

      <div className="bg-white rounded-[3rem] border border-gray-50 shadow-2xl overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400"># Número</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Cliente</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Status</th>
              <th className="px-10 py-6 text-xs font-black uppercase tracking-[0.2em] text-gray-400">Total</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {isLoading ? (
              <tr><td colSpan={4} className="p-20 text-center"><div className="w-10 h-10 border-4 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
            ) : data?.items.map((order: any) => (
              <tr key={order.id} className="hover:bg-gray-50/50 transition-colors group cursor-pointer">
                <td className="px-10 py-8 font-black text-gray-900 group-hover:text-primary-600 transition-colors">
                  {order.orderNumber}
                </td>
                <td className="px-10 py-8 text-gray-500 font-bold">{order.customerName}</td>
                <td className="px-10 py-8">
                  <span className={`px-4 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest ${getStatusColor(order.status)}`}>
                    {order.status}
                  </span>
                </td>
                <td className="px-10 py-8 text-xl font-black text-gray-900">
                  R$ {order.total.toLocaleString()}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
