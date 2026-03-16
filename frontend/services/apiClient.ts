// ========== API Base Configuration ==========
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5005/api';

// ========== Types ==========
interface ApiErrorResponse {
    message: string;
    errors?: Record<string, string[]>;
}

// ========== API Client Class ==========
export class ApiClient {
    private token: string | null = null;
    private refreshToken: string | null = null;

    constructor() {
        // Load tokens from localStorage
        this.token = localStorage.getItem('auth_token');
        this.refreshToken = localStorage.getItem('refresh_token');
    }

    setTokens(accessToken: string, refreshToken: string) {
        this.token = accessToken;
        this.refreshToken = refreshToken;
        localStorage.setItem('auth_token', accessToken);
        localStorage.setItem('refresh_token', refreshToken);
    }

    clearTokens() {
        this.token = null;
        this.refreshToken = null;
        localStorage.removeItem('auth_token');
        localStorage.removeItem('refresh_token');
    }

    getToken(): string | null {
        return this.token;
    }

    async request<T = any>(endpoint: string, options: RequestInit = {}): Promise<T> {
        const headers: Record<string, string> = {
            'Content-Type': 'application/json',
            ...(options.headers as Record<string, string>),
        };

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, {
                ...options,
                headers,
            });

            // Handle 401 - try to refresh token
            if (response.status === 401 && this.refreshToken) {
                const newToken = await this.tryRefreshToken();
                if (newToken) {
                    // Retry the original request with new token
                    headers['Authorization'] = `Bearer ${newToken}`;
                    const retryResponse = await fetch(`${API_BASE_URL}${endpoint}`, {
                        ...options,
                        headers,
                    });
                    return this.handleResponse<T>(retryResponse);
                }
            }

            return this.handleResponse<T>(response);
        } catch (error) {
            console.error('API Request failed:', error);
            throw error;
        }
    }

    private async handleResponse<T>(response: Response): Promise<T> {
        if (!response.ok) {
            const errorData: ApiErrorResponse = await response.json().catch(() => ({
                message: response.statusText,
            }));
            throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
        }

        // Handle empty responses (204 No Content, etc.)
        if (response.status === 204 || response.headers.get('content-length') === '0') {
            return {} as T;
        }

        return response.json();
    }

    private async tryRefreshToken(): Promise<string | null> {
        if (!this.refreshToken) return null;

        try {
            const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ refreshToken: this.refreshToken }),
            });

            if (!response.ok) {
                this.clearTokens();
                return null;
            }

            const data = await response.json();
            this.setTokens(data.token, data.refreshToken); // 使用 token 而不是 accessToken
            return data.token;
        } catch {
            this.clearTokens();
            return null;
        }
    }

    // Streaming request for SSE
    async streamRequest(endpoint: string, options: RequestInit = {}): Promise<ReadableStream<Uint8Array> | null> {
        const headers: Record<string, string> = {
            'Content-Type': 'application/json',
            ...(options.headers as Record<string, string>),
        };

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...options,
            headers,
        });

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({ message: response.statusText }));
            throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
        }

        return response.body;
    }
}

// Export singleton instance
export const apiClient = new ApiClient();
