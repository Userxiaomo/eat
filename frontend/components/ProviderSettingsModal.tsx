import React, { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { X, Zap, Box, Plus, Trash2, Check, Key, Server, Sparkles, Loader2 } from './Icon';
import { channelService, ChannelDto, AiModelDto, ProviderEnum, ModelTypeEnum } from '../services/channelService';

interface ProviderSettingsModalProps {
    isOpen: boolean;
    onClose: () => void;
    onRefresh: () => void;
}

type ProviderKey = 'Google Gemini' | 'OpenAI' | 'Anthropic';

const providerConfigs = {
    'Google Gemini': {
        icon: <Sparkles size={24} />,
        defaultBaseUrl: 'https://generativelanguage.googleapis.com',
        provider: ProviderEnum.Gemini,
        defaultModels: [
            { id: 'gemini-1.5-pro-latest', name: 'Gemini 1.5 Pro' },
            { id: 'gemini-1.5-flash-latest', name: 'Gemini 1.5 Flash' },
            { id: 'gemini-1.0-pro', name: 'Gemini 1.0 Pro' },
            { id: 'glm-4.7', name: 'GLM 4.7' }
        ]
    },
    'OpenAI': {
        icon: <Zap size={24} />,
        defaultBaseUrl: 'https://api.openai.com/v1',
        provider: ProviderEnum.OpenAI,
        defaultModels: [
            { id: 'gpt-4o', name: 'GPT-4o' },
            { id: 'gpt-4-turbo', name: 'GPT-4 Turbo' },
            { id: 'gpt-3.5-turbo', name: 'GPT-3.5 Turbo' }
        ]
    },
    'Anthropic': {
        icon: <Box size={24} />,
        defaultBaseUrl: 'https://api.anthropic.com/v1',
        provider: ProviderEnum.Claude,
        defaultModels: [
            { id: 'claude-3-opus-20240229', name: 'Claude 3 Opus' },
            { id: 'claude-3-sonnet-20240229', name: 'Claude 3 Sonnet' },
            { id: 'claude-3-haiku-20240307', name: 'Claude 3 Haiku' }
        ]
    },
};

export const ProviderSettingsModal: React.FC<ProviderSettingsModalProps> = ({ isOpen, onClose, onRefresh }) => {
    const { t } = useTranslation();
    const [activeProvider, setActiveProvider] = useState<ProviderKey>('Google Gemini');
    const [channels, setChannels] = useState<ChannelDto[]>([]);
    const [models, setModels] = useState<AiModelDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);

    // Form state
    const [apiKey, setApiKey] = useState('');
    const [baseUrl, setBaseUrl] = useState('');
    const [newModelId, setNewModelId] = useState('');
    const [newModelName, setNewModelName] = useState('');

    const currentChannel = channels.find(c => c.provider === providerConfigs[activeProvider].provider);

    // Load data
    useEffect(() => {
        if (isOpen) {
            loadChannels();
        }
    }, [isOpen]);

    useEffect(() => {
        const providerConfig = providerConfigs[activeProvider];
        const channel = channels.find(c => c.provider === providerConfig.provider);

        if (channel) {
            setApiKey(channel.apiKey);
            setBaseUrl(channel.baseUrl || providerConfig.defaultBaseUrl);
            loadModels(channel.id);
        } else {
            setApiKey('');
            setBaseUrl(providerConfig.defaultBaseUrl);
            setModels([]);
        }
    }, [activeProvider, channels]);

    const loadChannels = async () => {
        setLoading(true);
        try {
            const allChannels = await channelService.getAllChannels();
            setChannels(allChannels);
        } catch (error) {
            console.error('Failed to load channels:', error);
        } finally {
            setLoading(false);
        }
    };

    const loadModels = async (channelId: string) => {
        try {
            const channelModels = await channelService.getModelsByChannel(channelId);
            setModels(channelModels);
        } catch (error) {
            console.error('Failed to load models:', error);
        }
    };

    const handleSaveSettings = async () => {
        setSaving(true);
        try {
            const providerConfig = providerConfigs[activeProvider];
            let channelId = currentChannel?.id;

            if (currentChannel) {
                await channelService.updateChannel(currentChannel.id, {
                    apiKey,
                    baseUrl,
                });
            } else {
                const newChannel = await channelService.createChannel({
                    name: activeProvider,
                    provider: providerConfig.provider,
                    apiKey,
                    baseUrl,
                    priority: 1,
                });
                channelId = newChannel.id;
            }

            // Auto-add default models if channel is new or has no models
            if (channelId) {
                const existingModels = await channelService.getModelsByChannel(channelId);
                if (existingModels.length === 0) {
                    for (const dm of providerConfig.defaultModels) {
                        await channelService.createModel({
                            channelId,
                            name: dm.name,
                            modelId: dm.id,
                            modelType: ModelTypeEnum.Chat,
                            maxTokens: 8192,
                            sortOrder: 0
                        });
                    }
                }
            }

            await loadChannels();
            onRefresh();
        } catch (error: any) {
            console.error('Failed to save settings:', error);
            alert(error.message || 'Failed to save settings');
        } finally {
            setSaving(false);
        }
    };

    const handleToggleModel = async (modelId: string, currentStatus: boolean) => {
        try {
            await channelService.updateModel(modelId, {
                isEnabled: !currentStatus,
            });
            // Optimistic update
            setModels(prev => prev.map(m => m.id === modelId ? { ...m, isEnabled: !currentStatus } : m));
            onRefresh();
        } catch (error) {
            console.error('Failed to toggle model:', error);
        }
    };

    const handleAddModel = async () => {
        if (!currentChannel || !newModelId.trim() || !newModelName.trim()) {
            return;
        }

        try {
            const newModel = await channelService.createModel({
                channelId: currentChannel.id,
                name: newModelName,
                modelId: newModelId,
                modelType: ModelTypeEnum.Chat,
                maxTokens: 4096,
                sortOrder: 0
            });

            setNewModelId('');
            setNewModelName('');
            setModels(prev => [...prev, newModel]);
            onRefresh();
        } catch (error: any) {
            console.error('Failed to add model:', error);
            alert(error.message || 'Failed to add model');
        }
    };

    const handleDeleteModel = async (modelId: string) => {
        if (!confirm(t('provider.confirmDeleteModel') || 'Are you sure?')) {
            return;
        }

        try {
            await channelService.deleteModel(modelId);
            setModels(prev => prev.filter(m => m.id !== modelId));
            onRefresh();
        } catch (error) {
            console.error('Failed to delete model:', error);
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
            <div className="bg-white dark:bg-slate-900 rounded-3xl shadow-2xl max-w-4xl w-full max-h-[85vh] overflow-hidden flex">

                {/* Left Sidebar */}
                <div className="w-60 bg-slate-50 dark:bg-slate-950 p-4 flex flex-col border-r border-slate-100 dark:border-slate-800">
                    <h2 className="text-base font-bold text-slate-800 dark:text-slate-100 mb-4 px-2">服务聚合</h2>

                    <div className="space-y-2">
                        {(Object.keys(providerConfigs) as ProviderKey[]).map((provider) => {
                            const config = providerConfigs[provider];
                            const channel = channels.find(c => c.provider === config.provider);
                            const isActive = activeProvider === provider;

                            return (
                                <button
                                    key={provider}
                                    onClick={() => setActiveProvider(provider)}
                                    className={`
                    w-full flex items-center gap-3 px-3 py-3 rounded-xl transition-all text-sm
                    ${isActive
                                            ? 'bg-white dark:bg-slate-800 shadow-sm border border-slate-200/50 dark:border-slate-700 text-slate-900 dark:text-white'
                                            : 'text-slate-500 dark:text-slate-400 hover:bg-white/60 dark:hover:bg-slate-800/50 hover:text-slate-700'}
                  `}
                                >
                                    <div className={`
                    ${isActive ? 'text-blue-600 dark:text-blue-400' : 'text-slate-400'}
                  `}>
                                        {config.icon}
                                    </div>
                                    <span className="flex-1 text-left font-medium">{provider}</span>
                                    {channel?.isEnabled && <div className="w-1.5 h-1.5 rounded-full bg-green-500 shadow-[0_0_4px_rgba(34,197,94,0.5)]" />}
                                </button>
                            );
                        })}
                    </div>

                    <div className="mt-auto pt-4 border-t border-slate-200 dark:border-slate-800">
                        <div className="text-xs text-slate-400 px-2 font-medium">v1.2.0 • Pro Edition</div>
                    </div>
                </div>

                {/* Right Content */}
                <div className="flex-1 flex flex-col bg-white dark:bg-slate-900">

                    {/* Header */}
                    <div className="flex items-start justify-between px-8 py-6">
                        <div className="flex items-center gap-4">
                            {React.cloneElement(providerConfigs[activeProvider].icon as React.ReactElement, { size: 40, className: "text-blue-600 dark:text-blue-400" })}
                            <div className="pt-1">
                                <h3 className="text-2xl font-bold text-slate-900 dark:text-white leading-tight">{activeProvider}</h3>
                                <p className="text-sm text-slate-500 mt-1">配置 API 连接及管理可用模型。</p>
                            </div>
                        </div>
                        <button onClick={onClose} className="p-2 -mr-2 text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors">
                            <X size={20} />
                        </button>
                    </div>

                    {/* Content Scroll Area */}
                    <div className="flex-1 overflow-y-auto px-8 pb-8">
                        {loading && !apiKey ? (
                            <div className="flex items-center justify-center h-48">
                                <Loader2 className="animate-spin text-blue-500" size={32} />
                            </div>
                        ) : (
                            <div className="space-y-8">

                                {/* Connection Settings */}
                                <div className="space-y-4">
                                    <h4 className="text-sm font-medium text-slate-900 dark:text-slate-200">连接设置</h4>

                                    {/* API Key */}
                                    <div className="space-y-1.5">
                                        <label className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider ml-1">
                                            API 密钥
                                        </label>
                                        <div className="relative group">
                                            <div className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 transition-colors group-focus-within:text-blue-500">
                                                <Key size={16} />
                                            </div>
                                            <input
                                                type="password"
                                                value={apiKey}
                                                onChange={(e) => setApiKey(e.target.value)}
                                                placeholder="AIza..."
                                                className="w-full pl-10 pr-4 py-3 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 outline-none transition-all text-sm font-mono"
                                            />
                                        </div>
                                    </div>

                                    {/* Base URL */}
                                    <div className="space-y-1.5">
                                        <label className="text-xs font-medium text-slate-500 dark:text-slate-400 uppercase tracking-wider ml-1">
                                            接口代理地址 (可选)
                                        </label>
                                        <div className="relative group">
                                            <div className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 transition-colors group-focus-within:text-blue-500">
                                                <Server size={16} />
                                            </div>
                                            <input
                                                type="text"
                                                value={baseUrl}
                                                onChange={(e) => setBaseUrl(e.target.value)}
                                                placeholder={providerConfigs[activeProvider].defaultBaseUrl}
                                                className="w-full pl-10 pr-4 py-3 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 outline-none transition-all text-sm font-mono"
                                            />
                                        </div>
                                    </div>

                                    <div>
                                        <button
                                            onClick={handleSaveSettings}
                                            disabled={!apiKey.trim() || saving}
                                            className="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-xl shadow-lg shadow-blue-500/20 disabled:opacity-50 disabled:cursor-not-allowed transition-all flex items-center gap-2 transform active:scale-95"
                                        >
                                            {saving ? <Loader2 className="animate-spin" size={18} /> : <Check size={18} />}
                                            {saving ? '保存中...' : '检查连接'}
                                        </button>
                                    </div>
                                </div>

                                {/* Models Section */}
                                <div className="space-y-4 animate-in fade-in slide-in-from-bottom-4 duration-500">
                                    <div className="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-2 mt-8">
                                        <h4 className="text-sm font-medium text-slate-900 dark:text-slate-200">模型列表</h4>
                                        {currentChannel ? (
                                            <span className="text-xs text-slate-400 font-medium bg-slate-100 dark:bg-slate-800 px-2 py-0.5 rounded-full">{models.length} 已启用</span>
                                        ) : (
                                            <span className="text-xs text-slate-400 font-medium">未配置</span>
                                        )}
                                    </div>

                                    {!currentChannel ? (
                                        <div className="p-8 text-center text-slate-400 bg-slate-50 dark:bg-slate-800/50 rounded-xl border border-dashed border-slate-200 dark:border-slate-700">
                                            点击「检查连接」以加载和管理模型
                                        </div>
                                    ) : (
                                        <div className="space-y-3">
                                            {models.length === 0 && (
                                                <div className="text-center py-4 text-sm text-slate-500">暂无模型，请手动添加或检查连接。</div>
                                            )}
                                            {models.map((model) => (
                                                <div
                                                    key={model.id}
                                                    className="group flex items-center justify-between p-4 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 hover:border-blue-300 dark:hover:border-blue-700 transition-all shadow-sm hover:shadow-md"
                                                >
                                                    <div className="flex-1 min-w-0">
                                                        <div className="flex items-center gap-2 mb-1">
                                                            <span className="font-semibold text-sm text-slate-900 dark:text-white truncate">{model.name}</span>
                                                        </div>
                                                        <div className="text-xs text-slate-400 font-mono truncate">{model.modelId}</div>
                                                    </div>
                                                    <div className="flex items-center gap-4 ml-4">
                                                        <button
                                                            onClick={() => handleToggleModel(model.id, model.isEnabled)}
                                                            className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 ${model.isEnabled ? 'bg-blue-600' : 'bg-slate-200 dark:bg-slate-700'}`}
                                                        >
                                                            <span className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform shadow-sm ${model.isEnabled ? 'translate-x-6' : 'translate-x-1'}`} />
                                                        </button>
                                                        <button
                                                            onClick={() => handleDeleteModel(model.id)}
                                                            className="opacity-0 group-hover:opacity-100 p-2 text-slate-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-all"
                                                        >
                                                            <Trash2 size={18} />
                                                        </button>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )}

                                    {/* Add Model Form */}
                                    {currentChannel && (
                                        <div className="mt-6 p-1 border border-dashed border-slate-300 dark:border-slate-700 rounded-xl bg-slate-50/50 dark:bg-slate-800/30">
                                            <div className="p-4 flex gap-3 items-end">
                                                <div className="flex-1 space-y-3 md:space-y-0 md:flex md:gap-3">
                                                    <div className="flex-1">
                                                        <label className="block text-xs font-medium text-slate-500 mb-1.5 ml-1">模型ID</label>
                                                        <input
                                                            type="text"
                                                            value={newModelId}
                                                            onChange={(e) => setNewModelId(e.target.value)}
                                                            placeholder="gpt-4"
                                                            className="w-full px-4 py-2.5 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 outline-none transition-all text-sm"
                                                        />
                                                    </div>
                                                    <div className="flex-1">
                                                        <label className="block text-xs font-medium text-slate-500 mb-1.5 ml-1">显示名称</label>
                                                        <input
                                                            type="text"
                                                            value={newModelName}
                                                            onChange={(e) => setNewModelName(e.target.value)}
                                                            placeholder="GPT-4"
                                                            className="w-full px-4 py-2.5 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 outline-none transition-all text-sm"
                                                        />
                                                    </div>
                                                </div>
                                                <button
                                                    onClick={handleAddModel}
                                                    disabled={!newModelId.trim() || !newModelName.trim()}
                                                    className="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed text-sm font-medium flex items-center gap-1.5 shadow-sm shadow-blue-500/20 transition-all whitespace-nowrap"
                                                >
                                                    <Plus size={18} />
                                                    添加
                                                </button>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};
