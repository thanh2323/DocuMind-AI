import api from "./api";

export const adminService = {
    getDashboardStats: async () => {
        const response = await api.get("/api/admin/dashboard");
        return response.data;
    },

    getAllUsers: async () => {
        const response = await api.get("/api/admin/users");
        return response.data;
    },

    deleteUser: async (userId) => {
        const response = await api.delete(`/api/admin/users/${userId}`);
        return response.data;
    },

    lockUser: async (userId) => {
        const response = await api.post(`/api/admin/users/${userId}/lock`);
        return response.data;
    },

    unlockUser: async (userId) => {
        const response = await api.post(`/api/admin/users/${userId}/unlock`);
        return response.data;
    }
};
