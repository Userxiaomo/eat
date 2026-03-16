import React, { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import { ChatList } from './components/ChatList';
import { SettingsPanel } from './components/SettingsPanel';
import { ProviderModal } from './components/ProviderModal';
import { AuthModal } from './components/AuthModal';
import { MarkdownRenderer } from './components/MarkdownRenderer';
import { ModelControls } from './components/ModelControls';
import { Button } from './components/Button';
import {
    Send, Menu, Bot, User, Settings, Sparkles, Paperclip, X, Globe, Copy,
    Edit2, RefreshCw, Moon, Sun, Mic, MicOff, MessageSquare, FileText,
    Compass, PenTool
} from './components/Icon';
import { ChatSession, Message, Role, ModelConfig, Model } from './types';
import { DEFAULT_MODEL, DEFAULT_SYSTEM_INSTRUCTION, AVAILABLE_MODELS as DEFAULT_AVAILABLE_MODELS } from './constants';
import { authService } from './services/authService';
import { conversationService } from './services/conversationService';
import { modelService } from './services/modelService';

const generateId = () => Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);

// Storage keys (只保留主题和配置)
const STORAGE_CONFIG_KEY = 'gemini_chat_config';
const STORAGE_THEME_KEY = 'gemini_chat_theme';

const App: React.FC = () => {
    const { t } = useTranslation();

    // --- Global State ---
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [isInitializing, setIsInitializing] = useState(true);

    // Stores the state of all models (loaded from backend)
    const [allModels, setAllModels] = useState<Model[]>(DEFAULT_AVAILABLE_MODELS);

    // --- Chat State ---
    const [sessions, setSessions] = useState<ChatSession[]>([]);
    const [activeSessionId, setActiveSessionId] = useState<string | null>(null);
    const [input, setInput] = useState('');
    const [attachedImages, setAttachedImages] = useState<string[]>([]);
    const [isGenerating, setIsGenerating] = useState(false);
    const [isSidebarOpen, setIsSidebarOpen] = useState(true);
    const [isSettingsOpen, setIsSettingsOpen] = useState(false);
    const [isProviderModalOpen, setIsProviderModalOpen] = useState(false);
    const [theme, setTheme] = useState<'light' | 'dark'>('light');
    const [isListening, setIsListening] = useState(false);

    const [modelConfig, setModelConfig] = useState<ModelConfig>({
        modelId: DEFAULT_MODEL,
        temperature: 0.7,
        topP: 0.95,
        topK: 64,
        systemInstruction: DEFAULT_SYSTEM_INSTRUCTION,
        enableSearch: false,
    });

    const messagesEndRef = useRef<HTMLDivElement>(null);
    const textareaRef = useRef<HTMLTextAreaElement>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const recognitionRef = useRef<any>(null);

    // Derived state
    const activeSession = sessions.find(s => s.id === activeSessionId);
    const currentMessages = activeSession?.messages || [];

    const currentModelName = allModels.find(m => m.id === modelConfig.modelId)?.name || modelConfig.modelId;

    // --- Initialization ---
    useEffect(() => {
        const initialize = async () => {
            try {
                // 1. Check authentication
                if (!authService.isAuthenticated()) {
                    setIsAuthenticated(false);
                    setIsInitializing(false);
                    return;
                }

                setIsAuthenticated(true);

                // 2. Load theme and config from localStorage
                const savedConfig = localStorage.getItem(STORAGE_CONFIG_KEY);
                const savedTheme = localStorage.getItem(STORAGE_THEME_KEY);

                if (savedConfig) setModelConfig(JSON.parse(savedConfig));
                if (savedTheme) setTheme(savedTheme as 'light' | 'dark');
                else if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                    setTheme('dark');
                }

                // 3. Load models from backend
                try {
                    const models = await modelService.getAvailableModels();
                    if (models && models.length > 0) {
                        setAllModels(models.map(m => ({
                            id: m.id,
                            name: m.name,
                            description: m.description,
                            enabled: m.isEnabled,
                            contextWindow: m.contextWindow,
                        })));
                    }
                } catch (error) {
                    console.warn('Failed to load models from backend, using defaults:', error);
                }

                // 4. Load conversations from backend
                try {
                    const conversations = await conversationService.getUserConversations();
                    const mappedSessions = conversations.map(conv => ({
                        id: conv.id,
                        title: conv.title,
                        messages: [], // Messages loaded on demand
                        createdAt: new Date(conv.createdAt).getTime(),
                        updatedAt: new Date(conv.updatedAt).getTime(),
                        model: conv.modelId,
                    }));
                    setSessions(mappedSessions);

                    // If no conversations, create first one
                    if (mappedSessions.length === 0) {
                        await createNewChat();
                    }
                } catch (error) {
                    console.error('Failed to load conversations:', error);
                }

            } catch (error) {
                console.error('Initialization error:', error);
            } finally {
                setIsInitializing(false);
            }
        };

        initialize();
    }, []);

    // Persist theme and config
    useEffect(() => {
        if (theme === 'dark') document.documentElement.classList.add('dark');
        else document.documentElement.classList.remove('dark');
        localStorage.setItem(STORAGE_THEME_KEY, theme);
    }, [theme]);

    useEffect(() => {
        localStorage.setItem(STORAGE_CONFIG_KEY, JSON.stringify(modelConfig));
    }, [modelConfig]);

    // Scroll to bottom
    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [currentMessages.length, isGenerating, attachedImages]);

    // Textarea Auto-resize
    useEffect(() => {
        if (textareaRef.current) {
            textareaRef.current.style.height = 'inherit';
            textareaRef.current.style.height = `${Math.min(textareaRef.current.scrollHeight, 200)}px`;
        }
    }, [input]);

    // Load messages when switching conversation
    useEffect(() => {
        if (activeSessionId && activeSession && activeSession.messages.length === 0) {
            loadMessagesForSession(activeSessionId);
        }
    }, [activeSessionId]);

    // --- Backend Integration Functions ---

    const loadMessagesForSession = async (sessionId: string) => {
        try {
            const { messages } = await conversationService.getConversation(sessionId);
            const mappedMessages: Message[] = messages.map(msg => ({
                id: msg.id,
                role: msg.role === 'User' ? Role.USER : Role.MODEL,
                content: msg.content,
                timestamp: new Date(msg.createdAt).getTime(),
            }));

            setSessions(prev => prev.map(s =>
                s.id === sessionId ? { ...s, messages: mappedMessages } : s
            ));
        } catch (error) {
            console.error('Failed to load messages:', error);
        }
    };

    const createNewChat = async () => {
        try {
            const newConv = await conversationService.createConversation({
                title: t('chat.newChat'),
                modelId: modelConfig.modelId,
                temperature: modelConfig.temperature,
                maxTokens: null,
                systemPrompt: modelConfig.systemInstruction || null,
            });

            const newSession: ChatSession = {
                id: newConv.id,
                title: newConv.title,
                messages: [],
                createdAt: new Date(newConv.createdAt).getTime(),
                updatedAt: new Date(newConv.updatedAt).getTime(),
                model: newConv.modelId,
            };

            setSessions(prev => [newSession, ...prev]);
            setActiveSessionId(newSession.id);
            if (window.innerWidth < 768) setIsSidebarOpen(false);
        } catch (error) {
            console.error('Failed to create conversation:', error);
            alert('Failed to create new chat');
        }
    };

    const deleteSession = async (id: string, e: React.MouseEvent) => {
        e.stopPropagation();
        try {
            await conversationService.deleteConversation(id);
            const newSessions = sessions.filter(s => s.id !== id);
            setSessions(newSessions);
            if (activeSessionId === id) setActiveSessionId(null);
        } catch (error) {
            console.error('Failed to delete conversation:', error);
            alert('Failed to delete conversation');
        }
    };

    const updateSessionMessages = (sessionId: string, newMessages: Message[]) => {
        setSessions(prev => prev.map(s => {
            if (s.id === sessionId) {
                let title = s.title;
                // Auto-title logic
                if ((s.messages.length === 0 || s.title === 'New Conversation' || s.title === '新对话') && newMessages.length > 0) {
                    const firstUserMsg = newMessages.find(m => m.role === Role.USER);
                    if (firstUserMsg) {
                        const textContent = firstUserMsg.content || (firstUserMsg.images?.length ? 'Image analysis' : t('chat.newChat'));
                        title = textContent.slice(0, 30) + (textContent.length > 30 ? '...' : '');
                    }
                }
                return { ...s, messages: newMessages, updatedAt: Date.now(), title };
            }
            return s;
        }));
    };

    // --- Voice Input (unchanged) ---
    const toggleListening = () => {
        if (isListening) {
            recognitionRef.current?.stop();
            setIsListening(false);
            return;
        }
        const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
        if (!SpeechRecognition) {
            alert("Voice input is not supported in this browser.");
            return;
        }
        const recognition = new SpeechRecognition();
        recognition.lang = 'en-US';
        recognition.continuous = false;
        recognition.interimResults = true;
        recognition.onresult = (event: any) => {
            const transcript = Array.from(event.results).map((result: any) => result[0].transcript).join('');
            if (event.results[0].isFinal) {
                setInput(prev => prev ? `${prev} ${transcript}` : transcript);
                setIsListening(false);
            }
        };
        recognition.onerror = () => setIsListening(false);
        recognition.onend = () => setIsListening(false);
        recognitionRef.current = recognition;
        recognition.start();
        setIsListening(true);
    };

    // --- Chat Actions (Backend Integration) ---

    const triggerGeneration = async (
        sessionId: string,
        history: Message[]
    ) => {
        setIsGenerating(true);
        const aiMessageId = generateId();
        const initialAiMessage: Message = { id: aiMessageId, role: Role.MODEL, content: '', timestamp: Date.now() };

        const displayHistory = [...history, initialAiMessage];
        updateSessionMessages(sessionId, displayHistory);

        try {
            const lastMsg = history[history.length - 1];
            let accumulatedText = '';

            // Call backend streaming endpoint
            await conversationService.sendMessageStream(
                sessionId,
                { content: lastMsg.content, images: lastMsg.images },
                (delta: string) => {
                    // onChunk callback
                    accumulatedText += delta;
                    setSessions(prev => prev.map(s => {
                        if (s.id === sessionId) {
                            const msgs = s.messages.map(m => m.id === aiMessageId ? {
                                ...m,
                                content: accumulatedText,
                            } : m);
                            return { ...s, messages: msgs };
                        }
                        return s;
                    }));
                },
                () => {
                    // onDone callback
                    setIsGenerating(false);
                }
            );
        } catch (error) {
            console.error("Gen Error", error);
            setSessions(prev => prev.map(s => {
                if (s.id === sessionId) {
                    const msgs = s.messages.map(m => m.id === aiMessageId ? { ...m, content: t('chat.error') + ': ' + (error as Error).message } : m);
                    return { ...s, messages: msgs };
                }
                return s;
            }));
            setIsGenerating(false);
        }
    };

    const handleSendMessage = async () => {
        if ((!input.trim() && attachedImages.length === 0) || !activeSessionId || isGenerating) return;
        const userMessage: Message = {
            id: generateId(),
            role: Role.USER,
            content: input.trim(),
            images: attachedImages.length > 0 ? [...attachedImages] : undefined,
            timestamp: Date.now(),
        };
        const newHistory = [...currentMessages, userMessage];
        updateSessionMessages(activeSessionId, newHistory);
        setInput('');
        setAttachedImages([]);
        if (textareaRef.current) textareaRef.current.style.height = 'auto';
        await triggerGeneration(activeSessionId, newHistory);
    };

    const handleRegenerate = async () => {
        if (!activeSessionId || isGenerating || currentMessages.length === 0) return;
        let newHistory = [...currentMessages];
        if (newHistory[newHistory.length - 1].role === Role.MODEL) newHistory.pop();
        if (newHistory.length === 0 || newHistory[newHistory.length - 1].role !== Role.USER) return;
        updateSessionMessages(activeSessionId, newHistory);
        await triggerGeneration(activeSessionId, newHistory);
    };

    const handleEdit = (messageId: string, newContent: string) => {
        if (!activeSessionId || isGenerating) return;
        const msgIndex = currentMessages.findIndex(m => m.id === messageId);
        if (msgIndex === -1) return;
        const historyKept = currentMessages.slice(0, msgIndex);
        const updatedUserMsg: Message = { ...currentMessages[msgIndex], content: newContent, timestamp: Date.now() };
        const newHistory = [...historyKept, updatedUserMsg];
        updateSessionMessages(activeSessionId, newHistory);
        triggerGeneration(activeSessionId, newHistory);
    };

    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            const file = e.target.files[0];
            const reader = new FileReader();
            reader.onload = (event) => {
                if (event.target?.result) setAttachedImages(prev => [...prev, event.target!.result as string]);
            };
            reader.readAsDataURL(file);
        }
        e.target.value = '';
    };

    const removeImage = (index: number) => setAttachedImages(prev => prev.filter((_, i) => i !== index));
    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSendMessage();
        }
    };

    const EditMessageView = ({ msg, onCancel, onSave }: { msg: Message, onCancel: () => void, onSave: (id: string, text: string) => void }) => {
        const [val, setVal] = useState(msg.content);
        return (
            <div className="w-full bg-slate-50 dark:bg-slate-800 p-3 rounded-xl border border-blue-200 dark:border-blue-900/50">
                <textarea
                    value={val}
                    onChange={e => setVal(e.target.value)}
                    className="w-full bg-transparent border-none focus:ring-0 text-sm text-slate-800 dark:text-slate-200 resize-none" rows={3} autoFocus
                />
                <div className="flex justify-end gap-2 mt-2">
                    <Button size="sm" variant="ghost" onClick={onCancel}>{t('common.cancel')}</Button>
                    <Button size="sm" onClick={() => onSave(msg.id, val)}>{t('common.save')}</Button>
                </div>
            </div>
        )
    };
    const [editingMsgId, setEditingMsgId] = useState<string | null>(null);

    // --- Auth Success Handler ---
    const handleAuthSuccess = () => {
        setIsAuthenticated(true);
        window.location.reload(); // Reload to re-initialize with auth
    };

    // --- RENDER ---

    // Loading state
    if (isInitializing) {
        return (
            <div className="flex h-screen items-center justify-center bg-slate-50 dark:bg-slate-950">
                <div className="text-center">
                    <div className="w-16 h-16 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
                    <p className="text-slate-600 dark:text-slate-400">Loading...</p>
                </div>
            </div>
        );
    }

    // Show auth modal if not authenticated
    if (!isAuthenticated) {
        return <AuthModal onAuthSuccess={handleAuthSuccess} />;
    }

// ... (rest of the render logic stays the same, continuing from line 448)
            <div className="h-full flex flex-col items-center justify-center max-w-5xl mx-auto w-full pb-20">
              
              {/* Dashboard Hero */}
              <div className="text-center mb-10 space-y-4">
                  <h1 className="text-3xl md:text-4xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-slate-900 to-slate-500 dark:from-white dark:to-slate-400 tracking-tight">
                    今日目标，稳步达成
                  </h1>
                  <p className="text-slate-500 dark:text-slate-400">
                     从任何想法开始... 按 Ctrl Enter 换行...
                  </p>
              </div>

              {/* Action Pills */}
              <div className="flex flex-wrap justify-center gap-2 mb-12">
                  <button onClick={() => setInput("Start a new writing project")} className="flex items-center gap-2 px-4 py-2 rounded-full bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm hover:shadow-md transition-all text-sm text-slate-700 dark:text-slate-300">
                      <Bot size={14} className="text-blue-500" /> 创建助理
                  </button>
                  <button onClick={() => setInput("Analyze this data")} className="flex items-center gap-2 px-4 py-2 rounded-full bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm hover:shadow-md transition-all text-sm text-slate-700 dark:text-slate-300">
                      <Sparkles size={14} className="text-purple-500" /> 创建群组
                  </button>
                  <button onClick={() => setInput("Draft a blog post")} className="flex items-center gap-2 px-4 py-2 rounded-full bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm hover:shadow-md transition-all text-sm text-slate-700 dark:text-slate-300">
                      <PenTool size={14} className="text-orange-500" /> 写作
                  </button>
                  <button onClick={() => setInput("Draw a futuristic landscape")} className="flex items-center gap-2 px-4 py-2 rounded-full bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm hover:shadow-md transition-all text-sm text-slate-700 dark:text-slate-300">
                      <Compass size={14} className="text-green-500" /> 探索
                  </button>
              </div>

              {/* Recent Topics (Mapped from Sessions) */}
              <div className="w-full max-w-4xl grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
                  <div className="col-span-full">
                      <h3 className="flex items-center gap-2 text-sm font-bold text-slate-500 uppercase tracking-wider mb-2">
                          <MessageSquare size={14} /> 最近话题
                      </h3>
                  </div>
                  {sessions.slice(0, 3).map(s => (
                      <div 
                        key={s.id} 
                        onClick={() => setActiveSessionId(s.id)}
                        className="p-4 bg-white dark:bg-slate-900 border border-slate-100 dark:border-slate-800 rounded-2xl shadow-sm hover:shadow-md transition-all cursor-pointer group"
                      >
                          <div className="flex items-start gap-3">
                              <div className="w-10 h-10 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center text-slate-500 group-hover:bg-blue-50 dark:group-hover:bg-blue-900/20 group-hover:text-blue-500 transition-colors">
                                  <MessageSquare size={20} />
                              </div>
                              <div className="flex-1 min-w-0">
                                  <h4 className="font-semibold text-slate-800 dark:text-slate-200 truncate">{s.title || t('chat.untitled')}</h4>
                                  <p className="text-xs text-slate-400 mt-1">{new Date(s.updatedAt).toLocaleDateString()}</p>
                              </div>
                          </div>
                      </div>
                  ))}
                  {/* Fake cards if no sessions */}
                  {sessions.length === 0 && (
                      <>
                        <div className="p-4 bg-white dark:bg-slate-900 border border-slate-100 dark:border-slate-800 rounded-2xl shadow-sm opacity-60">
                            <div className="flex items-start gap-3">
                                <div className="w-10 h-10 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center"><MessageSquare size={20} /></div>
                                <div className="space-y-2 w-full">
                                    <div className="h-4 bg-slate-100 dark:bg-slate-800 rounded w-3/4"></div>
                                    <div className="h-3 bg-slate-100 dark:bg-slate-800 rounded w-1/2"></div>
                                </div>
                            </div>
                        </div>
                        <div className="p-4 bg-white dark:bg-slate-900 border border-slate-100 dark:border-slate-800 rounded-2xl shadow-sm opacity-60">
                            <div className="flex items-start gap-3">
                                <div className="w-10 h-10 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center"><MessageSquare size={20} /></div>
                                <div className="space-y-2 w-full">
                                    <div className="h-4 bg-slate-100 dark:bg-slate-800 rounded w-2/3"></div>
                                    <div className="h-3 bg-slate-100 dark:bg-slate-800 rounded w-1/3"></div>
                                </div>
                            </div>
                        </div>
                      </>
                  )}
              </div>

              {/* Recent Docs */}
              <div className="w-full max-w-4xl grid grid-cols-1 md:grid-cols-3 gap-4">
                 <div className="col-span-full">
                      <h3 className="flex items-center gap-2 text-sm font-bold text-slate-500 uppercase tracking-wider mb-2">
                          <FileText size={14} /> 最近文档
                      </h3>
                  </div>
                 {[
                     { title: "LobeChat 开发要点", desc: "开发一个类似 LobeChat 的 AI 聊天应用...", days: "5天前" },
                     { title: "FRP 内网穿透介绍", desc: "1. 总体架构 (Architecture) 采用 Master-Client...", days: "8天前" },
                     { title: "Gemini Pro 版本更新", desc: "查看最新模型的上下文窗口和多模态能力...", days: "8天前" }
                 ].map((doc, i) => (
                    <div key={i} className="p-4 bg-white dark:bg-slate-900 border border-slate-100 dark:border-slate-800 rounded-2xl shadow-sm hover:shadow-md transition-all cursor-pointer group">
                        <div className="mb-3">
                            <div className="w-8 h-8 rounded-lg bg-orange-50 dark:bg-orange-900/20 text-orange-500 flex items-center justify-center">
                                <FileText size={16} />
                            </div>
                        </div>
                        <h4 className="font-semibold text-slate-800 dark:text-slate-200 text-sm mb-1 line-clamp-1">{doc.title}</h4>
                        <p className="text-xs text-slate-400 line-clamp-2 mb-3 leading-relaxed">{doc.desc}</p>
                        <span className="text-[10px] text-slate-300 dark:text-slate-600">{doc.days}</span>
                    </div>
                 ))}
              </div>

            </div>
          ) : (
            currentMessages.map((msg, idx) => (
              <div key={msg.id} className={`flex gap-4 max-w-4xl mx-auto group mb-8 ${msg.role === Role.USER ? 'flex-row-reverse' : ''}`}>
                <div className={`w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0 mt-1 shadow-sm border ${msg.role === Role.USER ? 'bg-slate-900 dark:bg-slate-200 border-slate-900 dark:border-slate-200 text-white dark:text-slate-900' : 'bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-700 text-blue-600 dark:text-blue-400'}`}>
                  {msg.role === Role.USER ? <User size={18} /> : <Bot size={20} />}
                </div>
                <div className={`flex flex-col max-w-[85%] md:max-w-[75%] min-w-0`}>
                   <div className={`flex items-center gap-2 mb-1 px-1 text-xs text-slate-400 dark:text-slate-500 ${msg.role === Role.USER ? 'flex-row-reverse' : ''}`}>
                       <span className="font-medium text-slate-600 dark:text-slate-300">{msg.role === Role.USER ? 'You' : 'Gemini'}</span>
                       <span>{new Date(msg.timestamp).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</span>
                   </div>
                   {editingMsgId === msg.id ? (
                       <EditMessageView msg={msg} onCancel={() => setEditingMsgId(null)} onSave={(id, text) => { handleEdit(id, text); setEditingMsgId(null); }} />
                   ) : (
                       <div className={`relative px-5 py-3.5 rounded-2xl text-sm leading-relaxed shadow-sm ${msg.role === Role.USER ? 'bg-gradient-to-br from-blue-600 to-blue-700 text-white rounded-tr-sm dark:from-blue-700 dark:to-blue-800' : 'bg-white dark:bg-slate-900 border border-slate-100 dark:border-slate-800 text-slate-800 dark:text-slate-100 rounded-tl-sm shadow-slate-100 dark:shadow-none'}`}>
                         
                         {/* User uploaded images */}
                         {msg.images && msg.images.length > 0 && (
                            <div className="flex flex-wrap gap-2 mb-3">
                                {msg.images.map((img, idx) => (<img key={idx} src={img} alt="User upload" className="max-w-[200px] max-h-[200px] rounded-lg border border-white/20 object-cover" />))}
                            </div>
                         )}

                         {/* Model generated images */}
                         {msg.generatedImages && msg.generatedImages.length > 0 && (
                            <div className="flex flex-wrap gap-2 mb-3">
                                {msg.generatedImages.map((img, idx) => (
                                    <div key={idx} className="relative group">
                                        <img src={img} alt="Generated by AI" className="max-w-full rounded-lg border border-slate-200 dark:border-slate-700 shadow-sm" />
                                        <a 
                                            href={img} 
                                            download={`gemini-generated-${idx}.png`}
                                            className="absolute bottom-2 right-2 bg-black/60 text-white p-1.5 rounded-lg opacity-0 group-hover:opacity-100 transition-opacity hover:bg-black/80"
                                            title="Download"
                                        >
                                            <Paperclip size={14} />
                                        </a>
                                    </div>
                                ))}
                            </div>
                         )}

                         {msg.role === Role.USER ? (<div className="whitespace-pre-wrap">{msg.content}</div>) : (<MarkdownRenderer content={msg.content || (isGenerating && idx === currentMessages.length - 1 && (!msg.generatedImages || msg.generatedImages.length === 0) ? t('chat.thinking') : '')} />)}
                      </div>
                   )}
                  <div className={`flex items-center gap-1 mt-1 px-1 opacity-0 group-hover:opacity-100 transition-opacity ${msg.role === Role.USER ? 'flex-row-reverse' : ''}`}>
                         <button onClick={() => navigator.clipboard.writeText(msg.content)} className="p-1 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 rounded hover:bg-slate-100 dark:hover:bg-slate-800" title={t('common.copy')}><Copy size={12} /></button>
                         {msg.role === Role.USER && !isGenerating && (<button onClick={() => setEditingMsgId(msg.id)} className="p-1 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 rounded hover:bg-slate-100 dark:hover:bg-slate-800" title={t('common.edit')}><Edit2 size={12} /></button>)}
                         {msg.role === Role.MODEL && !isGenerating && idx === currentMessages.length - 1 && (<button onClick={handleRegenerate} className="p-1 text-slate-400 hover:text-blue-600 dark:hover:text-blue-400 rounded hover:bg-slate-100 dark:hover:bg-slate-800" title={t('common.regenerate')}><RefreshCw size={12} /></button>)}
                  </div>
                </div>
              </div>
            ))
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* Input Area (Floating Style) */}
        <div className="p-4 bg-transparent relative z-20">
          <div className="max-w-4xl mx-auto relative space-y-3">
             {attachedImages.length > 0 && (
                <div className="flex gap-2 overflow-x-auto py-2">
                    {attachedImages.map((img, index) => (
                        <div key={index} className="relative group flex-shrink-0">
                            <img src={img} alt="Preview" className="h-16 w-16 object-cover rounded-lg border border-slate-200 dark:border-slate-700" />
                            <button onClick={() => removeImage(index)} className="absolute -top-1.5 -right-1.5 bg-red-500 text-white rounded-full p-0.5 shadow-sm opacity-0 group-hover:opacity-100 transition-opacity"><X size={12} /></button>
                        </div>
                    ))}
                </div>
             )}
            <div className={`relative flex items-end gap-2 border rounded-3xl p-2 shadow-lg transition-all 
                ${isListening 
                    ? 'ring-2 ring-red-500/50 border-red-500 bg-red-50 dark:bg-red-900/10' 
                    : 'bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 focus-within:ring-2 focus-within:ring-blue-500/10 focus-within:border-blue-400'
                }
            `}>
              
              {/* Inserted Model Controls Here */}
              <div className="mb-0.5 self-center">
                  <ModelControls 
                    config={modelConfig}
                    availableModels={allModels}
                    onUpdateConfig={setModelConfig}
                    onOpenProviderSettings={() => setIsProviderModalOpen(true)}
                  />
              </div>
              <div className="w-[1px] h-6 bg-slate-200 dark:bg-slate-700 mx-1 self-center" />

              <input type="file" ref={fileInputRef} className="hidden" accept="image/*" onChange={handleFileSelect}/>
              <button onClick={() => fileInputRef.current?.click()} className="p-2.5 mb-0.5 text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 hover:text-slate-600 dark:hover:text-slate-300 rounded-xl transition-colors" title="Attach Image"><Paperclip size={20} /></button>
              
              <textarea ref={textareaRef} value={input} onChange={(e) => setInput(e.target.value)} onKeyDown={handleKeyDown} placeholder={isListening ? t('chat.listening') : (attachedImages.length > 0 ? t('chat.askImage') : t('chat.typeMessage'))} className="flex-1 max-h-48 bg-transparent border-none focus:ring-0 resize-none py-3 px-1 text-sm text-slate-800 dark:text-slate-100 placeholder:text-slate-400 leading-relaxed" rows={1}/>
              
              <button onClick={toggleListening} className={`p-2.5 mb-0.5 rounded-xl transition-all ${isListening ? 'text-red-500 animate-pulse' : 'text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800'}`} title={t('chat.voiceInput')}>{isListening ? <MicOff size={20} /> : <Mic size={20} />}</button>
              <Button onClick={handleSendMessage} disabled={(!input.trim() && attachedImages.length === 0) || isGenerating} variant={(!input.trim() && attachedImages.length === 0) ? "ghost" : "primary"} className={`mb-0.5 transition-all duration-200 rounded-xl ${(!input.trim() && attachedImages.length === 0) ? 'opacity-50' : 'shadow-md shadow-blue-500/20'}`} size="icon"><Send size={18} /></Button>
            </div>
            
            <div className="text-center text-[10px] text-slate-300 dark:text-slate-600">
               AI generated content may be inaccurate.
            </div>
          </div>
        </div>

        <SettingsPanel 
          config={modelConfig} 
          availableModels={allModels}
          onUpdateConfig={setModelConfig} 
          isOpen={isSettingsOpen} 
          onClose={() => setIsSettingsOpen(false)} 
        />

        <ProviderModal 
          isOpen={isProviderModalOpen}
          onClose={() => setIsProviderModalOpen(false)}
          apiKey={apiKey}
          baseUrl={baseUrl}
          availableModels={allModels}
          onUpdateApiKey={updateApiKey}
          onUpdateBaseUrl={updateBaseUrl}
          onUpdateModels={updateModels}
        />
      </div>
    </div>
  );
};

export default App;