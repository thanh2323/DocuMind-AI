import React, { useState, useEffect } from "react";
import { chatService } from "../services/chat.service";
import { useNavigate } from "react-router-dom";

const ChatListPage = () => {
  const navigate = useNavigate();
  const [sessions, setSessions] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchSessions = async () => {
      try {
        const res = await chatService.getSessions();
        if (res.success) {
          setSessions(res.data);
        }
      } catch (error) {
        console.error("Failed to load sessions", error);
      } finally {
        setLoading(false);
      }
    };
    fetchSessions();
  }, []);

  const handleCreateNew = () => {
    navigate("/chat/create"); // Navigate to the "New Chat" page
  };

  return (
    <div className="p-8 w-full h-full overflow-y-auto">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
              My Library
            </h1>
            <p className="text-subtext-light dark:text-subtext-dark mt-1">
              Manage all your research notebooks and conversations.
            </p>
          </div>
          <button
            onClick={handleCreateNew}
            className="flex items-center gap-2 bg-primary hover:bg-primary/90 text-white px-5 py-2.5 rounded-lg font-medium transition-colors shadow-sm"
          >
            <span className="material-symbols-outlined">add</span>
            <span>New Notebook</span>
          </button>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {[1, 2, 3].map((i) => (
              <div
                key={i}
                className="h-48 rounded-3xl bg-gray-100 dark:bg-gray-800 animate-pulse"
              ></div>
            ))}
          </div>
        ) : sessions.length === 0 ? (
          <div className="text-center py-24 bg-white dark:bg-surface-dark rounded-3xl border border-dashed border-gray-300 dark:border-gray-700">
            <div className="w-16 h-16 bg-blue-50 dark:bg-blue-900/20 rounded-full flex items-center justify-center mx-auto mb-4 text-primary">
              <span className="material-symbols-outlined text-3xl">
                library_books
              </span>
            </div>
            <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-2">
              No notebooks yet
            </h3>
            <p className="text-subtext-light dark:text-subtext-dark max-w-sm mx-auto mb-6">
              Create your first notebook to start analyzing documents with AI.
            </p>
            <button
              onClick={handleCreateNew}
              className="inline-flex items-center gap-2 text-primary hover:underline font-medium"
            >
              Create a new notebook
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {sessions.map((session) => (
              <div
                key={session.id}
                onClick={() => navigate(`/chat/${session.id}`)}
                className="group bg-white dark:bg-surface-dark border border-border-light dark:border-border-dark rounded-3xl p-5 cursor-pointer hover:shadow-md hover:border-blue-200 dark:hover:border-blue-900 transition-all flex flex-col h-64"
              >
                <div className="flex items-start justify-between mb-4">
                  <div className="w-10 h-10 rounded-2xl bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center text-white shadow-sm">
                    <span className="material-symbols-outlined">forum</span>
                  </div>
                  <button className="text-subtext-light hover:text-red-500 opacity-0 group-hover:opacity-100 transition-opacity p-1">
                    <span className="material-symbols-outlined text-lg">
                      more_vert
                    </span>
                  </button>
                </div>

                <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-2 line-clamp-2 leading-tight">
                  {session.title || "Untitled Notebook"}
                </h3>

                <p className="text-sm text-subtext-light dark:text-subtext-dark mb-4 line-clamp-3 flex-1">
                  Contains {session.documentCount || 0} sources.
                  {session.lastMessage
                    ? ` "${session.lastMessage}..."`
                    : " No messages yet."}
                </p>

                <div className="pt-4 border-t border-border-light dark:border-border-dark flex items-center justify-between text-xs text-subtext-light dark:text-subtext-dark">
                  <span>
                    {new Date(session.lastActiveAt).toLocaleDateString()}
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="material-symbols-outlined text-sm">
                      attachment
                    </span>
                    {session.documentCount || 0}
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default ChatListPage;
