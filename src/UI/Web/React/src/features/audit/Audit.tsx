import { useAudit } from './useAudit';

export default function Audit() {
  const { data, isLoading } = useAudit();

  return (
    <div className="p-10 max-w-7xl mx-auto space-y-10 animate-in fade-in duration-500">
      <header>
        <h1 className="text-5xl font-black text-gray-900 tracking-tighter">Auditoria</h1>
        <p className="text-gray-500 mt-3 font-medium text-lg">Histórico de integridade do sistema.</p>
      </header>

      <div className="bg-white rounded-[3rem] border border-gray-50 shadow-xl overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-900 text-white">
              <tr>
                <th className="px-10 py-6 text-xs font-black uppercase tracking-widest">Evento</th>
                <th className="px-10 py-6 text-xs font-black uppercase tracking-widest">Entidade</th>
                <th className="px-10 py-6 text-xs font-black uppercase tracking-widest">Usuário</th>
                <th className="px-10 py-6 text-xs font-black uppercase tracking-widest">Data</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {isLoading ? (
                <tr><td colSpan={4} className="p-20 text-center"><div className="w-10 h-10 border-4 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
              ) : data?.items.map((log: any) => (
                <tr key={log.eventId} className="hover:bg-gray-50/50 transition-colors">
                  <td className="px-10 py-6">
                    <span className="px-4 py-2 bg-primary-600 text-white rounded-xl text-[10px] font-black uppercase tracking-widest shadow-lg shadow-primary-600/20">
                      {log.eventType}
                    </span>
                  </td>
                  <td className="px-10 py-6 font-black text-gray-900">{log.aggregateType}</td>
                  <td className="px-10 py-6 text-gray-400 font-mono text-xs">{log.userId}</td>
                  <td className="px-10 py-6 text-gray-500 font-bold">
                    {new Date(log.occurredOn).toLocaleString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
