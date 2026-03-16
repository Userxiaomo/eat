import { apiClient } from './apiClient';

// ========== Types ==========
export enum ProviderEnum {
    OpenAI = 'OpenAI',
    Claude = 'Claude',
    Gemini = 'Gemini',
    Custom = 'Custom',
}

export enum ModelTypeEnum {
    Chat = 'Chat',
    Image = 'Image',
    Embedding = 'Embedding',
}

export interface ChannelDto {
    id: string;
    name: string;
    provider: ProviderEnum | string;
    apiKey?: string;
    baseUrl: string;
    isEnabled: boolean;
    modelCount?: number;
    priority?: number;
    weight?: number;
    maxRetries?: number;
    isHealthy: boolean;
    createdAt: string;
}

export interface CreateChannelRequest {
    name: string;
    provider: ProviderEnum | string;
    apiKey: string;
    baseUrl?: string;
}

export interface UpdateChannelRequest {
    name?: string;
    provider?: ProviderEnum | string;
    apiKey?: string;
    baseUrl?: string;
    isEnabled?: boolean;
    priority?: number;
    weight?: number;
    maxRetries?: number;
}

export interface AiModelDto {
    id: string;
    channelId: string;
    channelName: string;
    name: string;
    modelId: string;
    modelType: string;
    maxTokens: number;
    inputPrice: number;
    outputPrice: number;
    isEnabled: boolean;
    sortOrder: number;
}

export interface CreateModelRequest {
    channelId: string;
    name: string;
    modelId: string;
    modelType: ModelTypeEnum | string;
    maxTokens?: number;
    inputPrice?: number;
    outputPrice?: number;
    sortOrder?: number;
}

export interface UpdateModelRequest {
    name?: string;
    modelId?: string;
    modelType?: ModelTypeEnum | string;
    maxTokens?: number;
    inputPrice?: number;
    outputPrice?: number;
    isEnabled?: boolean;
    sortOrder?: number;
}

// ========== Channel Service ==========
export const channelService = {
    /**
     * Get all channels (Admin only)
     */
    async getAllChannels(): Promise<ChannelDto[]> {
        return apiClient.request<ChannelDto[]>('/admin/channels');
    },

    /**
     * Get channel by ID
     */
    async getChannel(id: string): Promise<ChannelDto> {
        return apiClient.request<ChannelDto>(`/admin/channels/${id}`);
    },

    /**
     * Create new channel
     */
    async createChannel(request: CreateChannelRequest): Promise<ChannelDto> {
        return apiClient.request<ChannelDto>('/admin/channels', {
            method: 'POST',
            body: JSON.stringify(request),
        });
    },

    /**
     * Update channel
     */
    async updateChannel(id: string, request: UpdateChannelRequest): Promise<ChannelDto> {
        return apiClient.request<ChannelDto>(`/admin/channels/${id}`, {
            method: 'PUT',
            body: JSON.stringify(request),
        });
    },

    /**
     * Delete channel
     */
    async deleteChannel(id: string): Promise<void> {
        return apiClient.request<void>(`/admin/channels/${id}`, {
            method: 'DELETE',
        });
    },

    /**
     * Get models by channel
     */
    async getModelsByChannel(channelId: string): Promise<AiModelDto[]> {
        return apiClient.request<AiModelDto[]>(`/admin/models/by-channel/${channelId}`);
    },

    /**
     * Create model
     */
    async createModel(request: CreateModelRequest): Promise<AiModelDto> {
        return apiClient.request<AiModelDto>('/admin/models', {
            method: 'POST',
            body: JSON.stringify(request),
        });
    },

    /**
     * Update model
     */
    async updateModel(id: string, request: UpdateModelRequest): Promise<AiModelDto> {
        return apiClient.request<AiModelDto>(`/admin/models/${id}`, {
            method: 'PUT',
            body: JSON.stringify(request),
        });
    },

    /**
     * Delete model
     */
    async deleteModel(id: string): Promise<void> {
        return apiClient.request<void>(`/admin/models/${id}`, {
            method: 'DELETE',
        });
    },

    /**
     * Recover channel health
     */
    async recoverChannelHealth(id: string): Promise<void> {
        return apiClient.request<void>(`/admin/channels/${id}/recover-health`, {
            method: 'POST',
        });
    },
};
