import { useDocuments } from './useDocuments';

export default function Documents() {
  const { docs } = useDocuments();

  return (
    <div className="p-10 max-w-7xl mx-auto space-y-10 animate-in fade-in duration-500">
      <header>
        <h1 className="text-5xl font-black text-gray-900 tracking-tighter">Documentos</h1>
        <p className="text-gray-500 mt-3 font-medium text-lg">Central de arquivos e contratos.</p>
      </header>

      <div className="bg-white rounded-[3rem] border border-gray-50 shadow-2xl overflow-hidden">
        <div className="divide-y divide-gray-50">
          {docs.map((doc, i) => (
            <div key={i} className="p-8 flex items-center justify-between hover:bg-gray-50/50 transition-all group">
              <div className="flex items-center space-x-6">
                <div className={`w-16 h-16 rounded-2xl flex items-center justify-center text-xs font-black text-white shadow-xl shadow-red-600/20 ${doc.type === 'PDF' ? 'bg-red-500' : 'bg-blue-500'}`}>
                  {doc.type}
                </div>
                <div>
                  <h4 className="text-lg font-black text-gray-900 group-hover:text-primary-600 transition-colors">{doc.name}</h4>
                  <p className="text-gray-400 text-sm font-bold uppercase tracking-widest">{doc.size} • PDF Document</p>
                </div>
              </div>
              <button className="bg-gray-900 text-white font-black px-6 py-3 rounded-xl hover:bg-primary-600 transition-all active:scale-95 shadow-lg">
                Download
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
