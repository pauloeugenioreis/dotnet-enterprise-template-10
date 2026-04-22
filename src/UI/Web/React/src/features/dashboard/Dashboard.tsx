import { useDashboard } from './useDashboard';

export default function Dashboard() {
  const { orders, isLoading, stats } = useDashboard();

  return (
    <div className="p-10 max-w-7xl mx-auto space-y-10 animate-in fade-in duration-500">
      <header>
        <h1 className="text-5xl font-black text-gray-900 tracking-tighter">Dashboard</h1>
        <p className="text-gray-500 mt-3 font-medium text-lg">Performance em tempo real.</p>
      </header>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        {stats.map((stat, i) => (
          <div key={i} className="bg-white p-10 rounded-[2.5rem] border border-gray-50 shadow-sm hover:shadow-2xl transition-all group cursor-default">
            <div className={`${stat.color} w-16 h-16 rounded-2xl flex items-center justify-center mb-6 text-3xl group-hover:scale-110 transition-transform`}>
              {stat.icon}
            </div>
            <p className="text-gray-400 text-xs font-black uppercase tracking-[0.2em] mb-2">{stat.label}</p>
            <h2 className="text-4xl font-black text-gray-900">{stat.value}</h2>
          </div>
        ))}
      </div>

      {/* Latest Orders Table Preview */}
      <div className="bg-white rounded-[3rem] border border-gray-50 shadow-xl overflow-hidden">
        <div className="p-10 border-b border-gray-50 flex justify-between items-center">
          <h3 className="text-2xl font-black text-gray-900">Últimos Pedidos</h3>
          <button className="text-primary-600 font-black text-sm uppercase tracking-widest hover:underline">Ver Todos</button>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50/50">
              <tr>
                <th className="px-10 py-5 text-xs font-black text-gray-400 uppercase tracking-widest">Pedido</th>
                <th className="px-10 py-5 text-xs font-black text-gray-400 uppercase tracking-widest">Cliente</th>
                <th className="px-10 py-5 text-xs font-black text-gray-400 uppercase tracking-widest">Total</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {isLoading ? (
                <tr><td colSpan={3} className="p-10 text-center font-bold text-gray-300">Carregando...</td></tr>
              ) : orders?.items.map((order: any) => (
                <tr key={order.id} className="hover:bg-gray-50/50 transition-colors">
                  <td className="px-10 py-6 font-black text-gray-900">#{order.orderNumber}</td>
                  <td className="px-10 py-6 text-gray-500 font-medium">{order.customerName}</td>
                  <td className="px-10 py-6 font-black text-primary-600">R$ {order.total.toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
