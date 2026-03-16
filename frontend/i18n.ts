import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

const resources = {
  en: {
    translation: {
      common: {
        settings: "Settings",
        save: "Save",
        cancel: "Cancel",
        delete: "Delete",
        edit: "Edit",
        copy: "Copy",
        regenerate: "Regenerate",
        loading: "Loading...",
        language: "Language",
        theme: "Theme",
      },
      auth: {
        title: "Gemini Chat Pro",
        apiKeyLabel: "API Access Key",
        getKey: "Get Key",
        placeholder: "AIzaSy...",
        initialize: "Initialize System",
        encrypted: "End-to-End Encrypted Storage",
        agreement: "By entering, you agree to connect to the Google Gemini API.",
        aiPowered: "AI Powered",
        fast: "Fast"
      },
      chat: {
        newChat: "New Chat",
        untitled: "Untitled Chat",
        noChats: "No chats yet",
        userAccount: "User Account",
        proPlan: "Pro Plan",
        typeMessage: "Type a message...",
        listening: "Listening...",
        askImage: "Ask about this image...",
        thinking: "Thinking...",
        error: "**Error:** Generation failed. Please check your API Key and Network Settings.",
        prompts: {
          news: "🌎 What's in the news?",
          python: "🐍 Python JSON script",
          quantum: "⚛️ Explain Quantum",
          image: "🖼️ Analyze Image"
        }
      },
      controls: {
        modelSelect: "Select Model",
        params: "Parameters",
        randomness: "Randomness",
        randomnessDesc: "Higher values make output more creative.",
        topP: "Top P (Nucleus)",
        topPDesc: "Controls diversity of word choice.",
        precise: "Precise",
        neutral: "Neutral",
        creative: "Creative",
        context: "Context",
        googleProvider: "Google"
      },
      settings: {
        title: "Chat Settings",
        activeModel: "Active Model",
        modelDesc: "Select the model for this conversation. To add more models, use the provider settings in the sidebar.",
        parameters: "Parameters",
        temperature: "Temperature",
        topP: "Top P",
        systemInstruction: "System Instruction",
        systemPlaceholder: "Define the AI's persona and behavior...",
        plugins: "Plugins & Tools",
        googleSearch: "Google Search",
        googleSearchDesc: "Real-time web grounding"
      },
      provider: {
        title: "Service Providers",
        subtitle: "Configure AI model providers and connections.",
        googleTitle: "Google Gemini",
        googleDesc: "Configure API connection and manage available models.",
        connection: "Connection",
        apiKey: "API Key",
        proxyUrl: "Proxy URL (Optional)",
        check: "Check Connection",
        checking: "Checking...",
        connected: "Connected",
        failed: "Failed",
        modelList: "Model List",
        enabled: "enabled",
        addCustom: "Add Custom Model ID",
        add: "Add",
        comingSoon: "Coming Soon",
        comingSoonDesc: "Support for this provider is currently under development.",
        custom: "Custom"
      }
    }
  },
  zh: {
    translation: {
      common: {
        settings: "设置",
        save: "保存",
        cancel: "取消",
        delete: "删除",
        edit: "编辑",
        copy: "复制",
        regenerate: "重新生成",
        loading: "加载中...",
        language: "语言",
        theme: "主题",
      },
      auth: {
        title: "Gemini Chat Pro",
        apiKeyLabel: "API 访问密钥",
        getKey: "获取密钥",
        placeholder: "输入 AIzaSy 开头的密钥...",
        initialize: "初始化系统",
        encrypted: "端到端加密存储",
        agreement: "进入即代表您同意连接 Google Gemini API。",
        aiPowered: "AI 驱动",
        fast: "极速响应"
      },
      chat: {
        newChat: "新对话",
        untitled: "未命名对话",
        noChats: "暂无对话",
        userAccount: "用户账户",
        proPlan: "专业版",
        typeMessage: "输入消息...",
        listening: "正在听...",
        askImage: "关于这张图片...",
        thinking: "思考中...",
        error: "**错误:** 生成失败。请检查您的 API 密钥和网络设置。",
        prompts: {
          news: "🌎 今天有什么新闻？",
          python: "🐍 写一个 Python JSON 解析脚本",
          quantum: "⚛️ 像给5岁孩子一样解释量子计算",
          image: "🖼️ 分析这张图片"
        }
      },
      controls: {
        modelSelect: "选择模型",
        params: "参数调节",
        randomness: "随机性 (Temperature)",
        randomnessDesc: "值越大，回复越具有创造性。",
        topP: "核采样 (Top P)",
        topPDesc: "控制词汇选择的多样性。",
        precise: "精确",
        neutral: "平衡",
        creative: "创造力",
        context: "上下文",
        googleProvider: "Google"
      },
      settings: {
        title: "会话设置",
        activeModel: "当前模型",
        modelDesc: "选择当前对话使用的模型。如需添加更多模型，请使用侧边栏的服务商设置。",
        parameters: "参数设置",
        temperature: "随机性 (Temperature)",
        topP: "核采样 (Top P)",
        systemInstruction: "系统提示词 (System Prompt)",
        systemPlaceholder: "定义 AI 的角色和行为方式...",
        plugins: "插件与工具",
        googleSearch: "Google 搜索",
        googleSearchDesc: "实时联网搜索"
      },
      provider: {
        title: "服务聚合",
        subtitle: "配置 AI 模型服务商及连接。",
        googleTitle: "Google Gemini",
        googleDesc: "配置 API 连接及管理可用模型。",
        connection: "连接设置",
        apiKey: "API 密钥",
        proxyUrl: "接口代理地址 (可选)",
        check: "检查连接",
        checking: "检查中...",
        connected: "连接成功",
        failed: "连接失败",
        modelList: "模型列表",
        enabled: "已启用",
        addCustom: "添加自定义模型 ID",
        add: "添加",
        comingSoon: "敬请期待",
        comingSoonDesc: "该服务商的支持正在开发中，敬请关注！",
        custom: "自定义"
      }
    }
  }
};

// Detect stored language manually to override browser default detection behavior
// This ensures "Default is Chinese" for new users, but remembers choice for returning users.
const storedLang = localStorage.getItem('i18nextLng');
const defaultLang = storedLang || 'zh';

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    lng: defaultLang, // Force usage of stored language or 'zh'
    fallbackLng: 'zh',
    debug: false,
    interpolation: {
      escapeValue: false
    },
    detection: {
      // Only cache to localStorage, don't look at navigator for the default
      order: ['localStorage'],
      caches: ['localStorage']
    }
  });

export default i18n;