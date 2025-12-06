import React, { useEffect, useState } from 'react';
import { authService } from '../services/auth.service';
import { dashboardService } from '../services/dashboard.service';
import { useNavigate } from 'react-router-dom';

const DashboardPage = () => {
    const navigate = useNavigate();
    const [dashboardData, setDashboardData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Helper to logout (temporary placement, maybe move to header later)
    const handleLogout = () => {
        authService.logout();
        navigate('/');
    };

    useEffect(() => {
        const fetchDashboard = async () => {
            try {
                const response = await dashboardService.getDashboard();
                if (response && response.success) {
                    setDashboardData(response.data);
                } else {

                    setError('Failed to load dashboard data.');
                }
            } catch (err) {
                console.error("Dashboard fetch error:", err);
                // If 401, redirect to login might happen automatically via interceptor, but good to handle
                setError('Failed to load data. Please try refreshing.');
            } finally {
                setLoading(false);
            }
        };

        fetchDashboard();
    }, []);

    if (loading) {
        return (
            <div className="flex h-screen items-center justify-center bg-background-light dark:bg-background-dark text-slate-900 dark:text-white">
                <p>Loading dashboard...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex h-screen flex-col items-center justify-center bg-background-light dark:bg-background-dark text-slate-900 dark:text-white gap-4">
                <p className="text-red-500">{error}</p>
                <button onClick={() => window.location.reload()} className="px-4 py-2 bg-primary text-white rounded">Retry</button>
            </div>
        );
    }

    const { statistics, recentDocuments, recentChats } = dashboardData || {};

    return (
        <div className="relative flex h-auto min-h-screen w-full flex-col group/design-root overflow-x-hidden text-[#0d121b] dark:text-gray-200 bg-background-light dark:bg-background-dark font-display">
            <div className="layout-container flex h-full grow flex-col">
                <header className="w-full bg-background-light dark:bg-background-dark/80 backdrop-blur-sm sticky top-0 z-10">
                    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                        <div className="flex items-center justify-between whitespace-nowrap border-b border-solid border-gray-200 dark:border-gray-800 h-16">
                            <div className="flex items-center gap-4 text-slate-900 dark:text-white">
                                <div className="size-6 text-primary">
                                    <svg fill="none" viewBox="0 0 48 48" xmlns="http://www.w3.org/2000/svg">
                                        <path d="M6 6H42L36 24L42 42H6L12 24L6 6Z" fill="currentColor"></path>
                                    </svg>
                                </div>
                                <h2 className="text-lg font-bold leading-tight tracking-[-0.015em]">DocuMind AI</h2>
                            </div>
                            <div className="flex items-center justify-end gap-3">
                                <button className="hidden sm:flex min-w-[84px] max-w-[480px] cursor-pointer items-center justify-center overflow-hidden rounded-lg h-10 px-4 bg-primary text-white text-sm font-bold leading-normal tracking-[0.015em] hover:bg-primary/90 transition-colors">
                                    <span className="truncate">Upload Document</span>
                                </button>
                                <button className="flex max-w-[480px] cursor-pointer items-center justify-center overflow-hidden rounded-lg h-10 bg-gray-200/60 dark:bg-gray-800 text-slate-800 dark:text-gray-300 gap-2 text-sm font-bold leading-normal tracking-[0.015em] min-w-0 px-2.5 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors">
                                    <span className="material-symbols-outlined text-xl">notifications</span>
                                </button>
                                <div
                                    className="bg-center bg-no-repeat aspect-square bg-cover rounded-full size-10 cursor-pointer"
                                    onClick={handleLogout}
                                    style={{ backgroundImage: 'url("https://lh3.googleusercontent.com/aida-public/AB6AXuB7w_ESzLFBjDfoW_p1QTzdH-lV8cifvhDkpyYDjmusmznGvelF8FS0YcVm397miZeGWMaKmT2P85uTH-20KWl4Dp-g-PdccUtKj60EZm1z-RWSe3CeJ_mAGX_hpIsat4URwf8aKXK9Yly9YaryWKCsn_6ziM_exALQz4ZGu4lmXs4bMvdPjYR5Erui17PDFXwyudss_6Sbn4mCmBdw1Rn4T0hPgyMNWaWuBnrmybW3_KIunUa258BuL7BNjMG-HcrHyXTVqgU0YlY")' }}
                                ></div>
                            </div>
                        </div>
                    </div>
                </header>
                <main className="flex-1 w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                    <div className="flex flex-wrap justify-between items-center gap-4 mb-8">
                        <h1 className="text-slate-900 dark:text-white text-4xl font-black leading-tight tracking-[-0.033em] min-w-72">Dashboard</h1>
                        <button className="sm:hidden flex min-w-[84px] max-w-[480px] cursor-pointer items-center justify-center overflow-hidden rounded-lg h-12 px-5 bg-primary text-white text-base font-bold leading-normal tracking-[0.015em] hover:bg-primary/90 transition-colors w-full">
                            <span className="truncate">Upload New Document</span>
                        </button>
                    </div>
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
                        <div className="flex flex-col gap-2 rounded-xl p-6 border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900/50">
                            <div className="flex items-center justify-between">
                                <p className="text-base font-medium leading-normal text-slate-600 dark:text-gray-400">Total Documents</p>
                                <span className="material-symbols-outlined text-slate-400 dark:text-gray-500">description</span>
                            </div>
                            <p className="text-slate-900 dark:text-white tracking-light text-3xl font-bold leading-tight">{statistics?.totalDocuments || 0}</p>
                        </div>
                        <div className="flex flex-col gap-2 rounded-xl p-6 border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900/50">
                            <div className="flex items-center justify-between">
                                <p className="text-base font-medium leading-normal text-slate-600 dark:text-gray-400">Total Chats</p>
                                <span className="material-symbols-outlined text-slate-400 dark:text-gray-500">chat</span>
                            </div>
                            <p className="text-slate-900 dark:text-white tracking-light text-3xl font-bold leading-tight">{statistics?.totalChats || 0}</p>
                        </div>
                        <div className="flex flex-col gap-2 rounded-xl p-6 border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900/50">
                            <div className="flex items-center justify-between">
                                <p className="text-base font-medium leading-normal text-slate-600 dark:text-gray-400">Document Status</p>
                                <span className="material-symbols-outlined text-slate-400 dark:text-gray-500">sync_problem</span>
                            </div>
                            <div className="flex gap-4 items-center mt-2">
                                <div className="flex items-center gap-2">
                                    <div className="size-2 rounded-full bg-green-500"></div>
                                    <span className="text-sm font-medium text-slate-700 dark:text-gray-300">Ready: {statistics?.statusCounts?.Ready || 0}</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <div className="size-2 rounded-full bg-yellow-500"></div>
                                    <span className="text-sm font-medium text-slate-700 dark:text-gray-300">Pending: {statistics?.statusCounts?.Pending || 0}</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <div className="size-2 rounded-full bg-red-500"></div>
                                    <span className="text-sm font-medium text-slate-700 dark:text-gray-300">Error: {statistics?.statusCounts?.Error || 0}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                        <div className="lg:col-span-2 bg-white dark:bg-gray-900/50 border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden">
                            <div className="p-6">
                                <div className="flex flex-col md:flex-row gap-4 justify-between items-center">
                                    <h2 className="text-slate-900 dark:text-white text-2xl font-bold leading-tight tracking-[-0.015em]">Recent Documents</h2>
                                    <div className="w-full md:w-auto md:max-w-xs">
                                        <label className="flex flex-col w-full">
                                            <div className="flex w-full flex-1 items-stretch rounded-lg h-11">
                                                <div className="text-slate-500 dark:text-gray-400 flex border-none bg-gray-100 dark:bg-gray-800 items-center justify-center pl-4 rounded-l-lg">
                                                    <span className="material-symbols-outlined text-xl">search</span>
                                                </div>
                                                <input className="form-input flex w-full min-w-0 flex-1 resize-none overflow-hidden rounded-lg text-slate-900 dark:text-white focus:outline-0 focus:ring-2 focus:ring-primary/50 border-none bg-gray-100 dark:bg-gray-800 h-full placeholder:text-slate-500 dark:placeholder:text-gray-500 px-4 rounded-l-none border-l-0 pl-2 text-sm font-normal leading-normal" placeholder="Search documents..." defaultValue="" />
                                            </div>
                                        </label>
                                    </div>
                                </div>
                            </div>
                            <div className="overflow-x-auto">
                                <table className="w-full text-left">
                                    <thead className="bg-gray-50 dark:bg-gray-900 border-b border-t border-gray-200 dark:border-gray-800">
                                        <tr>
                                            <th className="px-6 py-3 text-xs font-semibold text-slate-600 dark:text-gray-400 uppercase tracking-wider" scope="col">Document Name</th>
                                            <th className="px-6 py-3 text-xs font-semibold text-slate-600 dark:text-gray-400 uppercase tracking-wider" scope="col">Status</th>
                                            <th className="px-6 py-3 text-xs font-semibold text-slate-600 dark:text-gray-400 uppercase tracking-wider hidden md:table-cell" scope="col">Creation Date</th>
                                            <th className="px-6 py-3 text-xs font-semibold text-slate-600 dark:text-gray-400 uppercase tracking-wider text-right" scope="col"></th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-200 dark:divide-gray-800">
                                        {recentDocuments && recentDocuments.map((doc) => (
                                            <tr key={doc.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-slate-800 dark:text-gray-200">{doc.fileName}</td>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-slate-500 dark:text-gray-400">
                                                    <div className="flex items-center gap-2">
                                                        <div className={`size-2 rounded-full ${doc.status === 'Ready' ? 'bg-green-500' :
                                                            doc.status === 'Pending' ? 'bg-yellow-500' :
                                                                doc.status === 'Error' ? 'bg-red-500' : 'bg-gray-500'
                                                            }`}></div>
                                                        <span>{doc.status}</span>
                                                    </div>
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-sm text-slate-500 dark:text-gray-400 hidden md:table-cell">
                                                    {new Date(doc.createdAt).toLocaleDateString()}
                                                </td>
                                                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                                    <a className="text-primary hover:text-primary/80" href="#">View Details</a>
                                                </td>
                                            </tr>
                                        ))}
                                        {(!recentDocuments || recentDocuments.length === 0) && (
                                            <tr>
                                                <td colSpan="4" className="px-6 py-4 text-center text-sm text-gray-500">No recent documents found.</td>
                                            </tr>
                                        )}
                                    </tbody>
                                </table>
                            </div>
                            <div className="px-6 py-4 border-t border-gray-200 dark:border-gray-800 flex items-center justify-between">
                                <span className="text-sm text-slate-600 dark:text-gray-400">Showing {recentDocuments?.length || 0} results</span>
                                {/* Pagination controls can be added here later */}
                            </div>
                        </div>
                        <div className="lg:col-span-1">
                            <div className="bg-white dark:bg-gray-900/50 border border-gray-200 dark:border-gray-800 rounded-xl">
                                <div className="p-6 border-b border-gray-200 dark:border-gray-800">
                                    <h2 className="text-slate-900 dark:text-white text-2xl font-bold leading-tight tracking-[-0.015em]">Recent Chat Sessions</h2>
                                </div>
                                <div className="p-6 flex flex-col gap-4">
                                    {recentChats && recentChats.map((chat) => (
                                        <div key={chat.id} className="flex justify-between items-center gap-4">
                                            <div className="flex flex-col">
                                                <a className="text-sm font-medium text-slate-800 dark:text-gray-200 hover:text-primary dark:hover:text-primary/90 transition-colors" href="#">{chat.title}</a>
                                                <p className="text-xs text-slate-500 dark:text-gray-400">{new Date(chat.lastActiveAt).toLocaleString()}</p>
                                            </div>
                                            <button className="flex items-center justify-center h-8 w-8 text-slate-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg">
                                                <span className="material-symbols-outlined text-xl">chevron_right</span>
                                            </button>
                                        </div>
                                    ))}
                                    {(!recentChats || recentChats.length === 0) && (
                                        <p className="text-sm text-gray-500 text-center">No recent chats.</p>
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>
                </main>
            </div>
        </div>
    );
};

export default DashboardPage;
