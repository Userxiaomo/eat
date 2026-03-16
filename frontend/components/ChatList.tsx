import React from 'react';
import { useTranslation } from 'react-i18next';
import { ChatSession } from '../types';
import {
  MessageSquare, Trash2, Plus, Settings, User,
  LogOut, Upload, LayoutGrid, Sparkles, History
} from './Icon';
import { Button } from './Button';

interface ChatListProps {
  sessions: ChatSession[];
  activeId: string | null;
  onSelectSession: (id: string) => void;
  onDeleteSession: (id: string, e: React.MouseEvent) => void;
  onNewChat: () => void;
  onOpenProviderSettings: () => void;
  canManageProviders?: boolean;
  userInfo?: {
    username: string;
    email: string;
    role?: string;
  } | null;
  onLogout: () => void;
}

export const ChatList: React.FC<ChatListProps> = ({
  sessions,
  activeId,
  onSelectSession,
  onDeleteSession,
  onNewChat,
  onOpenProviderSettings,
  canManageProviders = false,
  userInfo,
  onLogout
}) => {
  const { t } = useTranslation();

  // Mock stats for the UI
  const stats = [
    { label: '助手', value: 7 },
    { label: '话题', value: sessions.length },
    { label: '消息', value: sessions.reduce((acc, s) => acc + s.messages.length, 0) }
  ];

  return (
    <div className="flex flex-col h-full bg-slate-50 dark:bg-slate-950 border-r border-slate-200 dark:border-slate-800 w-[280px] flex-shrink-0 transition-colors duration-200 font-sans">

      {/* 1. User Profile Area (LobeChat Style) */}
      <div className="p-4 pb-2">
        <div className="bg-white dark:bg-slate-900 rounded-2xl p-3 shadow-sm border border-slate-100 dark:border-slate-800">
          {/* Header */}
          <div className="flex items-center gap-3 mb-4">
            <div className="w-10 h-10 rounded-full bg-gradient-to-tr from-blue-500 to-purple-500 flex items-center justify-center text-white font-bold shadow-md">
              {userInfo?.username?.substring(0, 2).toUpperCase() || 'U'}
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2">
                <h3 className="font-bold text-slate-800 dark:text-slate-100 truncate text-sm">{userInfo?.username || 'User'}</h3>
                <span className="text-[10px] bg-slate-100 dark:bg-slate-800 px-1.5 py-0.5 rounded text-slate-500 border border-slate-200 dark:border-slate-700">
                  {userInfo?.role === 'Admin' ? '管理员' : '免费版'}
                </span>
              </div>
              <p className="text-xs text-slate-400 truncate">{userInfo?.email || 'user@example.com'}</p>
            </div>
          </div>

          {/* Stats Row */}
          <div className="flex justify-between px-2">
            {stats.map((stat, idx) => (
              <div key={idx} className="flex flex-col items-center cursor-pointer hover:bg-slate-50 dark:hover:bg-slate-800 p-1 rounded-lg transition-colors">
                <span className="text-sm font-bold text-slate-700 dark:text-slate-200">{stat.value}</span>
                <span className="text-[10px] text-slate-400">{stat.label}</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* 2. Quick Actions */}
      <div className="px-4 py-2 space-y-1">
        {canManageProviders && (
          <button onClick={onOpenProviderSettings} className="w-full flex items-center gap-3 px-3 py-2 text-sm text-slate-600 dark:text-slate-400 hover:bg-white dark:hover:bg-slate-900 hover:shadow-sm rounded-xl transition-all">
            <Settings size={16} />
            <span>应用设置</span>
          </button>
        )}
        <button className="w-full flex items-center gap-3 px-3 py-2 text-sm text-slate-600 dark:text-slate-400 hover:bg-white dark:hover:bg-slate-900 hover:shadow-sm rounded-xl transition-all">
          <Upload size={16} />
          <span>导入数据</span>
        </button>
        <button onClick={onLogout} className="w-full flex items-center gap-3 px-3 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 hover:shadow-sm rounded-xl transition-all">
          <LogOut size={16} />
          <span>退出登录</span>
        </button>
      </div>

      {/* Divider */}
      <div className="px-6 py-2">
        <div className="h-px bg-slate-200 dark:bg-slate-800" />
      </div>

      {/* 3. Session List Header */}
      <div className="px-4 pb-2 flex items-center justify-between">
        <span className="text-xs font-bold text-slate-400 uppercase tracking-wider pl-2">近期活动</span>
        <button onClick={onNewChat} className="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded-lg transition-colors">
          <Plus size={16} />
        </button>
      </div>

      {/* 4. Session List */}
      <div className="flex-1 overflow-y-auto px-3 space-y-1">
        {sessions.length === 0 ? (
          <div className="text-center py-10 text-slate-400 dark:text-slate-500 text-sm">
            <LayoutGrid className="mx-auto mb-2 opacity-30" size={24} />
            <p>{t('chat.noChats')}</p>
          </div>
        ) : (
          sessions.map((session) => (
            <div
              key={session.id}
              onClick={() => onSelectSession(session.id)}
              className={`
                group relative flex items-center gap-3 p-2.5 rounded-xl cursor-pointer transition-all duration-200
                ${activeId === session.id
                  ? 'bg-white dark:bg-slate-800 shadow-sm text-slate-900 dark:text-slate-100'
                  : 'hover:bg-slate-200/50 dark:hover:bg-slate-800/50 text-slate-600 dark:text-slate-400'}
              `}
            >
              <div className={`
                flex items-center justify-center w-9 h-9 rounded-[10px] flex-shrink-0 transition-colors
                ${activeId === session.id
                  ? 'bg-slate-900 text-white dark:bg-slate-200 dark:text-slate-900'
                  : 'bg-white border border-slate-200 dark:bg-slate-900 dark:border-slate-800 text-slate-400'}
              `}>
                {activeId === session.id ? <Sparkles size={16} /> : <MessageSquare size={16} />}
              </div>

              <div className="flex-1 min-w-0">
                <h3 className={`text-sm font-medium truncate`}>
                  {session.title || t('chat.untitled')}
                </h3>
                <p className="text-[10px] opacity-60 truncate">
                  {new Date(session.updatedAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} · {session.messages.length} messages
                </p>
              </div>

              <button
                onClick={(e) => onDeleteSession(session.id, e)}
                className="opacity-0 group-hover:opacity-100 p-1.5 text-slate-400 hover:text-red-500 rounded-md transition-all"
                title={t('common.delete')}
              >
                <Trash2 size={14} />
              </button>
            </div>
          ))
        )}
      </div>

      {/* Footer Branding */}
      <div className="p-3 text-center">
        <div className="text-[10px] text-slate-300 dark:text-slate-700 font-medium">
          Powered by Gemini Pro
        </div>
      </div>
    </div>
  );
};
