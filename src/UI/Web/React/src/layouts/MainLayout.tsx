import React from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../store/useAuthStore';

export default function MainLayout({ children }: { children: React.ReactNode }) {
  const logout = useAuthStore((state) => state.logout);
  const user = useAuthStore((state) => state.user);
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const menuItems = [
    { name: 'Dashboard', path: '/dashboard', icon: '📊' },
    { name: 'Produtos', path: '/products', icon: '📦' },
    { name: 'Pedidos', path: '/orders', icon: '🛍️' },
    { name: 'Auditoria', path: '/audit', icon: '🛡️' },
  ];

  const isAdmin = user?.email?.toLowerCase() === 'admin@projecttemplate.com';
  const userInitial = isAdmin ? 'S' : (user?.firstName?.[0] || user?.email?.[0] || 'U').toUpperCase();
  const userName = isAdmin ? 'System Administrator' : (user?.firstName ? `${user.firstName} ${user.lastName}` : 'Usuário');

  return (
    <div className="flex h-screen bg-gray-100 overflow-hidden font-sans">
      {/* Sidebar */}
      <div className="w-72 bg-white shadow-2xl relative flex flex-col z-20">
        <div className="p-10 border-b border-gray-50 flex items-center gap-4">
          <div className="w-12 h-12 bg-primary-600 rounded-[1.25rem] flex items-center justify-center text-white font-black text-2xl shadow-xl shadow-primary-600/20">
            ET
          </div>
          <div className="flex flex-col">
            <span className="font-black text-2xl text-gray-900 tracking-tighter">Enterprise</span>
            <span className="text-[10px] font-bold text-primary-600 uppercase tracking-[0.3em] -mt-1">React Edition</span>
          </div>
        </div>

        <nav className="mt-10 px-6 space-y-3 flex-1">
          {menuItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              className={`flex items-center gap-4 px-6 py-4 rounded-2xl transition-all duration-300 group ${
                location.pathname === item.path
                  ? 'bg-primary-600 text-white shadow-lg shadow-primary-600/30'
                  : 'text-gray-400 hover:bg-gray-50 hover:text-gray-900'
              }`}
            >
              <span className="text-xl group-hover:scale-110 transition-transform">{item.icon}</span>
              <span className="font-black text-xs uppercase tracking-widest">{item.name}</span>
            </Link>
          ))}
        </nav>

        {/* User Section */}
        <div className="p-8 border-t border-gray-100 flex flex-col gap-6 bg-gray-50/30">
          <div className="flex items-center gap-4 group cursor-default">
            <div className="w-14 h-14 bg-gradient-to-br from-primary-500 to-primary-700 rounded-2xl flex items-center justify-center text-white font-black text-2xl shadow-xl shadow-primary-200 group-hover:scale-110 transition-transform">
              {userInitial}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-black text-gray-900 truncate tracking-tight">{userName}</p>
              <p className="text-[10px] text-gray-400 font-bold truncate uppercase tracking-widest">{user?.email || 'admin@projecttemplate.com'}</p>
            </div>
          </div>
          <button
            onClick={handleLogout}
            className="w-full flex items-center justify-center gap-3 px-6 py-4 bg-white border border-red-100 text-red-500 rounded-2xl hover:bg-red-500 hover:text-white hover:border-red-500 transition-all text-[11px] font-black uppercase tracking-widest active:scale-95 shadow-sm hover:shadow-lg hover:shadow-red-500/20"
          >
            <span className="text-lg">🚪</span> Sair da Conta
          </button>
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 overflow-auto bg-gray-50/50 relative">
        <div className="absolute inset-0 bg-[radial-gradient(#e5e7eb_1px,transparent_1px)] [background-size:24px_24px] [mask-image:radial-gradient(ellipse_50%_50%_at_50%_50%,#000_70%,transparent_100%)] opacity-40 pointer-events-none"></div>
        <main className="relative z-10 min-h-full">
          <div className="p-10">
            {children}
          </div>
        </main>
      </div>
    </div>
  );
}
