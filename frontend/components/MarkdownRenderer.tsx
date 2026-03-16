import React, { useState } from 'react';
import ReactMarkdown from 'react-markdown';
import { Copy, Check } from './Icon';

interface MarkdownRendererProps {
  content: string;
}

export const MarkdownRenderer: React.FC<MarkdownRendererProps> = ({ content }) => {
  
  const CopyButton = ({ text }: { text: string }) => {
    const [copied, setCopied] = useState(false);

    const handleCopy = () => {
      navigator.clipboard.writeText(text);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    };

    return (
      <button 
        onClick={handleCopy}
        className="absolute right-2 top-2 p-1.5 rounded-md text-slate-400 hover:text-white hover:bg-slate-700/50 transition-all opacity-0 group-hover:opacity-100"
        title="Copy code"
      >
        {copied ? <Check size={14} className="text-green-400" /> : <Copy size={14} />}
      </button>
    );
  };

  return (
    <div className="prose prose-slate max-w-none dark:prose-invert prose-p:leading-relaxed prose-pre:p-0 prose-pre:bg-slate-900 prose-pre:rounded-lg prose-pre:border prose-pre:border-slate-800">
      <ReactMarkdown
        components={{
          code({ node, className, children, ...props }) {
            const match = /language-(\w+)/.exec(className || '');
            const isInline = !match;
            const textContent = String(children).replace(/\n$/, '');
            
            if (isInline) {
              return (
                <code className="bg-slate-100 dark:bg-slate-800 px-1.5 py-0.5 rounded text-sm font-mono text-pink-600 font-semibold" {...props}>
                  {children}
                </code>
              );
            }

            return (
              <div className="relative group my-4 rounded-lg overflow-hidden">
                <div className="flex items-center justify-between px-4 py-2 bg-slate-800 border-b border-slate-700 text-xs text-slate-400">
                  <span className="font-mono lowercase">{match?.[1] || 'text'}</span>
                  <CopyButton text={textContent} />
                </div>
                <code className={`block overflow-x-auto p-4 text-sm text-slate-200 font-mono bg-slate-900 ${className}`} {...props}>
                  {children}
                </code>
              </div>
            );
          },
          p({ children }) {
            return <p className="mb-3 last:mb-0 leading-7">{children}</p>
          },
          ul({ children }) {
            return <ul className="list-disc pl-5 mb-4 space-y-1">{children}</ul>
          },
          ol({ children }) {
            return <ol className="list-decimal pl-5 mb-4 space-y-1">{children}</ol>
          },
          a({ children, href }) {
            return <a href={href} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline break-all">{children}</a>
          }
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
};