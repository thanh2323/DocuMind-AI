import api from './api';

export const userService = {
    getProfile: async () => {
        try {
            const response = await api.get('/api/User/profile');
            return response.data;
        } catch (error) {
            console.error("Error fetching user profile:", error);
            throw error;
        }
    },
    updateProfile: async (data) => {
        try {
            const response = await api.put('/api/User/profile', data);
            return response.data;
        } catch (error) {
            console.error("Error updating user profile:", error);
            throw error;
        }
    }
};
