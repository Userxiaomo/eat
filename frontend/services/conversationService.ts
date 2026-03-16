import { apiClient } from './apiClient';

// ========== Types ==========
export interface ConversationDto {
    id: string;
    title: string;
    modelId: string;
    temperature: number;
    maxTokens: number | null;
    systemPrompt: string | null;
    createdAt: string;
    updatedAt: string;
    messageCount: number;
}

export interface MessageDto {
    id: string;
    role: string; // 'User' | 'Assistant'
    content: string;
    createdAt: string;
    inputTokens?: number | null;
    outputTokens?: number | null;
    reasoningContent?: string | null;
}

export interface CreateConversationRequest {
    title: string;
    modelId: string;
    temperature?: number;
    maxTokens?: number;
    systemPrompt?: string;
}

export interface SendMessageRequest {
    content: string;
    images?: string[]; // Base64 encoded images
}

// ========== Conversation Service ==========
export const conversationService = {
    /**
     * Get all user conversations
     */
    async getUserConversations(): Promise<ConversationDto[]> {
        return apiClient.request<ConversationDto[]>('/conversations');
    },

    /**
     * Get conversation by ID with messages
     */
    async getConversation(id: string): Promise<ConversationDto> {
        return apiClient.request(`/conversations/${id}`);
    },

    /**
     * Get conversation messages
     */
    async getConversationMessages(id: string): Promise<MessageDto[]> {
        return apiClient.request<MessageDto[]>(`/conversations/${id}/messages`);
    },

    /**
     * Create new conversation
     */
    async createConversation(request: CreateConversationRequest): Promise<ConversationDto> {
        return apiClient.request<ConversationDto>('/conversations', {
            method: 'POST',
            body: JSON.stringify(request),
        });
    },

    /**
     * Update conversation settings
     */
    async updateConversation(id: string, updates: Partial<CreateConversationRequest>): Promise<ConversationDto> {
        return apiClient.request<ConversationDto>(`/conversations/${id}`, {
            method: 'PUT',
            body: JSON.stringify(updates),
        });
    },

    /**
     * Delete conversation
     */
    async deleteConversation(id: string): Promise<void> {
        return apiClient.request(`/conversations/${id}`, {
            method: 'DELETE',
        });
    },

    /**
     * Send message and get streaming response
     */
    async sendMessageStream(
        conversationId: string,
        request: SendMessageRequest,
        onChunk: (delta: string) => void,
        onDone: () => void
    ): Promise<void> {
        const stream = await apiClient.streamRequest(`/conversations/${conversationId}/messages/stream`, {
            method: 'POST',
            body: JSON.stringify(request),
        });

        if (!stream) {
            throw new Error('Failed to get stream');
        }

        const reader = stream.getReader();
        const decoder = new TextDecoder();

        try {
            while (true) {
                const { done, value } = await reader.read();
                if (done) {
                    onDone();
                    break;
                }

                const chunk = decoder.decode(value, { stream: true });
                const lines = chunk.split('\n\n');

                for (const line of lines) {
                    if (line.startsWith('data: ')) {
                        try {
                            const data = JSON.parse(line.slice(6));
                            if (data.delta) {
                                onChunk(data.delta);
                            }
                            if (data.done) {
                                onDone();
                                return;
                            }
                        } catch (e) {
                            console.warn('Failed to parse SSE data:', e);
                        }
                    }
                }
            }
        } finally {
            reader.releaseLock();
        }
    },
};
