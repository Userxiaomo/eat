import { GoogleGenAI, Content, Part } from "@google/genai";
import { Message, Role, ModelConfig } from '../types';

/**
 * Prepares the history for the Gemini Chat API.
 */
const prepareHistory = (messages: Message[]): Content[] => {
  return messages.map((msg) => {
    const parts: Part[] = [];

    // Handle user uploaded images
    if (msg.images && msg.images.length > 0) {
      msg.images.forEach((imgBase64) => {
        const match = imgBase64.match(/^data:(.+);base64,(.+)$/);
        if (match) {
          parts.push({
            inlineData: {
              mimeType: match[1],
              data: match[2],
            },
          });
        } else {
             parts.push({
            inlineData: {
              mimeType: 'image/jpeg', 
              data: imgBase64,
            },
          });
        }
      });
    }

    // Handle text content
    if (msg.content) {
      parts.push({ text: msg.content });
    }

    // Note: We don't currently send back model-generated images into the history 
    // as context for future turns in this simplified implementation, 
    // but the model will see its own text response.

    return {
      role: msg.role === Role.USER ? 'user' : 'model',
      parts: parts,
    };
  });
};

/**
 * Validates the API Key and Base URL by making a lightweight request.
 */
export const validateApiKey = async (apiKey: string, baseUrl?: string): Promise<boolean> => {
    try {
        const clientConfig: any = { apiKey };
        if (baseUrl && baseUrl.trim().length > 0) {
            clientConfig.baseUrl = baseUrl.trim();
        }
        const ai = new GoogleGenAI(clientConfig);
        // Use a lightweight model and operation to test connection
        // countTokens is usually fast and cheap/free for validation
        await ai.models.generateContent({
             model: 'gemini-3-flash-preview',
             contents: 'test',
        });
        return true;
    } catch (error) {
        console.error("Connection validation failed:", error);
        return false;
    }
};

/**
 * Sends a message to the Gemini model using streaming.
 */
export const streamChatResponse = async (
  apiKey: string,
  baseUrl: string | undefined,
  currentHistory: Message[],
  newMessage: string,
  newImages: string[] | undefined,
  config: ModelConfig,
  onUpdate: (content: { text: string, images: string[] }) => void
): Promise<void> => {
  
  if (!apiKey) {
    throw new Error("API Key is missing. Please configure it in settings.");
  }

  // Initialize with optional baseUrl
  const clientConfig: any = { apiKey };
  if (baseUrl && baseUrl.trim().length > 0) {
    clientConfig.baseUrl = baseUrl.trim();
  }

  const ai = new GoogleGenAI(clientConfig);
  
  const history = prepareHistory(currentHistory);
  
  const tools = [];
  if (config.enableSearch) {
    tools.push({ googleSearch: {} });
  }

  // Handle specific config for image generation models if needed
  // But generally, the standard generateContent works for both text and image models in this SDK
  const chat = ai.chats.create({
    model: config.modelId,
    history: history,
    config: {
      temperature: config.temperature,
      topP: config.topP,
      topK: config.topK,
      systemInstruction: config.systemInstruction,
      tools: tools.length > 0 ? tools : undefined,
    },
  });

  try {
    const userMessageParts: Part[] = [];
    
    if (newImages && newImages.length > 0) {
         newImages.forEach((imgBase64) => {
            const match = imgBase64.match(/^data:(.+);base64,(.+)$/);
            if (match) {
                userMessageParts.push({
                    inlineData: { mimeType: match[1], data: match[2] }
                });
            }
         });
    }
    
    if (newMessage) {
        userMessageParts.push({ text: newMessage });
    }

    const resultStream = await chat.sendMessageStream({ 
        message: { 
            role: 'user', 
            parts: userMessageParts 
        } 
    });
    
    let fullText = '';
    const collectedImages: string[] = [];
    
    for await (const chunk of resultStream) {
        // 1. Handle Text
        const chunkText = chunk.text; 
        if (chunkText) {
          fullText += chunkText;
        }

        // 2. Handle Images (Inline Data)
        // Image generation models might return parts with inlineData
        if (chunk.candidates && chunk.candidates[0]?.content?.parts) {
            for (const part of chunk.candidates[0].content.parts) {
                if (part.inlineData) {
                    const mimeType = part.inlineData.mimeType || 'image/png';
                    const base64Data = part.inlineData.data;
                    const imageUrl = `data:${mimeType};base64,${base64Data}`;
                    if (!collectedImages.includes(imageUrl)) {
                        collectedImages.push(imageUrl);
                    }
                }
            }
        }

        onUpdate({ text: fullText, images: collectedImages });
    }
    
  } catch (error) {
    console.error("Error in streamChatResponse:", error);
    throw error;
  }
};