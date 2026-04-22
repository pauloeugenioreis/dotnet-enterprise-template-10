import { useLogin } from './useLogin';

export default function Login() {
  const { email, setEmail, password, setPassword, loading, handleLogin } = useLogin();

  return (
    <div className="min-h-screen flex items-center justify-center bg-primary-50 p-6">
      <div className="max-w-md w-full bg-white rounded-[2.5rem] shadow-2xl p-10 border border-gray-100 animate-in fade-in slide-in-from-bottom-4 duration-700">
        <div className="flex justify-center mb-10">
          <div className="w-20 h-20 bg-primary-600 rounded-3xl flex items-center justify-center shadow-2xl shadow-primary-600/30 rotate-3">
            <span className="text-white text-4xl font-black -rotate-3">ET</span>
          </div>
        </div>
        
        <h1 className="text-4xl font-black text-center text-gray-900 tracking-tight mb-3">Bem-vindo</h1>
        <p className="text-center text-gray-500 mb-10 font-medium">Ecossistema Enterprise React</p>

        <form onSubmit={handleLogin} className="space-y-6">
          <div>
            <label className="block text-sm font-bold text-gray-700 mb-2 ml-1">E-mail</label>
            <input 
              type="email" 
              required
              className="w-full px-5 py-4 rounded-2xl border border-gray-100 bg-gray-50/50 focus:bg-white focus:ring-4 focus:ring-primary-600/10 focus:border-primary-600 transition-all outline-none"
              placeholder="seu@email.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>

          <div>
            <label className="block text-sm font-bold text-gray-700 mb-2 ml-1">Senha</label>
            <input 
              type="password" 
              required
              className="w-full px-5 py-4 rounded-2xl border border-gray-100 bg-gray-50/50 focus:bg-white focus:ring-4 focus:ring-primary-600/10 focus:border-primary-600 transition-all outline-none"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          <button 
            type="submit" 
            disabled={loading}
            className="w-full bg-primary-600 hover:bg-primary-700 text-white font-black py-5 rounded-2xl shadow-xl shadow-primary-600/20 transition-all active:scale-[0.98] disabled:opacity-50 flex items-center justify-center"
          >
            {loading ? <div className="w-6 h-6 border-4 border-white/30 border-t-white rounded-full animate-spin" /> : 'Entrar na Plataforma'}
          </button>
        </form>
      </div>
    </div>
  );
}
