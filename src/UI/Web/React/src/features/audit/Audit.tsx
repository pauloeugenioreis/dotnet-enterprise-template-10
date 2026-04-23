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
      <div className="flex flex-wrap gap-6 items-end mb-10">
        <div className="w-[180px] space-y-2">
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

        <div className="w-[180px] space-y-2">
          <label className="text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 ml-4">Evento</label>
          <input 
            type="text"
            placeholder="Tipo..."
            className="w-full px-8 py-5 bg-white border-none rounded-[2rem] shadow-xl shadow-gray-100/50 outline-none font-bold text-xs text-gray-900 focus:ring-2 focus:ring-primary-600/10 transition-all"
            value={eventType}
            onChange={(e) => setEventType(e.target.value)}
          />
        </div>

        <div className="w-[180px] space-y-2">
          <label className="text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 ml-4">Usuário</label>
          <input 
            type="text"
            placeholder="User ID..."
            className="w-full px-8 py-5 bg-white border-none rounded-[2rem] shadow-xl shadow-gray-100/50 outline-none font-bold text-xs text-gray-900 focus:ring-2 focus:ring-primary-600/10 transition-all"
            value={userId}
            onChange={(e) => setUserId(e.target.value)}
          />
        </div>

        <div className="flex-1 min-w-[320px] space-y-2">
          <label className="text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 ml-4">Período</label>
          <div className="flex bg-white p-2 rounded-[2rem] shadow-xl shadow-gray-100/50 gap-2 items-center border border-gray-50">
            <div className="flex-1 px-6 py-3 flex items-center gap-2">
              <label className="text-[10px] font-black uppercase text-gray-300">De</label>
              <input 
                type="date"
                className="w-full bg-transparent border-none outline-none font-bold text-xs text-gray-900 cursor-pointer"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
              />
            </div>
            <div className="w-px h-8 bg-gray-100"></div>
            <div className="flex-1 px-6 py-3 flex items-center gap-2">
              <label className="text-[10px] font-black uppercase text-gray-300">Até</label>
              <input 
                type="date"
                className="w-full bg-transparent border-none outline-none font-bold text-xs text-gray-900 cursor-pointer"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
              />
            </div>
          </div>
        </div>

        <div className="flex justify-end">
          <button 
            onClick={() => {
              setEntityType('');
              setEventType('');
              setUserId('');
              setFrom('');
              setToDate('');
            }}
            className="group flex items-center gap-3 px-8 py-5 text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 hover:text-red-500 transition-all"
          >
            <span className="group-hover:rotate-180 transition-transform duration-500">✕</span>
            Limpar
          </button>
        </div>
      </div>

      <div className="bg-white rounded-[3rem] border border-gray-50 shadow-xl overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-900 text-white">
              <tr>
                <th className="px-10 py-6 text-xs font-black uppercase tracking-widest">Evento</th>
                <th className="px-10 py-6 text-xs font-black uppercase tracking-widest">Entidade</th>
                <th className="px-10 py-6 text-xs font-black uppercase tracking-widest">Usuário</th>
                <th className="px-10 py-6 text-xs font-black uppercase tracking-widest">Data</th>
                <th className="px-10 py-6 text-xs font-black uppercase tracking-widest text-center">Ações</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {isLoading ? (
                <tr><td colSpan={5} className="p-20 text-center"><div className="w-10 h-10 border-4 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto" /></td></tr>
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
                  <td className="px-10 py-6 text-center">
                    <button 
                      onClick={() => setSelectedLog(log)}
                      className="p-3 bg-gray-50 hover:bg-gray-900 hover:text-white rounded-2xl transition-all duration-300 group"
                      title="Ver Detalhes"
                    >
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                      </svg>
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        
        <Pagination 
          currentPage={page}
          totalPages={totalPages}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
        />
      </div>

      {/* Detail Modal */}
      {selectedLog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-6 bg-gray-900/40 backdrop-blur-sm animate-in fade-in duration-300">
          <div className="bg-white w-full max-w-4xl rounded-[3rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-300">
            <header className="px-10 py-8 bg-gray-900 text-white flex justify-between items-center">
              <div>
                <h3 className="text-2xl font-black tracking-tighter">Detalhes do Evento</h3>
                <p className="text-gray-400 text-sm font-bold uppercase tracking-widest mt-1">{selectedLog.eventType}</p>
              </div>
              <button onClick={closeLog} className="p-3 hover:bg-white/10 rounded-2xl transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </header>
            
            <div className="p-10 max-h-[70vh] overflow-y-auto bg-gray-50">
              <div className="grid grid-cols-2 gap-6 mb-10">
                <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
                  <label className="text-[10px] font-black uppercase tracking-widest text-gray-400 block mb-2">Entidade</label>
                  <p className="font-black text-gray-900 text-xl">{selectedLog.aggregateType}</p>
                  <p className="text-gray-500 font-mono text-xs mt-1">ID: {selectedLog.aggregateId}</p>
                </div>
                <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
                  <label className="text-[10px] font-black uppercase tracking-widest text-gray-400 block mb-2">Metadata</label>
                  <p className="font-bold text-gray-900">{new Date(selectedLog.occurredOn).toLocaleString()}</p>
                  <p className="text-gray-500 text-xs mt-1">Versão: {selectedLog.version}</p>
                </div>
              </div>

              <div className="bg-gray-900 rounded-[2rem] p-8 shadow-inner">
                <label className="text-[10px] font-black uppercase tracking-widest text-gray-500 block mb-4">Payload (JSON)</label>
                <pre className="text-primary-300 font-mono text-sm overflow-x-auto leading-relaxed">
                  {JSON.stringify(selectedLog.eventData, null, 2)}
                </pre>
              </div>
            </div>

            <footer className="px-10 py-8 bg-white border-t border-gray-100 flex justify-end">
              <button 
                onClick={closeLog}
                className="px-10 py-4 bg-gray-900 text-white rounded-2xl font-black tracking-tighter hover:scale-105 active:scale-95 transition-all shadow-xl shadow-gray-900/20"
              >
                Fechar
              </button>
            </footer>
          </div>
        </div>
      )}
    </div>
  );
}
