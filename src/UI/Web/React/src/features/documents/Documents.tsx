import { useState } from 'react';
import { documentService } from '../../api/services/document.service';
import { CloudUpload, Download, Trash2, FileCode } from 'lucide-react';

export default function Documents() {
  const [fileName, setFileName] = useState('');
  const [uploading, setUploading] = useState(false);
  const [processing, setProcessing] = useState(false);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setUploading(true);
      try {
        const res = await documentService.upload(file);
        setFileName(res.fileName);
      } finally {
        setUploading(false);
      }
    }
  };

  const handleDownload = async () => {
    if (!fileName) return;
    setProcessing(true);
    try {
      const response = await documentService.download(fileName);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } finally {
      setProcessing(false);
    }
  };

  const handleDelete = async () => {
    if (!fileName) return;
    setProcessing(true);
    try {
      await documentService.delete(fileName);
      setFileName('');
    } finally {
      setProcessing(false);
    }
  };

  return (
    <div className="animate-in fade-in slide-in-from-bottom-4 duration-700 max-w-4xl mx-auto py-10">
      <div className="mb-16 text-center">
        <h1 className="text-6xl font-black text-gray-900 tracking-tighter">Cloud Storage</h1>
        <p className="text-gray-500 mt-4 font-medium text-xl">Upload, download e gerenciamento direto de arquivos.</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-10">
        {/* Upload Section */}
        <div className="bg-white p-12 rounded-[3.5rem] shadow-2xl shadow-gray-200/60 border border-gray-50 flex flex-col items-center gap-8 group">
          <div className="w-32 h-32 bg-primary-50 rounded-[2.5rem] flex items-center justify-center text-primary-600 transition-transform group-hover:scale-110 duration-500">
            {uploading ? (
              <div className="w-12 h-12 border-4 border-primary-100 border-t-primary-600 rounded-full animate-spin" />
            ) : (
              <CloudUpload size={64} />
            )}
          </div>
          
          <div className="text-center">
            <h3 className="text-2xl font-black text-gray-900 mb-2">Novo Upload</h3>
            <p className="text-gray-400 text-sm font-medium">Selecione um arquivo para subir</p>
          </div>

          <label className="w-full cursor-pointer">
            <input type="file" className="hidden" onChange={handleFileChange} disabled={uploading} />
            <div className="w-full py-5 bg-gray-900 text-white rounded-2xl font-black text-center hover:bg-black transition-all shadow-lg shadow-gray-200 active:scale-95">
              Selecionar Arquivo
            </div>
          </label>
        </div>

        {/* Management Section */}
        <div className="bg-white p-12 rounded-[3.5rem] shadow-2xl shadow-gray-200/60 border border-gray-50 flex flex-col gap-8">
          <div>
            <h3 className="text-2xl font-black text-gray-900 mb-2">Gerenciar Arquivo</h3>
            <p className="text-gray-400 text-sm font-medium">Digite o nome do arquivo para as ações</p>
          </div>

          <div className="space-y-4">
            <div className="relative">
              <input 
                type="text" 
                value={fileName}
                onChange={(e) => setFileName(e.target.value)}
                placeholder="ex: contrato_v1.pdf"
                className="w-full px-6 py-5 bg-gray-50 border-none rounded-2xl font-bold text-gray-700 placeholder:text-gray-300 focus:ring-2 focus:ring-primary-500 transition-all outline-none"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <button 
                onClick={handleDownload}
                disabled={!fileName || processing}
                className="py-5 bg-primary-50 text-primary-600 rounded-2xl font-black hover:bg-primary-100 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
              >
                <Download size={20} />
                Download
              </button>
              
              <button 
                onClick={handleDelete}
                disabled={!fileName || processing}
                className="py-5 bg-rose-50 text-rose-600 rounded-2xl font-black hover:bg-rose-100 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
              >
                <Trash2 size={20} />
                Deletar
              </button>
            </div>
          </div>

          {processing && (
            <div className="flex items-center justify-center gap-3 text-primary-600 font-bold animate-pulse">
              <div className="w-2 h-2 bg-primary-600 rounded-full"></div>
              Processando...
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
