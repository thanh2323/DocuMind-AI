import api from './api';


export const authService = {
    login: async (email, password) => {
        try {
            const response = await api.post('/api/Auth/login', { email, password });
            if (response.data?.data?.token) {
                localStorage.setItem('token', response.data.data.token);
                localStorage.setItem('user', JSON.stringify(response.data.data));
            }
            return response.data;
        } catch (error) {
            if (error.response && error.response.data) {
                throw error.response.data;
            }
            throw error;
        }
    },
    register: async (fullName, email, password, confirmPassword) => {
        try {
            const response = await api.post('/api/Auth/register', {
                fullName,
                email,
                password,
                confirmPassword
            });
            if (response.data?.data?.token) {
                localStorage.setItem('token', response.data.data.token);
                localStorage.setItem('user', JSON.stringify(response.data.data));
            }
            return response.data;
        } catch (error) {
            if (error.response && error.response.data) {
                throw error.response.data;
            }
            throw error;
        }
    },
    logout: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
    },
    getCurrentUser: () => {
        const user = localStorage.getItem('user');
        return user ? JSON.parse(user) : null;
    }
};
