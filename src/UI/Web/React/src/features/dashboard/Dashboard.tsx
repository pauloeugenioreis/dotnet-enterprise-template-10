import { useNavigate } from 'react-router-dom';
import { useDashboard } from './useDashboard';

export default function Dashboard() {
  const navigate = useNavigate();
  const { orders, isLoading, stats } = useDashboard();

  return (
    <div className="w-full space-y-10 animate-in fade-in duration-500">
      <header>
        <h1 className="text-6xl font-extrabold text-slate-900 tracking-tight">Dashboard</h1>
        <p className="text-gray-500 mt-3 font-medium text-xl">Performance em tempo real.</p>
      </header>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-10">
        {stats.map((stat, i) => (
          <div key={i} className="bg-white p-10 rounded-[3rem] shadow-xl shadow-gray-200/50 border border-gray-50 flex flex-col gap-6 group cursor-default">
            <div className={`${stat.color} w-16 h-16 rounded-2xl flex items-center justify-center text-3xl group-hover:scale-110 transition-transform`}>
              {stat.icon}
            </div>
            <div>
              <p className="text-[10px] font-black text-gray-400 uppercase tracking-[0.3em] mb-2">{stat.label}</p>
              <h2 className="text-4xl font-black text-gray-900 tracking-tighter">{stat.value}</h2>
            </div>
          </div>
        ))}
      </div>

      {/* Latest Orders Table Preview */}
      <div className="bg-white rounded-[3.5rem] shadow-2xl shadow-gray-200/60 border border-gray-50 overflow-hidden">
        <div className="p-12 border-b border-gray-50 flex justify-between items-center">
          <h3 className="text-3xl font-black text-gray-900 tracking-tight">Últimos Pedidos</h3>
          <button 
            onClick={() => navigate('/orders')}
            className="text-[10px] font-black text-primary-600 uppercase tracking-[0.2em] hover:bg-primary-50 px-6 py-3 rounded-xl transition-all"
          >
            Ver Todos
          </button>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50/30">
              <tr>
                <th className="px-12 py-8 text-[10px] font-black text-gray-300 uppercase tracking-[0.2em]">Pedido</th>
                <th className="px-12 py-8 text-[10px] font-black text-gray-300 uppercase tracking-[0.2em]">Cliente</th>
                <th className="px-12 py-8 text-[10px] font-black text-gray-300 uppercase tracking-[0.2em]">Total</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {isLoading ? (
                <tr><td colSpan={3} className="px-12 py-8 animate-pulse bg-gray-50/50 h-20"></td></tr>
              ) : orders?.items.map((order: any) => (
                <tr key={order.id} className="hover:bg-gray-50/30 transition-colors group">
                  <td className="px-12 py-10 font-black text-gray-900 tracking-tight text-lg">#{order.orderNumber}</td>
                  <td className="px-12 py-10 font-bold text-gray-500 text-lg">{order.customerName}</td>
                  <td className="px-12 py-10 font-black text-primary-600 text-xl tracking-tight">R$ {order.total.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
