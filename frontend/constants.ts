export const DEFAULT_MODEL = 'gemini-3-flash-preview';

// A more comprehensive list similar to LobeChat's defaults
export const AVAILABLE_MODELS = [
  { 
    id: 'gemini-3-flash-preview', 
    name: 'Gemini 3 Flash (Preview)', 
    description: 'Fast, cost-efficient, high-volume', 
    enabled: true,
    contextWindow: 1048576 
  },
  { 
    id: 'gemini-3-pro-preview', 
    name: 'Gemini 3 Pro (Preview)', 
    description: 'Complex reasoning, coding, math', 
    enabled: true,
    contextWindow: 2097152 
  },
  {
    id: 'gemini-2.5-flash-image',
    name: 'Gemini 2.5 Flash Image',
    description: 'Text-to-image generation & editing',
    enabled: true,
    contextWindow: 32768
  },
  { 
    id: 'gemini-flash-lite-latest', 
    name: 'Gemini Flash Lite', 
    description: 'Cost effective, low latency', 
    enabled: false,
    contextWindow: 1048576
  },
  { 
    id: 'gemini-2.5-flash-native-audio-preview-12-2025', 
    name: 'Gemini 2.5 Flash Audio', 
    description: 'Real-time audio & video conversation', 
    enabled: false,
    contextWindow: 1048576
  },
];

export const DEFAULT_SYSTEM_INSTRUCTION = `You are an intelligent, helpful, and professional AI assistant. 
Response Format:
- Use Markdown for formatting.
- Be concise but thorough.
- If writing code, use syntax highlighting.`;