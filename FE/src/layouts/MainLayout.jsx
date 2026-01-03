import React, { useState } from 'react';
import { Outlet, NavLink, useLocation } from 'react-router-dom';

const MainLayout = () => {
    const [isSidebarOpen, setIsSidebarOpen] = useState(false);
    const location = useLocation();

    // Close sidebar on route change (mobile)
    React.useEffect(() => {
        setIsSidebarOpen(false);
    }, [location]);

    const navItems = [
        { path: '/dashboard', label: 'Dashboard', icon: 'dashboard' },
        { path: '/library', label: 'My Library', icon: 'library_books' },
        // { path: '/chat', label: 'Recent Chat', icon: 'chat_bubble' }, // Optional
    ];

    return (
        <div className="flex h-screen bg-[#f8f9fa] dark:bg-background-dark text-text-light dark:text-text-dark font-display overflow-hidden relative">

            {/* Mobile Header */}
            <div className="md:hidden fixed top-0 left-0 right-0 h-16 bg-white dark:bg-content-dark border-b border-border-light dark:border-border-dark flex items-center px-4 z-40 justify-between">
                <div className="flex items-center gap-2 text-primary">
                    <span className="material-symbols-outlined text-2xl">smart_toy</span>
                    <span className="font-bold text-lg">DocuAI</span>
                </div>
                <button onClick={() => setIsSidebarOpen(!isSidebarOpen)} className="p-2 text-subtext-light">
                    <span className="material-symbols-outlined">{isSidebarOpen ? 'close' : 'menu'}</span>
                </button>
            </div>

            {/* Mobile Overlay */}
            {isSidebarOpen && (
                <div
                    className="fixed inset-0 bg-black/50 z-40 md:hidden backdrop-blur-sm transition-opacity"
                    onClick={() => setIsSidebarOpen(false)}
                />
            )}

            {/* Sidebar */}
            <aside
                className={`
                    fixed inset-y-0 left-0 z-50 w-64 bg-white dark:bg-content-dark border-r border-border-light dark:border-border-dark flex flex-col shrink-0 transition-transform duration-300 ease-in-out md:static md:translate-x-0
                    ${isSidebarOpen ? 'translate-x-0' : '-translate-x-full'}
                `}
            >
                <div className="h-16 flex items-center px-6 border-b border-border-light dark:border-border-dark">
                    <div className="flex items-center gap-2.5 text-primary group cursor-pointer">
                        <div className="bg-blue-50 dark:bg-blue-900/30 p-1.5 rounded-lg group-hover:bg-blue-100 dark:group-hover:bg-blue-900/50 transition-colors">
                            <span className="material-symbols-outlined text-2xl">smart_toy</span>
                        </div>
                        <span className="text-xl font-bold tracking-tight text-gray-900 dark:text-white">DocuAI</span>
                    </div>
                </div>

                <div className="p-4 space-y-1 flex-1 overflow-y-auto">
                    <div className="text-xs font-semibold text-subtext-light dark:text-subtext-dark uppercase tracking-wider mb-2 px-2 mt-2">
                        Menu
                    </div>
                    {navItems.map((item) => (
                        <NavLink
                            key={item.path}
                            to={item.path}
                            className={({ isActive }) => `
                                flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all font-medium text-sm
                                ${isActive
                                    ? 'bg-blue-50 text-primary dark:bg-blue-900/20 dark:text-blue-400'
                                    : 'text-subtext-light dark:text-subtext-dark hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-slate-900 dark:hover:text-white'
                                }
                            `}
                        >
                            <span className="material-symbols-outlined text-[20px]">{item.icon}</span>
                            {item.label}
                        </NavLink>
                    ))}


                </div>

                <div className="p-4 border-t border-border-light dark:border-border-dark">
                    <button className="flex items-center gap-3 w-full p-2 rounded-xl hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left">
                        <div className="w-8 h-8 rounded-full bg-gradient-to-br from-purple-500 to-indigo-500 flex items-center justify-center text-white text-xs font-bold">
                            U
                        </div>
                        <div className="flex-1 min-w-0">
                            <p className="text-sm font-medium text-slate-900 dark:text-white truncate">User</p>
                            <p className="text-xs text-subtext-light dark:text-subtext-dark truncate">user@example.com</p>
                        </div>
                        <span className="material-symbols-outlined text-subtext-light">settings</span>
                    </button>
                </div>
            </aside>

            {/* Main Content Area */}
            <main className="flex-1 flex flex-col relative overflow-hidden pt-16 md:pt-0">
                <Outlet />
            </main>
        </div>
    );
};

export default MainLayout;
