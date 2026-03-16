import { apiClient } from './apiClient';

// ========== Types ==========
export interface ModelDto {
    id: string;
    name: string;
    description?: string;
    provider?: string;
    contextWindow?: number;
    isEnabled: boolean;
}

// ========== Model Service ==========
export const modelService = {
    /**
     * Get all available models
     */
    async getAvailableModels(): Promise<ModelDto[]> {
        return apiClient.request<ModelDto[]>('/availablemodels');
    },

    /**
     * Get model details
     */
    async getModel(id: string): Promise<ModelDto> {
        return apiClient.request<ModelDto>(`/availablemodels/${id}`);
    },
};
