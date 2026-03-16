export enum Role {
  USER = 'user',
  MODEL = 'model'
}

export interface Message {
  id: string;
  role: Role;
  content: string;
  timestamp: number;
  images?: string[]; // User uploaded images
  generatedImages?: string[]; // Model generated images
}

export interface ChatSession {
  id: string;
  title: string;
  messages: Message[];
  createdAt: number;
  updatedAt: number;
  model: string;
}

export interface Model {
  id: string;
  name: string;
  description?: string;
  isCustom?: boolean; 
  enabled?: boolean; // New: Toggle visibility
  contextWindow?: number; // Optional: Display context window size
}

export interface ModelConfig {
  modelId: string;
  temperature: number;
  topP: number;
  topK: number;
  systemInstruction?: string;
  enableSearch?: boolean;
}

export interface AppSettings {
  apiKey: string;
  baseUrl?: string; // New: Proxy URL support
  customModels: Model[];
}

export type ChatState = {
  sessions: ChatSession[];
  activeSessionId: string | null;
  isSidebarOpen: boolean;
};