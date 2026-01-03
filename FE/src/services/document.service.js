import api from './api';

export const documentService = {
    // Upload a document to a specific session
    uploadDocument: async (sessionId, file) => {
        const formData = new FormData();
        formData.append('File', file); // Uppercase 'File' to match DTO usually, or check DTO. Standard is often case-insensitive or 'file'.
        // If DTO is UploadDocumentDto { IFormFile File { get; set; } }

        try {
            const response = await api.post(`/api/Document/sessions/${sessionId}/upload`, formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            });
            return response.data;
        } catch (error) {
            console.error("Error uploading document:", error);
            throw error;
        }
    },

    // Add other document related methods here if needed (delete, list, etc.)
};
