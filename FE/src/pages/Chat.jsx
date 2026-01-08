import React, { useState, useEffect, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { dashboardService } from "../services/dashboard.service";
import { chatService } from "../services/chat.service";
import { documentService } from "../services/document.service";
import { authService } from "../services/auth.service";
import Toast from "../components/Toast";

const ChatPage = () => {
  const { sessionId } = useParams();
  const navigate = useNavigate();

  // Data State
  const [documents, setDocuments] = useState([]);
  const [selectedDocIds, setSelectedDocIds] = useState([]);
  const [currentSession, setCurrentSession] = useState(null);
  const [messages, setMessages] = useState([]);
  const [inputMessage, setInputMessage] = useState("");

  // UI State
  const [isLoadingDocs, setIsLoadingDocs] = useState(false);
  const [isCreatingChat, setIsCreatingChat] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [isSourcesOpen, setIsSourcesOpen] = useState(true);
  const [isUploading, setIsUploading] = useState(false);
  const [toast, setToast] = useState(null);

  const messagesEndRef = useRef(null);
  const fileInputRef = useRef(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  // Load Data
  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoadingDocs(true);
        // 1. If Session EXISTS: Load Session Data + Documents
        if (sessionId) {
          const sessionRes = await chatService.getSession(sessionId);
          if (sessionRes.success) {
            setCurrentSession(sessionRes.data);
            // If backend returns documents in session, use them.
            if (sessionRes.data.documents) {
              setDocuments(sessionRes.data.documents);
              // Auto-select all by default in active session?
              setSelectedDocIds(sessionRes.data.documents.map((d) => d.id));
            }

            // Load Messages
            const msgsRes = await chatService.getMessages(sessionId);
            if (msgsRes.success) {
              setMessages(msgsRes.data);
            }
          }
        } else {
          // 2. If NO Session (New Chat Mode): Do NOT load global docs. Clean slate.
        }
      } catch (error) {
        console.error("Failed to load data", error);
        setToast({ message: "Failed to load session data.", type: "error" });
      } finally {
        setIsLoadingDocs(false);
      }
    };
    fetchData();
  }, [sessionId]);

  // Polling for Pending Documents
  useEffect(() => {
    // Only poll if there are pending docs with real IDs.
    // Check for both backend status 0 ('Pending') and the frontend 'isPending' flag 
    // though the latter is removed once real doc is swapped in.
    const realPendingDocs = documents.filter(
      (d) => (d.status === "Pending" || d.status === 0) && !d.id.toString().startsWith("temp-")
    );

    if (realPendingDocs.length === 0) return;

    const documentIds = realPendingDocs.map(d => d.id);

    const pollInterval = setInterval(async () => {
      try {
        const statuses = await documentService.checkStatus(documentIds);

        setDocuments((prevDocs) => {
          let hasChanges = false;

          // Map over previous docs to create next state and detect changes
          const nextDocs = prevDocs.map((doc) => {
            const newStatusObj = statuses.find((s) => s.id === doc.id);
            // Only update if status CHANGED
            if (newStatusObj && newStatusObj.status !== doc.status) {
              hasChanges = true;
              return { ...doc, status: newStatusObj.status };
            }
            return doc;
          });

          if (hasChanges) {
            // Check for newly ready/error docs to show toast
            statuses.forEach(s => {
              const prevDoc = prevDocs.find(d => d.id === s.id);
              // If we found the doc and status changed
              if (prevDoc && prevDoc.status !== s.status) {
                if (s.status === 1 || s.status === "Ready") { // Ready
                  setToast({ message: `${prevDoc.fileName} is ready!`, type: "success" });
                } else if (s.status === 2 || s.status === "Error") { // Error
                  setToast({ message: `Failed to process ${prevDoc.fileName}`, type: "error" });
                }
              }
            });
            return nextDocs;
          }
          return prevDocs;
        });

      } catch (error) {
        console.error("Polling failed", error);
      }
    }, 5000);

    return () => clearInterval(pollInterval);
  }, [documents]);





  const toggleDocument = (docId) => {
    setSelectedDocIds((prev) => {
      if (prev.includes(docId)) {
        return prev.filter((id) => id !== docId);
      } else {
        return [...prev, docId];
      }
    });
  };

  const handleSelectAll = (e) => {
    if (e.target.checked) {
      setSelectedDocIds(documents.map((d) => d.id));
    } else {
      setSelectedDocIds([]);
    }
  };

  const handleSendMessage = async (e) => {
    e?.preventDefault();
    if (!inputMessage.trim()) return;

    // Mode 1: Creating New Chat
    if (!sessionId) {
      if (selectedDocIds.length === 0) {
        setToast({
          message: "Please select at least one document source.",
          type: "warning",
        });
        return;
      }
      try {
        setIsCreatingChat(true);
        const title = inputMessage.trim().substring(0, 50) + "...";
        const createRes = await chatService.createChat({
          title: title
        });

        if (createRes.success) {
          const newId = createRes.data.id;
          await chatService.sendMessage(newId, { content: inputMessage });
          navigate(`/chat/${newId}`);
        }
      } catch (error) {
        console.error("Creation failed", error);
        setToast({
          message: "Failed to create new chat session.",
          type: "error",
        });
      } finally {
        setIsCreatingChat(false);
      }
      return;
    }

    // Mode 2: Existing Chat
    try {
      const content = inputMessage.trim();
      setInputMessage("");
      setIsSending(true);

      // Optimistic
      const tempMsg = {
        id: Date.now(),
        role: "user",
        content: content,
        timestamp: new Date().toISOString(),
      };
      setMessages((prev) => [...prev, tempMsg]);

      const res = await chatService.sendMessage(sessionId, { content });
      if (res.success) {
        // Fetch messages to get the AI response
        const msgsRes = await chatService.getMessages(sessionId);
        if (msgsRes.success) setMessages(msgsRes.data);
      }
    } catch (error) {
      console.error("Send failed", error);
      setToast({
        message: "Failed to send message. Please try again.",
        type: "error",
      });
    } finally {
      setIsSending(false);
    }
  };

  const handleUploadClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const tempId = `temp-${Date.now()}`;
    const tempDoc = {
      id: tempId,
      fileName: file.name,
      isPending: true,
    };

    try {
      setIsUploading(true);
      // Optimistic UI: Add temp doc
      setDocuments((prev) => [...prev, tempDoc]);
      // Prevent interactions while uploading? Maybe just visual indication is enough.

      let targetSessionId = sessionId;

      // If NO session, create one first!
      if (!targetSessionId) {
        const title = file.name.replace(/\.[^/.]+$/, ""); // Use filename as title
        const createRes = await chatService.createChat({
          title: title
        });

        if (createRes.success) {
          targetSessionId = createRes.data.id;
        } else {
          throw new Error("Failed to create session for upload");
        }
      }

      const res = await documentService.uploadDocument(targetSessionId, file);
      if (res) {
        setToast({
          message: "Document uploaded successfully!",
          type: "success",
        });
        if (!sessionId) {
          navigate(`/chat/${targetSessionId}`);
          return;
        }

        // Update documents list: remove temp doc and add real doc
        setDocuments((prev) => {
          // Remove temp doc
          const withoutTemp = prev.filter(d => d.id !== tempId);
          // Add new doc if not already there (it shouldn't be)
          return [...withoutTemp, res];
        });

        // Auto-select the NEWLY uploaded document
        if (!selectedDocIds.includes(res.id)) {
          setSelectedDocIds((prev) => [...prev, res.id]);
        }
      }
    } catch (error) {
      console.error("Upload failed", error);
      // Remove temp doc on failure
      setDocuments((prev) => prev.filter((d) => d.id !== tempId));
      setToast({
        message: "Failed to upload document. Please try again.",
        type: "error",
      });
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  return (
    <div className="font-display bg-[#f8f9fa] dark:bg-background-dark text-text-light dark:text-text-dark h-full overflow-hidden flex flex-col relative w-full">
      {toast && (
        <Toast
          message={toast.message}
          type={toast.type}
          onClose={() => setToast(null)}
        />
      )}

      {/* Header */}
      <nav className="h-16 bg-white dark:bg-content-dark border-b border-border-light dark:border-border-dark flex items-center justify-between px-6 shrink-0 z-10 transition-colors">
        <div className="flex items-center gap-3">
          <button
            onClick={() => setIsSourcesOpen(!isSourcesOpen)}
            className="md:hidden p-2 text-subtext-light"
          >
            <span className="material-symbols-outlined">menu</span>
          </button>
          <div>
            <h1 className="text-lg font-semibold text-slate-900 dark:text-white truncate max-w-xs md:max-w-md">
              {currentSession?.title || "New Conversation"}
            </h1>
            <span className="text-xs text-subtext-light dark:text-subtext-dark">
              {selectedDocIds.length} source
              {selectedDocIds.length !== 1 ? "s" : ""} selected
            </span>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={() => navigate("/chat/create")}
            className="flex items-center gap-2 bg-primary hover:bg-primary/90 text-white px-5 py-2.5 rounded-lg font-medium transition-colors shadow-sm"
          >
            <span className="material-symbols-outlined">add</span>
            <span>New Notebook</span>
          </button>
        </div>
      </nav>

      <div className="flex flex-1 w-full overflow-hidden relative">
        {/* Sources Sidebar */}
        <aside
          className={`${isSourcesOpen
            ? "w-80 translate-x-0"
            : "w-0 -translate-x-full opacity-0"
            } transition-all duration-300 bg-white dark:bg-content-dark border-r border-border-light dark:border-border-dark flex flex-col h-full shrink-0 absolute md:static z-20 shadow-xl md:shadow-none`}
        >
          <div className="p-4 flex items-center justify-between border-b border-border-light dark:border-border-dark">
            <h2 className="text-base font-semibold text-text-light dark:text-text-dark">
              Sources
            </h2>
            <button
              onClick={() => setIsSourcesOpen(false)}
              className="md:hidden text-subtext-light hover:text-primary transition-colors"
            >
              <span className="material-symbols-outlined text-xl">
                left_panel_close
              </span>
            </button>
          </div>

          <div className="p-4 space-y-4 flex-1 overflow-y-auto">
            <button
              onClick={handleUploadClick}
              disabled={isUploading}
              className="w-full flex items-center justify-center gap-2 py-2.5 px-4 rounded-lg border border-border-light dark:border-border-dark hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors text-sm font-medium text-text-light dark:text-text-dark disabled:opacity-50"
            >
              <span className="material-symbols-outlined text-lg">add</span>
              Add Source
            </button>
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleFileChange}
              className="hidden"
              accept=".pdf"
            />

            {documents.length > 0 && (
              <div className="flex items-center justify-between pt-2">
                <span className="text-xs font-medium text-subtext-light dark:text-subtext-dark">
                  Select All
                </span>
                <input
                  className="rounded border-gray-300 text-primary focus:ring-primary h-4 w-4"
                  type="checkbox"
                  checked={
                    selectedDocIds.length === documents.length &&
                    documents.length > 0
                  }
                  onChange={handleSelectAll}
                />
              </div>
            )}

            <div className="space-y-1">
              {documents.map((doc) => {
                const isProcessing = doc.isPending || doc.status === 0 || doc.status === "Pending";
                const isError = doc.status === 2 || doc.status === "Error";

                return (
                  <div
                    key={doc.id}
                    className={`flex items-center justify-between group cursor-pointer p-2 -mx-2 rounded-lg transition-colors ${isProcessing || isError
                      ? "cursor-not-allowed bg-gray-50 dark:bg-gray-800/50 opacity-80"
                      : "hover:bg-gray-100 dark:hover:bg-gray-800"
                      }`}
                  >
                    <div
                      className="flex items-center gap-3 overflow-hidden flex-1"
                      onClick={() => !isProcessing && !isError && toggleDocument(doc.id)}
                    >
                      <div
                        className={`w-8 h-8 rounded flex items-center justify-center shrink-0 ${selectedDocIds.includes(doc.id)
                          ? "bg-blue-100 text-blue-600"
                          : "bg-gray-100 text-gray-500"
                          }`}
                      >
                        <span className="material-symbols-outlined text-lg">
                          {isError ? "error" : "article"}
                        </span>
                      </div>
                      <div className="flex flex-col overflow-hidden">
                        <span
                          className={`text-sm truncate ${selectedDocIds.includes(doc.id)
                            ? "font-medium text-text-light dark:text-white"
                            : "text-subtext-light"
                            }`}
                        >
                          {doc.fileName}
                        </span>
                        {isProcessing && <span className="text-[10px] text-primary">Processing...</span>}
                        {isError && <span className="text-[10px] text-red-500">Error</span>}
                      </div>
                    </div>
                    {isProcessing ? (
                      <div className="w-4 h-4 mr-2 border-2 border-primary border-t-transparent rounded-full animate-spin shrink-0"></div>
                    ) : isError ? (
                      <span className="material-symbols-outlined text-red-500 text-lg mr-1">warning</span>
                    ) : (
                      <input
                        checked={selectedDocIds.includes(doc.id)}
                        onChange={() => toggleDocument(doc.id)}
                        className="rounded border-gray-300 text-primary focus:ring-primary h-4 w-4 ml-2"
                        type="checkbox"
                      />
                    )}
                  </div>
                );
              })}
            </div>
            {documents.length === 0 && (
              <div className="text-center py-8 text-subtext-light text-sm">
                No sources yet.
              </div>
            )}
          </div>
        </aside>

        {/* Main Chat Content */}
        <main className="flex-1 flex flex-col bg-white dark:bg-background-dark h-full relative w-full">
          <div className="flex-1 overflow-y-auto p-4 md:p-8 md:px-20 lg:px-32 xl:px-48 scroll-smooth">
            <div className="max-w-4xl mx-auto space-y-6 pb-24">
              {messages.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-12">
                  <div className="w-16 h-16 bg-gradient-to-br from-blue-500 to-purple-500 rounded-2xl flex items-center justify-center mb-6 shadow-lg text-white">
                    <span className="material-symbols-outlined text-3xl">
                      auto_awesome
                    </span>
                  </div>
                  <h2 className="text-2xl font-display font-medium text-slate-800 dark:text-white mb-2 text-center">
                    Welcome to your Notebook
                  </h2>
                  <p className="text-subtext-light dark:text-subtext-dark text-center max-w-md">
                    Select sources from the sidebar to get started. I can help
                    you summarize, analyze, and find insights from your
                    documents.
                  </p>
                </div>
              ) : (
                messages.map((msg, idx) => (
                  <div
                    key={idx}
                    className={`group flex gap-4 ${msg.role === "user" ? "flex-row-reverse" : ""
                      }`}
                  >
                    {msg.role !== "user" && (
                      <div className="w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-purple-600 text-white flex items-center justify-center shrink-0 shadow-sm mt-1">
                        <span className="material-symbols-outlined text-sm">
                          auto_awesome
                        </span>
                      </div>
                    )}
                    <div
                      className={`flex flex-col max-w-[85%] ${msg.role === "user" ? "items-end" : "items-start"
                        }`}
                    >
                      <div
                        className={`px-5 py-3.5 ${msg.role === "user"
                          ? "bg-surface-light dark:bg-surface-dark text-text-light dark:text-text-dark rounded-3xl rounded-tr-md"
                          : "text-text-light dark:text-text-dark leading-relaxed"
                          }`}
                      >
                        <p className="whitespace-pre-wrap text-[15px]">
                          {msg.content}
                        </p>
                      </div>
                    </div>
                  </div>
                ))
              )}
              {isSending && (
                <div className="flex gap-4">
                  <div className="w-8 h-8 rounded-full bg-primary text-white flex items-center justify-center shrink-0">
                    <span className="material-symbols-outlined text-sm animate-pulse">
                      smart_toy
                    </span>
                  </div>
                  <div className="flex items-center gap-1 h-8">
                    <span className="w-1.5 h-1.5 bg-gray-400 rounded-full animate-bounce"></span>
                    <span className="w-1.5 h-1.5 bg-gray-400 rounded-full animate-bounce [animation-delay:0.2s]"></span>
                    <span className="w-1.5 h-1.5 bg-gray-400 rounded-full animate-bounce [animation-delay:0.4s]"></span>
                  </div>
                </div>
              )}
              <div ref={messagesEndRef} />
            </div>
          </div>

          {/* Input Area */}
          <div className="absolute bottom-0 left-0 w-full p-4 md:px-20 lg:px-32 xl:px-48 bg-white/90 dark:bg-background-dark/95 backdrop-blur-sm border-t border-transparent">
            <div className="max-w-4xl mx-auto relative group">
              <form
                onSubmit={handleSendMessage}
                className="bg-gray-50 dark:bg-content-dark border border-border-light dark:border-border-dark rounded-3xl flex items-end pr-2 focus-within:ring-2 focus-within:ring-primary/20 focus-within:border-primary transition-all shadow-sm"
              >
                <textarea
                  value={inputMessage}
                  onChange={(e) => setInputMessage(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && !e.shiftKey) {
                      e.preventDefault();
                      handleSendMessage();
                    }
                  }}
                  className="w-full bg-transparent border-none focus:ring-0 py-3.5 px-4 text-text-light dark:text-text-dark placeholder:text-subtext-light outline-none resize-none max-h-32 min-h-[52px]"
                  placeholder="Ask anything..."
                  rows={1}
                />
                <button
                  type="submit"
                  disabled={!inputMessage.trim() || isSending}
                  className="p-2 mb-1.5 rounded-full bg-gray-200 dark:bg-gray-700 text-subtext-light dark:text-subtext-dark hover:bg-primary hover:text-white transition-all disabled:opacity-50 disabled:hover:bg-gray-200 flex items-center justify-center"
                >
                  <span className="material-symbols-outlined text-xl">
                    arrow_upward
                  </span>
                </button>
              </form>
            </div>
            <div className="text-center mt-2">
              <p className="text-[10px] text-subtext-light/50 dark:text-subtext-dark/50">
                DocuAI can make mistakes. Verify important information.
              </p>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
};

export default ChatPage;
