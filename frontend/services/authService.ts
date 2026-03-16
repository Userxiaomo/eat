import { apiClient } from './apiClient';

// ========== Types ==========
export interface LoginRequest {
    username: string;
    password: string;
}

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
}

export interface AuthResponse {
    token: string; // 后端返回的是 token 不是 accessToken
    refreshToken: string;
    expiresAt: string; // 后端返回的是 expiresAt 不是 expiresIn
    user: {
        id: string;
        username: string;
        email: string;
        createdAt: string;
    };
}

export interface UserProfile {
    id: string;
    username: string;
    email: string;
    role: string;
    currentMonthTotalTokens: number;
    groupId?: string;
    createdAt: string;
}

//========== Auth Service ==========
export const authService = {
    /**
     * User login
     */
    async login(request: LoginRequest): Promise<AuthResponse> {
        const response = await apiClient.request<AuthResponse>('/auth/login', {
            method: 'POST',
            body: JSON.stringify(request),
        });

        // Store tokens - 使用正确的字段名
        apiClient.setTokens(response.token, response.refreshToken);

        return response;
    },

    /**
     * User registration
     */
    async register(request: RegisterRequest): Promise<AuthResponse> {
        const response = await apiClient.request<AuthResponse>('/auth/register', {
            method: 'POST',
            body: JSON.stringify(request),
        });

        // Store tokens - 使用正确的字段名
        apiClient.setTokens(response.token, response.refreshToken);

        return response;
    },

    /**
     * Logout (clear local tokens)
     */
    logout() {
        apiClient.clearTokens();
    },

    /**
     * Get current user profile
     */
    async getCurrentUser(): Promise<UserProfile> {
        return apiClient.request<UserProfile>('/auth/me');
    },

    /**
     * Check if user is authenticated
     */
    isAuthenticated(): boolean {
        return apiClient.getToken() !== null;
    },
};
