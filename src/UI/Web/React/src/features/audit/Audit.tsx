import { useState } from 'react';
import { useAudit } from './useAudit';
import Pagination from '../../components/Pagination';
import Dropdown from '../../components/Dropdown';

export default function Audit() {
  const {
    data, isLoading, page, setPage, pageSize, setPageSize,
    entityType, setEntityType, eventType, setEventType,
    userId, setUserId, from, setFrom, toDate, setToDate
  } = useAudit();
  const [selectedLog, setSelectedLog] = useState<any>(null);

  const totalPages = data?.totalPages || 1;

  const closeLog = () => setSelectedLog(null);

  return (
    <div className="w-full space-y-10 animate-in fade-in duration-500">
      <header>
        <h1 className="text-6xl font-black text-gray-900 tracking-tighter">Auditoria</h1>
        <p className="text-gray-500 mt-4 font-medium text-xl">Histórico de integridade do sistema.</p>
      </header>

      {/* Filter Bar */}
      <div className="grid grid-cols-12 gap-6 items-end mb-10">
        <div className="col-span-2 space-y-3">
          <label className="text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 ml-4">Agregado</label>
          <Dropdown
            value={entityType}
            onChange={setEntityType}
            options={[
              { label: 'Todos', value: '' },
              { label: 'Pedidos', value: 'Order' },
              { label: 'Produtos', value: 'Product' }
            ]}
          />
        </div>

        <div className="col-span-2 space-y-3">
          <label className="text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 ml-4">Evento</label>
          <input
            type="text"
            placeholder="Tipo..."
            className="w-full px-8 py-5 bg-white border-none rounded-[2rem] shadow-xl shadow-gray-100/50 outline-none font-bold text-xs text-gray-900 focus:ring-2 focus:ring-primary-600/10 transition-all placeholder:text-gray-300"
            value={eventType}
            onChange={(e) => setEventType(e.target.value)}
          />
        </div>

        <div className="col-span-2 space-y-3">
          <label className="text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 ml-4">Usuário</label>
          <input
            type="text"
            placeholder="User ID..."
            className="w-full px-8 py-5 bg-white border-none rounded-[2rem] shadow-xl shadow-gray-100/50 outline-none font-bold text-xs text-gray-900 focus:ring-2 focus:ring-primary-600/10 transition-all placeholder:text-gray-300"
            value={userId}
            onChange={(e) => setUserId(e.target.value)}
          />
        </div>

        <div className="col-span-4 space-y-3">
          <label className="text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 ml-4">Período</label>
          <div className="flex bg-white p-2 rounded-[2rem] shadow-xl shadow-gray-100/50 gap-0 items-center border border-gray-50">
            <div className="flex-1 px-4 py-3 hover:bg-gray-50 rounded-2xl transition-colors flex items-center gap-3">
              <label className="text-[10px] font-black uppercase tracking-[0.1em] text-gray-400">De</label>
              <input
                type="date"
                className="w-full bg-transparent border-none outline-none font-bold text-[10px] text-gray-900 cursor-pointer"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
              />
            </div>
            <div className="w-[1px] h-6 bg-gray-100 self-center"></div>
            <div className="flex-1 px-4 py-3 hover:bg-gray-50 rounded-2xl transition-colors flex items-center gap-3">
              <label className="text-[10px] font-black uppercase tracking-[0.1em] text-gray-400">Até</label>
              <input
                type="date"
                className="w-full bg-transparent border-none outline-none font-bold text-[10px] text-gray-900 cursor-pointer"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
              />
            </div>
          </div>
        </div>

        <div className="col-span-2 flex justify-end">
          <button
            onClick={() => {
              setEntityType('');
              setEventType('');
              setUserId('');
              setFrom('');
              setToDate('');
            }}
            className="group flex items-center gap-3 px-8 py-5 text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 hover:text-primary-600 transition-all"
          >
            <span className="group-hover:rotate-180 transition-transform duration-500">🔄</span>
            Limpar
          </button>
        </div>
      </div>

      <div className="bg-white rounded-[3rem] shadow-2xl shadow-gray-200/40 border border-gray-50 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead className="bg-primary-600 text-white">
              <tr>
                <th className="px-10 py-8 text-[10px] font-black uppercase tracking-[0.2em]">Evento</th>
                <th className="px-10 py-8 text-[10px] font-black uppercase tracking-[0.2em]">Entidade</th>
                <th className="px-10 py-8 text-[10px] font-black uppercase tracking-[0.2em]">Usuário</th>
                <th className="px-10 py-8 text-[10px] font-black uppercase tracking-[0.2em]">Data</th>
                <th className="px-10 py-8 text-[10px] font-black uppercase tracking-[0.2em]">Ações</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {isLoading ? (
                <tr><td colSpan={5} className="p-20 text-center"><div className="w-10 h-10 border-4 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
              ) : data?.items.map((log: any) => (
                <tr key={log.eventId} className="hover:bg-gray-50/50 transition-colors group">
                  <td className="px-10 py-8">
                    <span className="px-4 py-2 bg-primary-50 text-primary-600 rounded-xl font-black text-[10px] uppercase tracking-widest">
                      {log.eventType}
                    </span>
                  </td>
                  <td className="px-10 py-8">
                    <div className="flex flex-col">
                      <span className="font-black text-gray-900 tracking-tight">{log.aggregateType}</span>
                      <span className="text-[10px] font-bold text-gray-300 uppercase tracking-widest">{log.aggregateId}</span>
                    </div>
                  </td>
                  <td className="px-10 py-8">
                    <span className="font-bold text-gray-500">{log.userId || 'Sistema'}</span>
                  </td>
                  <td className="px-10 py-8">
                    <span className="text-xs font-bold text-gray-400">
                      {new Date(log.occurredOn).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })}
                    </span>
                  </td>
                  <td className="px-10 py-8">
                    <div className="flex gap-2 group-hover:opacity-100 transition-opacity">
                      <button
                        onClick={() => setSelectedLog(log)}
                        className="p-3 hover:bg-primary-50 rounded-xl text-primary-600 transition-colors"
                      >
                        🔍
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

        <Pagination
          currentPage={page}
          totalPages={totalPages}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
        />

      {/* Detail Modal */}
      {selectedLog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-6 bg-gray-900/40 backdrop-blur-sm animate-in fade-in duration-300">
          <div className="bg-white w-full max-w-5xl rounded-[3rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-300 relative">
            <header className="px-10 py-8 bg-white border-b border-gray-50 flex justify-between items-center">
              <h3 className="text-3xl font-black text-gray-900 tracking-tighter">Detalhes do Evento</h3>
              <button onClick={closeLog} className="text-gray-300 hover:text-gray-900 transition-colors">
                <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </header>

            <div className="p-10 space-y-6">
              <div className="flex items-center gap-4">
                <span className="px-4 py-1.5 bg-primary-600 text-white rounded-xl text-[10px] font-black uppercase tracking-[0.2em] shadow-lg shadow-primary-600/20">
                  {selectedLog.eventType}
                </span>
                <div className="h-px flex-1 bg-gray-100"></div>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div className="bg-gray-50/50 p-6 rounded-[2rem] border border-gray-100/50">
                  <label className="text-[10px] font-black uppercase tracking-widest text-gray-400 block mb-2 ml-1">Entidade</label>
                  <p className="font-black text-gray-900 text-xl tracking-tight">{selectedLog.aggregateType}</p>
                  <p className="text-gray-400 font-mono text-[9px] mt-1">ID: {selectedLog.aggregateId}</p>
                </div>
                <div className="bg-gray-50/50 p-6 rounded-[2rem] border border-gray-100/50">
                  <label className="text-[10px] font-black uppercase tracking-widest text-gray-400 block mb-2 ml-1">Metadata</label>
                  <p className="font-black text-gray-900 text-xl tracking-tight">{new Date(selectedLog.occurredOn).toLocaleString('pt-BR')}</p>
                  <p className="text-gray-400 text-[9px] font-bold uppercase tracking-widest mt-1">Versão: {selectedLog.version}</p>
                </div>
                <div className="bg-gray-50/50 p-6 rounded-[2rem] border border-gray-100/50">
                  <label className="text-[10px] font-black uppercase tracking-widest text-gray-400 block mb-2 ml-1">Usuário</label>
                  <p className="font-black text-gray-900 text-xl tracking-tight">{selectedLog.userId || 'Sistema'}</p>
                  <p className="text-gray-400 text-[9px] font-bold uppercase tracking-widest mt-1">Origin: API</p>
                </div>
              </div>

              <div className="bg-gray-900 rounded-[2.5rem] p-8 shadow-xl shadow-gray-900/30 relative">
                <label className="text-[10px] font-black uppercase tracking-widest text-gray-500 block mb-4 ml-2">Payload (JSON)</label>
                <div className="overflow-hidden rounded-xl bg-gray-950/50 p-5 border border-white/5">
                  <pre className="text-primary-300 font-mono text-xs overflow-x-auto leading-relaxed max-h-[350px] scrollbar-thin">
                    {JSON.stringify(selectedLog.eventData, null, 2)}
                  </pre>
                </div>
              </div>

              <div className="flex justify-end pt-2">
                <button
                  onClick={closeLog}
                  className="group flex items-center gap-3 px-10 py-4 bg-gray-900 text-white rounded-2xl font-black text-[10px] uppercase tracking-widest hover:bg-primary-600 hover:scale-105 active:scale-90 transition-all shadow-xl shadow-gray-900/20"
                >
                  <span>Fechar Detalhes</span>
                  <span className="text-lg group-hover:translate-x-1 transition-transform">→</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
