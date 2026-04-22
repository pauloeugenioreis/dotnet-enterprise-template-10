import { useProducts } from './useProducts';

export default function Products() {
  const { data, isLoading } = useProducts();

  return (
    <div className="p-10 max-w-7xl mx-auto space-y-10 animate-in fade-in duration-500">
      <header className="flex justify-between items-end">
        <div>
          <h1 className="text-5xl font-black text-gray-900 tracking-tighter">Produtos</h1>
          <p className="text-gray-500 mt-3 font-medium text-lg">Catálogo completo de itens.</p>
        </div>
        <button className="bg-primary-600 hover:bg-primary-700 text-white font-black px-8 py-4 rounded-2xl shadow-lg shadow-primary-600/20 transition-all active:scale-95">
          + Adicionar Produto
        </button>
      </header>

      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {[1,2,3].map(i => <div key={i} className="h-80 bg-gray-100 rounded-[2.5rem] animate-pulse" />)}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {data?.items.map((product: any) => (
            <div key={product.id} className="bg-white rounded-[2.5rem] p-8 border border-gray-50 shadow-sm hover:shadow-2xl transition-all group overflow-hidden relative">
              <div className="w-20 h-20 bg-gray-50 rounded-2xl flex items-center justify-center text-4xl mb-6 group-hover:scale-110 transition-transform">
                📦
              </div>
              <h3 className="text-xl font-black text-gray-900 mb-2">{product.name}</h3>
              <p className="text-gray-400 text-sm font-medium mb-6">{product.category}</p>
              
              <div className="flex justify-between items-center pt-6 border-t border-gray-50">
                <div className="text-2xl font-black text-primary-600">
                  R$ {product.price.toLocaleString()}
                </div>
                <span className="text-[10px] font-black uppercase tracking-widest text-gray-300">
                  Qtd: {product.stockQuantity}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
