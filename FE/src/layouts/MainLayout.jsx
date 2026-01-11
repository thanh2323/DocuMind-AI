import React, { useState, useEffect } from 'react';
import { Outlet, NavLink, useLocation } from 'react-router-dom';
import { userService } from '../services/user.service';
import { authService } from '../services/auth.service';

const MainLayout = () => {
    const [isSidebarOpen, setIsSidebarOpen] = useState(false);
    const [user, setUser] = useState(authService.getCurrentUser());
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [activeTab, setActiveTab] = useState('profile'); // 'profile' or 'password'
    const [editFormData, setEditFormData] = useState({ fullName: '' });
    const [passwordFormData, setPasswordFormData] = useState({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
    const location = useLocation();

    // Close sidebar on route change (mobile)
    useEffect(() => {
        setIsSidebarOpen(false);
    }, [location]);

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const response = await userService.getProfile();
                if (response.success) {
                    setUser(response.data);
                    // Update localStorage as well to keep it in sync
                    const storedUser = authService.getCurrentUser();
                    if (storedUser) {
                        localStorage.setItem('user', JSON.stringify({ ...storedUser, ...response.data }));
                    }
                }
            } catch (error) {
                console.error("Failed to fetch user profile", error);
            }
        };
        fetchProfile();
    }, []);

    const handleOpenModal = () => {
        setEditFormData({ fullName: user?.fullName || '' });
        setPasswordFormData({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
        setActiveTab('profile'); // Default to profile
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
    };

    const handleUpdateProfile = async (e) => {
        e.preventDefault();
        try {
            const response = await userService.updateProfile(editFormData);
            if (response.success) {
                const updatedUser = { ...user, ...response.data };
                setUser(updatedUser);
                // Update localStorage
                const storedUser = authService.getCurrentUser();
                if (storedUser) {
                    localStorage.setItem('user', JSON.stringify({ ...storedUser, ...response.data }));
                }
                alert("Profile updated successfully!");
                // Keep modal open or close? User preference usually close or show toggle. Let's keep distinct actions.
            }
        } catch (error) {
            console.error("Failed to update profile", error);
            alert("Failed to update profile");
        }
    };

    const handleChangePassword = async (e) => {
        e.preventDefault();
        if (passwordFormData.newPassword !== passwordFormData.confirmNewPassword) {
            alert("New passwords do not match!");
            return;
        }
        try {
            const response = await authService.changePassword(
                user.email,
                passwordFormData.currentPassword,
                passwordFormData.newPassword,
                passwordFormData.confirmNewPassword
            );
            if (response.success) {
                alert("Password changed successfully!");
                setPasswordFormData({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
            }
        } catch (error) {
            console.error("Failed to change password", error);
            alert(error.message || "Failed to change password");
        }
    };

    const handleLogout = () => {
        authService.logout();
        window.location.href = "/"; // Navigate to home/login
    };

    // Check for Admin Role
    const token = localStorage.getItem("token");
    let isAdmin = false;
    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            // Check for standard role claim (http://schemas.microsoft.com/ws/2008/06/identity/claims/role) 
            // OR simple "role" claim depending on what the backend sends now.
            // Backend was recently switched to ClaimTypes.Role, which usually maps to the long schema URL.
            const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload["role"];
            isAdmin = role === "Admin";
        } catch (e) {
            console.error("Error parsing token", e);
        }
    }

    const navItems = [
        { path: '/dashboard', label: 'Dashboard', icon: 'dashboard' },
        { path: '/library', label: 'My Library', icon: 'library_books' },
        // { path: '/chat', label: 'Recent Chat', icon: 'chat_bubble' }, // Optional
    ];

    if (isAdmin) {
        navItems.push({ path: '/admin', label: 'Admin Panel', icon: 'admin_panel_settings' });
    }

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
                    <button
                        onClick={handleOpenModal}
                        className="flex items-center gap-3 w-full p-2 rounded-xl hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors text-left"
                    >
                        <div className="w-8 h-8 rounded-full bg-gradient-to-br from-purple-500 to-indigo-500 flex items-center justify-center text-white text-xs font-bold">
                            {user?.fullName?.charAt(0).toUpperCase() || 'U'}
                        </div>
                        <div className="flex-1 min-w-0">
                            <p className="text-sm font-medium text-slate-900 dark:text-white truncate">{user?.fullName || 'User'}</p>
                            <p className="text-xs text-subtext-light dark:text-subtext-dark truncate">{user?.email || 'user@example.com'}</p>
                        </div>
                        <span className="material-symbols-outlined text-subtext-light">settings</span>
                    </button>
                </div>
            </aside>

            {/* Edit Profile Modal */}
            {isModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
                    <div className="bg-white dark:bg-gray-900 rounded-2xl w-full max-w-md shadow-xl border border-gray-200 dark:border-gray-800">
                        <div className="p-6 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center">
                            <h3 className="text-xl font-bold text-slate-900 dark:text-white">User Settings</h3>
                            <button onClick={handleCloseModal} className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200">
                                <span className="material-symbols-outlined">close</span>
                            </button>
                        </div>

                        {/* Tabs */}
                        <div className="flex border-b border-gray-200 dark:border-gray-800">
                            <button
                                className={`flex-1 py-3 text-sm font-medium transition-colors border-b-2 ${activeTab === 'profile' ? 'border-primary text-primary' : 'border-transparent text-slate-500 dark:text-gray-400 hover:text-slate-700 dark:hover:text-gray-200'}`}
                                onClick={() => setActiveTab('profile')}
                            >
                                Profile
                            </button>
                            <button
                                className={`flex-1 py-3 text-sm font-medium transition-colors border-b-2 ${activeTab === 'password' ? 'border-primary text-primary' : 'border-transparent text-slate-500 dark:text-gray-400 hover:text-slate-700 dark:hover:text-gray-200'}`}
                                onClick={() => setActiveTab('password')}
                            >
                                Password
                            </button>
                        </div>

                        {activeTab === 'profile' && (
                            <form onSubmit={handleUpdateProfile} className="p-6 space-y-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1">
                                        Full Name
                                    </label>
                                    <input
                                        type="text"
                                        value={editFormData.fullName}
                                        onChange={(e) => setEditFormData({ ...editFormData, fullName: e.target.value })}
                                        className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-primary/50 focus:border-primary outline-none transition-all"
                                        required
                                        minLength={2}
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1">
                                        Email
                                    </label>
                                    <input
                                        type="email"
                                        value={user?.email || ''}
                                        disabled
                                        className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-700 bg-gray-100 dark:bg-gray-800 text-slate-500 dark:text-gray-400 cursor-not-allowed"
                                    />
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1">
                                            Role
                                        </label>
                                        <input
                                            type="text"
                                            value={user?.role || 'User'}
                                            disabled
                                            className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-700 bg-gray-100 dark:bg-gray-800 text-slate-500 dark:text-gray-400 cursor-not-allowed"
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1">
                                            Joined
                                        </label>
                                        <input
                                            type="text"
                                            value={user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : 'N/A'}
                                            disabled
                                            className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-700 bg-gray-100 dark:bg-gray-800 text-slate-500 dark:text-gray-400 cursor-not-allowed"
                                        />
                                    </div>
                                </div>

                                <div className="pt-4 border-t border-gray-200 dark:border-gray-700 flex justify-between items-center">
                                    <button
                                        type="button"
                                        onClick={handleLogout}
                                        className="px-4 py-2 rounded-lg text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors font-medium flex items-center gap-2"
                                    >
                                        <span className="material-symbols-outlined">logout</span>
                                        Log Out
                                    </button>
                                    <div className="flex gap-3">
                                        <button
                                            type="button"
                                            onClick={handleCloseModal}
                                            className="px-4 py-2 rounded-lg text-slate-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors font-medium"
                                        >
                                            Cancel
                                        </button>
                                        <button
                                            type="submit"
                                            className="px-4 py-2 rounded-lg bg-primary text-white hover:bg-primary/90 transition-colors font-medium shadow-lg shadow-primary/20"
                                        >
                                            Save Changes
                                        </button>
                                    </div>
                                </div>
                            </form>
                        )}

                        {activeTab === 'password' && (
                            <form onSubmit={handleChangePassword} className="p-6 space-y-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1">
                                        Current Password
                                    </label>
                                    <input
                                        type="password"
                                        value={passwordFormData.currentPassword}
                                        onChange={(e) => setPasswordFormData({ ...passwordFormData, currentPassword: e.target.value })}
                                        className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-primary/50 focus:border-primary outline-none transition-all"
                                        required
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1">
                                        New Password
                                    </label>
                                    <input
                                        type="password"
                                        value={passwordFormData.newPassword}
                                        onChange={(e) => setPasswordFormData({ ...passwordFormData, newPassword: e.target.value })}
                                        className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-primary/50 focus:border-primary outline-none transition-all"
                                        required
                                        minLength={6}
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-700 dark:text-gray-300 mb-1">
                                        Confirm New Password
                                    </label>
                                    <input
                                        type="password"
                                        value={passwordFormData.confirmNewPassword}
                                        onChange={(e) => setPasswordFormData({ ...passwordFormData, confirmNewPassword: e.target.value })}
                                        className="w-full px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-slate-900 dark:text-white focus:ring-2 focus:ring-primary/50 focus:border-primary outline-none transition-all"
                                        required
                                        minLength={6}
                                    />
                                </div>
                                <div className="flex justify-end gap-3 mt-6">
                                    <button
                                        type="button"
                                        onClick={handleCloseModal}
                                        className="px-4 py-2 rounded-lg text-slate-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors font-medium"
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        type="submit"
                                        className="px-4 py-2 rounded-lg bg-primary text-white hover:bg-primary/90 transition-colors font-medium shadow-lg shadow-primary/20"
                                    >
                                        Change Password
                                    </button>
                                </div>
                            </form>
                        )}
                    </div>
                </div>
            )}

            {/* Main Content Area */}
            <main className="flex-1 flex flex-col relative overflow-hidden pt-16 md:pt-0">
                <Outlet />
            </main>
        </div>
    );
};

export default MainLayout;
