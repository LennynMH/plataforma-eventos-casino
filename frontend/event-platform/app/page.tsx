'use client';

import { useState, useEffect } from 'react';
import EventRegistrationForm from '@/components/EventRegistrationForm';
import LoginForm from '@/components/LoginForm';
import AuthGuard from '@/components/AuthGuard';
import { hasValidToken, clearToken, isTokenExpired, saveToken } from '@/utils/auth';

export default function Home() {
  const [token, setToken] = useState<string | null>(null);
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    // Verificar si hay token guardado y si es válido
    const checkToken = () => {
      const savedToken = localStorage.getItem('jwt_token');
      
      if (savedToken) {
        // Verificar si el token ha expirado
        if (isTokenExpired(savedToken)) {
          // Token expirado, limpiar
          clearToken();
          setToken(null);
        } else {
          // Token válido
          setToken(savedToken);
        }
      } else {
        setToken(null);
      }
      
      setIsChecking(false);
    };

    checkToken();

    // Verificar periódicamente si el token ha expirado (cada minuto)
    const interval = setInterval(() => {
      if (token && isTokenExpired(token)) {
        clearToken();
        setToken(null);
      }
    }, 60000);

    return () => clearInterval(interval);
  }, [token]);

  const handleLogin = (newToken: string) => {
    saveToken(newToken);
    setToken(newToken);
  };

  const handleLogout = () => {
    clearToken();
    setToken(null);
  };

  // Mostrar loading mientras se verifica el token
  if (isChecking) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p className="mt-4 text-gray-600">Verificando sesión...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4">
        <div className="flex justify-between items-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            Plataforma de Eventos
          </h1>
          {token && (
            <button
              onClick={handleLogout}
              className="px-4 py-2 bg-red-500 text-white rounded-md hover:bg-red-600 transition-colors"
            >
              Cerrar Sesión
            </button>
          )}
        </div>

        {!token ? (
          <LoginForm onLogin={handleLogin} />
        ) : (
          <AuthGuard>
            <EventRegistrationForm token={token} />
          </AuthGuard>
        )}
      </div>
    </div>
  );
}
