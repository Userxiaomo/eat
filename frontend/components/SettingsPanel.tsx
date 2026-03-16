import React from 'react';
import { useTranslation } from 'react-i18next';
import i18nInstance from '../i18n'; // Import the initialized instance directly
import { ModelConfig, Model } from '../types';
import { 
    X, Cpu, Settings
} from './Icon';

interface SettingsPanelProps {
  config: ModelConfig;
  availableModels: Model[];
  onUpdateConfig: (newConfig: ModelConfig) => void;
  isOpen: boolean;
  onClose: () => void;
}

export const SettingsPanel: React.FC<SettingsPanelProps> = ({ 
  config, 
  availableModels, // Kept in props interface for compatibility, though unused in reduced panel
  onUpdateConfig, 
  isOpen, 
  onClose 
}) => {
  const { t, i18n } = useTranslation();

  if (!isOpen) return null;

  const handleChange = (key: keyof ModelConfig, value: any) => {
    onUpdateConfig({ ...config, [key]: value });
  };

  const toggleLanguage = () => {
      // Robustly get current language from hook or global instance
      const currentLang = i18n?.language || i18nInstance.language || 'en';
      const newLang = currentLang.startsWith('zh') ? 'en' : 'zh';
      
      // Try using the hook instance first, fallback to global instance if method missing
      if (i18n && typeof i18n.changeLanguage === 'function') {
          i18n.changeLanguage(newLang);
      } else {
          i18nInstance.changeLanguage(newLang);
      }
  };

  const currentLanguageCode = i18n?.language || i18nInstance.language || 'en';
  const isChinese = currentLanguageCode.startsWith('zh');

  return (
    <div className="absolute inset-y-0 right-0 w-[380px] bg-white dark:bg-slate-950 border-l border-slate-200 dark:border-slate-800 shadow-2xl transform transition-transform duration-300 z-40 flex flex-col font-sans">
      
      {/* Header */}
      <div className="h-14 px-4 border-b border-slate-100 dark:border-slate-800 flex items-center justify-between bg-white/80 dark:bg-slate-950/80 backdrop-blur-md sticky top-0 z-10">
        <div className="flex items-center gap-2 text-slate-800 dark:text-slate-200 font-medium">
          <Settings size={18} />
          <span>{t('settings.title')}</span>
        </div>
        <button 
          onClick={onClose}
          className="p-1.5 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
        >
          <X size={18} />
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-6">
          <div className="space-y-6 animate-in fade-in slide-in-from-right-4 duration-300">
            
            {/* General Settings (Language) */}
            <div className="space-y-3">
                 <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">{t('common.language')}</label>
                 <button 
                    onClick={toggleLanguage}
                    className="w-full p-3 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-sm flex items-center justify-between text-slate-800 dark:text-slate-200 hover:border-blue-400 dark:hover:border-blue-600 transition-colors"
                 >
                     <span>{isChinese ? '简体中文' : 'English'}</span>
                     <span className="text-xs text-slate-400 bg-slate-100 dark:bg-slate-800 px-2 py-0.5 rounded">{isChinese ? 'Switch to English' : '切换到中文'}</span>
                 </button>
            </div>

            {/* System Prompt */}
            <div className="space-y-3">
               <label className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider flex items-center gap-2">
                 <Cpu size={14} />
                 {t('settings.systemInstruction')}
               </label>
               <textarea 
                 value={config.systemInstruction || ''}
                 onChange={(e) => handleChange('systemInstruction', e.target.value)}
                 className="w-full p-4 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 dark:text-slate-200 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent min-h-[300px] resize-y leading-relaxed placeholder:text-slate-400 shadow-sm"
                 placeholder={t('settings.systemPlaceholder')}
               />
               <p className="text-xs text-slate-400">
                   {t('settings.systemPlaceholder')}
               </p>
            </div>

          </div>
      </div>
    </div>
  );
};