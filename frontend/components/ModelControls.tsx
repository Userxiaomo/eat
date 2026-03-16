import React, { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { ModelConfig, Model } from '../types';
import { SlidersHorizontal, Sparkles, Check, ChevronRight, Globe } from './Icon';

interface ModelControlsProps {
  config: ModelConfig;
  availableModels: Model[];
  onUpdateConfig: (newConfig: ModelConfig) => void;
  onOpenProviderSettings: () => void;
}

export const ModelControls: React.FC<ModelControlsProps> = ({
  config,
  availableModels,
  onUpdateConfig,
  onOpenProviderSettings
}) => {
  const { t } = useTranslation();
  const [activePopover, setActivePopover] = useState<'model' | 'params' | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const activeModel = availableModels.find(m => m.id === config.modelId) || availableModels[0];
  const enabledModels = availableModels.filter(m => m.enabled);

  // Close popover when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setActivePopover(null);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleChange = (key: keyof ModelConfig, value: any) => {
    onUpdateConfig({ ...config, [key]: value });
  };

  const formatContext = (size?: number) => {
      if (!size) return '';
      if (size >= 1000000) return `${(size / 1000000).toFixed(0)}M`;
      if (size >= 1000) return `${(size / 1000).toFixed(0)}K`;
      return size.toString();
  };

  return (
    <div className="relative flex items-center gap-1" ref={containerRef}>
      
      {/* Model Selector Button */}
      <button 
        onClick={() => setActivePopover(activePopover === 'model' ? null : 'model')}
        className={`
          flex items-center gap-2 p-2 rounded-xl transition-all duration-200 border
          ${activePopover === 'model' 
            ? 'bg-blue-50 border-blue-200 text-blue-600 dark:bg-blue-900/30 dark:border-blue-800 dark:text-blue-400' 
            : 'bg-transparent border-transparent text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-800 hover:text-slate-700 dark:hover:text-slate-200'}
        `}
        title={t('controls.modelSelect')}
      >
        <div className="w-5 h-5 flex items-center justify-center rounded-full bg-gradient-to-tr from-blue-500 to-purple-500 text-white shadow-sm">
            <Sparkles size={12} fill="currentColor" />
        </div>
        <span className="text-sm font-medium max-w-[100px] truncate hidden md:block">
            {activeModel?.name?.replace('Gemini ', '')}
        </span>
      </button>

      {/* Params Button */}
      <button 
        onClick={() => setActivePopover(activePopover === 'params' ? null : 'params')}
        className={`
          p-2 rounded-xl transition-all duration-200 border
          ${activePopover === 'params' 
            ? 'bg-blue-50 border-blue-200 text-blue-600 dark:bg-blue-900/30 dark:border-blue-800 dark:text-blue-400' 
            : 'bg-transparent border-transparent text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 hover:text-slate-600 dark:hover:text-slate-300'}
        `}
        title={t('controls.params')}
      >
        <SlidersHorizontal size={18} />
      </button>

      {/* Search Toggle Button */}
      <button 
        onClick={() => handleChange('enableSearch', !config.enableSearch)}
        className={`
          p-2 rounded-xl transition-all duration-200 border
          ${config.enableSearch 
            ? 'bg-blue-50 border-blue-200 text-blue-600 dark:bg-blue-900/30 dark:border-blue-800 dark:text-blue-400' 
            : 'bg-transparent border-transparent text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 hover:text-slate-600 dark:hover:text-slate-300'}
        `}
        title={t('settings.googleSearch')}
      >
        <Globe size={18} />
      </button>

      {/* --- Model Popover --- */}
      {activePopover === 'model' && (
        <div className="absolute bottom-full left-0 mb-3 w-72 bg-white dark:bg-slate-900 rounded-2xl shadow-xl border border-slate-200 dark:border-slate-800 p-2 z-50 animate-in fade-in zoom-in-95 duration-200 origin-bottom-left">
           <div className="px-3 py-2 text-xs font-semibold text-slate-400 uppercase tracking-wider flex items-center gap-2">
              <img src="https://www.gstatic.com/lamda/images/gemini_sparkle_v002_d4735304ff6292a690345.svg" className="w-4 h-4" alt="Google" />
              {t('controls.googleProvider')}
           </div>
           
           <div className="space-y-1 max-h-[300px] overflow-y-auto">
              {enabledModels.map(model => (
                  <button
                    key={model.id}
                    onClick={() => { handleChange('modelId', model.id); setActivePopover(null); }}
                    className={`
                        w-full text-left flex items-center gap-3 p-2.5 rounded-xl transition-colors
                        ${config.modelId === model.id 
                            ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400' 
                            : 'hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-300'}
                    `}
                  >
                      <div className={`w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 ${config.modelId === model.id ? 'bg-white dark:bg-slate-800 text-blue-600' : 'bg-slate-100 dark:bg-slate-800 text-slate-500'}`}>
                          <Sparkles size={16} />
                      </div>
                      <div className="flex-1 min-w-0">
                          <div className="text-sm font-medium truncate">{model.name}</div>
                          <div className="flex items-center gap-2 text-[10px] opacity-70">
                              {model.contextWindow && (
                                  <span className="bg-slate-200 dark:bg-slate-700 px-1 rounded flex items-center gap-0.5">
                                     {formatContext(model.contextWindow)} 
                                  </span>
                              )}
                              <span className="truncate">{model.description}</span>
                          </div>
                      </div>
                      {config.modelId === model.id && <Check size={16} />}
                  </button>
              ))}
           </div>

           <div className="mt-2 pt-2 border-t border-slate-100 dark:border-slate-800">
               <button 
                  onClick={() => { setActivePopover(null); onOpenProviderSettings(); }}
                  className="w-full flex items-center justify-between px-3 py-2 text-xs text-slate-500 hover:bg-slate-50 dark:hover:bg-slate-800 rounded-lg transition-colors"
               >
                   <span>{t('provider.title')}</span>
                   <ChevronRight size={14} />
               </button>
           </div>
        </div>
      )}

      {/* --- Params Popover --- */}
      {activePopover === 'params' && (
        <div className="absolute bottom-full left-0 mb-3 w-80 bg-white dark:bg-slate-900 rounded-2xl shadow-xl border border-slate-200 dark:border-slate-800 p-4 z-50 animate-in fade-in zoom-in-95 duration-200 origin-bottom-left">
            <div className="space-y-6">
                
                {/* Temperature */}
                <div className="space-y-3">
                    <div className="flex justify-between items-center">
                        <div className="flex items-center gap-2">
                             <label className="text-sm font-medium text-slate-800 dark:text-slate-200">{t('controls.randomness')}</label>
                        </div>
                        <span className="text-xs font-mono bg-slate-100 dark:bg-slate-800 px-2 py-0.5 rounded text-slate-600 dark:text-slate-400">{config.temperature}</span>
                    </div>
                    <input 
                        type="range" min="0" max="2" step="0.1" 
                        value={config.temperature}
                        onChange={(e) => handleChange('temperature', parseFloat(e.target.value))}
                        className="w-full h-1.5 bg-slate-200 dark:bg-slate-800 rounded-lg appearance-none cursor-pointer accent-blue-600"
                    />
                    <div className="flex justify-between text-[10px] text-slate-400 uppercase tracking-wide font-medium">
                        <span>{t('controls.precise')}</span>
                        <span>{t('controls.neutral')}</span>
                        <span>{t('controls.creative')}</span>
                    </div>
                </div>

                {/* Top P */}
                <div className="space-y-3">
                    <div className="flex justify-between items-center">
                        <label className="text-sm font-medium text-slate-800 dark:text-slate-200">{t('controls.topP')}</label>
                        <span className="text-xs font-mono bg-slate-100 dark:bg-slate-800 px-2 py-0.5 rounded text-slate-600 dark:text-slate-400">{config.topP}</span>
                    </div>
                    <input 
                        type="range" min="0" max="1" step="0.05" 
                        value={config.topP}
                        onChange={(e) => handleChange('topP', parseFloat(e.target.value))}
                        className="w-full h-1.5 bg-slate-200 dark:bg-slate-800 rounded-lg appearance-none cursor-pointer accent-blue-600"
                    />
                    <p className="text-xs text-slate-500 dark:text-slate-400">
                        {t('controls.topPDesc')}
                    </p>
                </div>

            </div>
        </div>
      )}

    </div>
  );
};