import React, { useEffect, useState } from 'react';
import { adminService } from '../services/admin.service';
import Toast from '../components/Toast';

const AdminDashboard = () => {
    const [stats, setStats] = useState(null);
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [toast, setToast] = useState(null);

    useEffect(() => {
        fetchData();
    }, []);

    const fetchData = async () => {
        setLoading(true);
        try {
            const [statsRes, usersRes] = await Promise.all([
                adminService.getDashboardStats(),
                adminService.getAllUsers()
            ]);

            if (statsRes.success) setStats(statsRes.data);
            if (usersRes.success) setUsers(usersRes.data);
        } catch (error) {
            setToast({ message: "Failed to load admin data", type: "error" });
        } finally {
            setLoading(false);
        }
    };

    const handleLockToggle = async (user) => {
        try {
            if (user.isLocked) {
                await adminService.unlockUser(user.id);
                setToast({ message: `Unlocked user ${user.fullName}`, type: "success" });
            } else {
                if (!window.confirm(`Are you sure you want to lock ${user.fullName}?`)) return;
                await adminService.lockUser(user.id);
                setToast({ message: `Locked user ${user.fullName}`, type: "success" });
            }
            fetchData(); // Refresh list - Optimize later to just update local state
        } catch (error) {
            setToast({ message: "Action failed", type: "error" });
        }
    };

    const handleDelete = async (userId) => {
        if (!window.confirm("WARNING: This will delete the user and ALL their documents/chats permanently. This cannot be undone. \n\nAre you sure completely?")) return;

        try {
            await adminService.deleteUser(userId);
            setToast({ message: "User deleted successfully", type: "success" });
            fetchData();
        } catch (error) {
            setToast({ message: "Failed to delete user", type: "error" });
        }
    };

    const formatBytes = (bytes) => {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    if (loading) {
        return (
            <div className="flex h-full items-center justify-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
            </div>
        );
    }

    return (
        <div className="p-6 md:p-8 max-w-7xl mx-auto w-full">
            {toast && <Toast message={toast.message} type={toast.type} onClose={() => setToast(null)} />}

            <div className="flex flex-col gap-8">
                {/* Header */}
                <div>
                    <h1 className="text-3xl font-bold text-slate-900 dark:text-white tracking-tight">Admin Dashboard</h1>
                    <p className="text-slate-500 dark:text-gray-400 mt-2">Overview of system performance and user management.</p>
                </div>

                {/* Stats Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                    <StatCard
                        title="Total Users"
                        value={stats?.totalUsers || 0}
                        icon="group"
                        color="bg-blue-500"
                    />
                    <StatCard
                        title="Total Documents"
                        value={stats?.totalDocuments || 0}
                        icon="description"
                        color="bg-amber-500"
                    />
                    <StatCard
                        title="Total Chats"
                        value={stats?.totalChatSessions || 0}
                        icon="chat"
                        color="bg-purple-500"
                    />
                    <StatCard
                        title="Storage Used"
                        value={formatBytes(stats?.totalStorageUsed || 0)}
                        icon="cloud"
                        color="bg-green-500"
                    />
                </div>

                {/* User List */}
                <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
                    <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center">
                        <h2 className="text-lg font-bold text-slate-900 dark:text-white">User Management</h2>
                        <span className="text-xs text-slate-500 bg-slate-100 dark:bg-gray-800 px-2 py-1 rounded-full">{users.length} Users</span>
                    </div>

                    <div className="overflow-x-auto">
                        <table className="w-full text-left">
                            <thead className="bg-gray-50 dark:bg-gray-800/50">
                                <tr>
                                    <th className="px-6 py-3 text-xs font-semibold text-slate-500 dark:text-gray-400 uppercase tracking-wider">User</th>
                                    <th className="px-6 py-3 text-xs font-semibold text-slate-500 dark:text-gray-400 uppercase tracking-wider">Role</th>
                                    <th className="px-6 py-3 text-xs font-semibold text-slate-500 dark:text-gray-400 uppercase tracking-wider">Usage</th>
                                    <th className="px-6 py-3 text-xs font-semibold text-slate-500 dark:text-gray-400 uppercase tracking-wider">Status</th>
                                    <th className="px-6 py-3 text-xs font-semibold text-slate-500 dark:text-gray-400 uppercase tracking-wider text-right">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-200 dark:divide-gray-800">
                                {users.map(user => (
                                    <tr key={user.id} className="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="flex flex-col">
                                                <span className="text-sm font-medium text-slate-900 dark:text-white">{user.fullName}</span>
                                                <span className="text-xs text-slate-500 dark:text-gray-400">{user.email}</span>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className={`px-2 py-1 text-xs font-bold rounded-full ${user.role === 'Admin' ? 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300' : 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300'}`}>
                                                {user.role}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="text-xs text-slate-500 dark:text-gray-400">
                                                <div><span className="font-semibold text-slate-700 dark:text-gray-300">{user.documentCount}</span> Docs</div>
                                                <div><span className="font-semibold text-slate-700 dark:text-gray-300">{user.chatCount}</span> Chats</div>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            {user.isLocked ? (
                                                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300">
                                                    Locked
                                                </span>
                                            ) : (
                                                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300">
                                                    Active
                                                </span>
                                            )}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                            <div className="flex items-center justify-end gap-2">
                                                <button
                                                    onClick={() => handleLockToggle(user)}
                                                    className={`p-2 rounded-lg transition-colors ${user.isLocked
                                                        ? 'text-green-600 hover:bg-green-50 dark:hover:bg-green-900/20'
                                                        : 'text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'}`}
                                                    title={user.isLocked ? "Unlock User" : "Lock User"}
                                                >
                                                    <span className="material-symbols-outlined text-lg">{user.isLocked ? 'lock_open' : 'lock'}</span>
                                                </button>
                                                <button
                                                    onClick={() => handleDelete(user.id)}
                                                    className="p-2 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors"
                                                    title="Delete User"
                                                >
                                                    <span className="material-symbols-outlined text-lg">delete</span>
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    );
};

const StatCard = ({ title, value, icon, color }) => (
    <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 p-6 rounded-xl flex items-center justify-between shadow-sm">
        <div>
            <p className="text-sm font-medium text-slate-500 dark:text-gray-400">{title}</p>
            <p className="text-2xl font-bold text-slate-900 dark:text-white mt-1">{value}</p>
        </div>
        <div className={`size-12 rounded-lg ${color} flex items-center justify-center text-white shadow-md`}>
            <span className="material-symbols-outlined text-2xl">{icon}</span>
        </div>
    </div>
);

export default AdminDashboard;
