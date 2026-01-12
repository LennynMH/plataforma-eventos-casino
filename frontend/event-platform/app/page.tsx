'use client';

import { useState, useEffect } from 'react';
import EventRegistrationForm from '@/components/EventRegistrationForm';
import LoginForm from '@/components/LoginForm';

export default function Home() {
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    // Verificar si hay token guardado
    const savedToken = localStorage.getItem('jwt_token');
    if (savedToken) {
      setToken(savedToken);
    }
  }, []);

  const handleLogin = (newToken: string) => {
    localStorage.setItem('jwt_token', newToken);
    setToken(newToken);
  };

  const handleLogout = () => {
    localStorage.removeItem('jwt_token');
    setToken(null);
  };

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
          <EventRegistrationForm token={token} />
        )}
      </div>
    </div>
  );
}
