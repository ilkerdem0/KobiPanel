import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useState, useEffect, createContext, useContext } from "react";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import Customers from "./pages/Customers";
import Transactions from "./pages/Transactions";
import Layout from "./components/Layout";

// Auth Context
interface AuthContextType {
  token: string | null;
  userId: number | null;
  businessId: number | null;
  fullName: string | null;
  login: (token: string, userId: number, fullName: string) => void;
  logout: () => void;
  setBusinessId: (id: number) => void;
}

export const AuthContext = createContext<AuthContextType>(null!);
export const useAuth = () => useContext(AuthContext);

function App() {
  const [token, setToken] = useState<string | null>(localStorage.getItem("token"));
  const [userId, setUserId] = useState<number | null>(
    localStorage.getItem("userId") ? Number(localStorage.getItem("userId")) : null
  );
  const [businessId, setBusinessId] = useState<number | null>(
    localStorage.getItem("businessId") ? Number(localStorage.getItem("businessId")) : null
  );
  const [fullName, setFullName] = useState<string | null>(localStorage.getItem("fullName"));

  const login = (newToken: string, newUserId: number, newFullName: string) => {
    setToken(newToken);
    setUserId(newUserId);
    setFullName(newFullName);
    localStorage.setItem("token", newToken);
    localStorage.setItem("userId", String(newUserId));
    localStorage.setItem("fullName", newFullName);
  };

  const logout = () => {
    setToken(null);
    setUserId(null);
    setBusinessId(null);
    setFullName(null);
    localStorage.clear();
  };

  const updateBusinessId = (id: number) => {
    setBusinessId(id);
    localStorage.setItem("businessId", String(id));
  };

  return (
    <AuthContext.Provider value={{ token, userId, businessId, fullName, login, logout, setBusinessId: updateBusinessId }}>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={!token ? <Login /> : <Navigate to="/" />} />
          <Route path="/register" element={!token ? <Register /> : <Navigate to="/" />} />
          <Route path="/" element={token ? <Layout /> : <Navigate to="/login" />}>
            <Route index element={<Dashboard />} />
            <Route path="customers" element={<Customers />} />
            <Route path="transactions" element={<Transactions />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthContext.Provider>
  );
}

export default App;