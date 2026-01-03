import api from "./api";

export const chatService = {
  createChat: async (data) => {
    // data: { title: string, documentIds: number[] }
    const response = await api.post("/api/Chat/create-chat", data);
    return response.data;
  },
  getSessions: async () => {
    const response = await api.get("/api/Chat/sessions");
    return response.data;
  },
  getSession: async (sessionId) => {
    const response = await api.get(`/api/Chat/sessions/${sessionId}`);
    return response.data;
  },
  getMessages: async (sessionId) => {
    const response = await api.get(`/api/Chat/sessions/${sessionId}/messages`);
    return response.data;
  },
  sendMessage: async (sessionId, data) => {
    // data: { content: string }
    const response = await api.post(
      `/api/Chat/sessions/${sessionId}/messages`,
      data
    );
    return response.data;
  },
};
