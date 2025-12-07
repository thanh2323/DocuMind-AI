import api from './api';


export const dashboardService = {
    getDashboard: async () => {
        try {
            const response = await api.get('/api/DashBoard/dashboard');
            return response.data;
        } catch (error) {
            if (error.response && error.response.data) {
                throw error.response.data;
            }
            throw error;
        }
    }
};
