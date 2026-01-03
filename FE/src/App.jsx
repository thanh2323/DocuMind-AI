import { BrowserRouter, Routes, Route } from 'react-router-dom';
import LoginPage from './pages/Login';
import SignUpPage from './pages/SignUp';
import DashboardPage from './pages/Dashboard';
import ChatPage from './pages/Chat';
import ChatListPage from './pages/ChatList';
import MainLayout from './layouts/MainLayout';
//import ProtectedRoute from './components/ProtectedRoute'; // Assuming we have this, or for now just layout

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route path="/register" element={<SignUpPage />} />

        {/* Protected Routes wrapped in MainLayout */}
        <Route element={<MainLayout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/library" element={<ChatListPage />} />
          <Route path="/chat" element={<ChatListPage />} /> {/* Entry is Library */}
          <Route path="/chat/create" element={<ChatPage />} /> {/* New Session Config */}
          <Route path="/chat/:sessionId" element={<ChatPage />} /> {/* Active Session */}
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
