import React, { memo } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { vscDarkPlus } from "react-syntax-highlighter/dist/esm/styles/prism";

const MarkdownRenderer = memo(({ content }) => {
  return (
    <ReactMarkdown
      remarkPlugins={[remarkGfm]}
      components={{
        code({ node, inline, className, children, ...props }) {
          const match = /language-(\w+)/.exec(className || "");
          return !inline && match ? (
            <div className="rounded-md overflow-hidden my-2">
              <div className="bg-gray-800 text-gray-200 text-xs px-3 py-1 flex justify-between items-center">
                <span>{match[1]}</span>
              </div>
              <SyntaxHighlighter
                style={vscDarkPlus}
                language={match[1]}
                PreTag="div"
                customStyle={{
                  margin: 0,
                  borderRadius: "0 0 0.375rem 0.375rem",
                }}
                {...props}
              >
                {String(children).replace(/\n$/, "")}
              </SyntaxHighlighter>
            </div>
          ) : (
            <code
              className="bg-gray-200 dark:bg-gray-700 rounded px-1 py-0.5 text-sm font-mono text-pink-600 dark:text-pink-400"
              {...props}
            >
              {children}
            </code>
          );
        },
        // Styling other elements to match your design system (Tailwind)
        h1: ({ node, ...props }) => (
          <h1
            className="text-2xl font-bold mb-4 mt-6 text-slate-900 dark:text-white"
            {...props}
          />
        ),
        h2: ({ node, ...props }) => (
          <h2
            className="text-xl font-bold mb-3 mt-5 text-slate-800 dark:text-gray-100"
            {...props}
          />
        ),
        h3: ({ node, ...props }) => (
          <h3
            className="text-lg font-semibold mb-2 mt-4 text-slate-800 dark:text-gray-100"
            {...props}
          />
        ),
        p: ({ node, ...props }) => (
          <p
            className="mb-2 leading-relaxed text-slate-700 dark:text-gray-300"
            {...props}
          />
        ),
        ul: ({ node, ...props }) => (
          <ul
            className="list-disc pl-5 mb-4 space-y-1 text-slate-700 dark:text-gray-300"
            {...props}
          />
        ),
        ol: ({ node, ...props }) => (
          <ol
            className="list-decimal pl-5 mb-4 space-y-1 text-slate-700 dark:text-gray-300"
            {...props}
          />
        ),
        li: ({ node, ...props }) => <li className="pl-1" {...props} />,
        blockquote: ({ node, ...props }) => (
          <blockquote
            className="border-l-4 border-primary/50 pl-4 py-1 my-4 bg-gray-50 dark:bg-gray-800/50 rounded-r text-slate-600 dark:text-gray-400 italic"
            {...props}
          />
        ),
        a: ({ node, ...props }) => (
          <a
            className="text-primary hover:underline hover:text-blue-600 transition-colors font-medium"
            target="_blank"
            rel="noopener noreferrer"
            {...props}
          />
        ),
        table: ({ node, ...props }) => (
          <div className="overflow-x-auto my-4 rounded-lg border border-gray-200 dark:border-gray-700">
            <table
              className="min-w-full divide-y divide-gray-200 dark:divide-gray-700"
              {...props}
            />
          </div>
        ),
        thead: ({ node, ...props }) => (
          <thead className="bg-gray-50 dark:bg-gray-800" {...props} />
        ),
        tbody: ({ node, ...props }) => (
          <tbody
            className="bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-700"
            {...props}
          />
        ),
        tr: ({ node, ...props }) => <tr {...props} />,
        th: ({ node, ...props }) => (
          <th
            className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider"
            {...props}
          />
        ),
        td: ({ node, ...props }) => (
          <td
            className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400"
            {...props}
          />
        ),
      }}
    >
      {content}
    </ReactMarkdown>
  );
});

export default MarkdownRenderer;
