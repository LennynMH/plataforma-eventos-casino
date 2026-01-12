'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { hasValidToken, clearToken, getTokenTimeRemaining } from '@/utils/auth';

interface AuthGuardProps {
  children: React.ReactNode;
  redirectTo?: string;
}

/**
 * Guard de autenticación que protege rutas y componentes
 * Verifica que el usuario tenga una sesión válida antes de renderizar el contenido
 */
export default function AuthGuard({ children, redirectTo = '/' }: AuthGuardProps) {
  const [isValid, setIsValid] = useState<boolean | null>(null);
  const [timeRemaining, setTimeRemaining] = useState<number | null>(null);
  const router = useRouter();

  useEffect(() => {
    // Verificar token al montar el componente
    const checkAuth = () => {
      const valid = hasValidToken();
      setIsValid(valid);
      
      if (!valid) {
        // Token inválido o expirado, limpiar y redirigir
        clearToken();
        router.push(redirectTo);
        return;
      }
      
      // Obtener tiempo restante del token
      const token = localStorage.getItem('jwt_token');
      if (token) {
        const remaining = getTokenTimeRemaining(token);
        setTimeRemaining(remaining);
        
        // Si queda menos de 5 minutos, mostrar advertencia
        if (remaining < 5 && remaining > 0) {
          console.warn(`Tu sesión expirará en ${remaining} minuto(s)`);
        }
      }
    };

    checkAuth();

    // Verificar periódicamente (cada minuto)
    const interval = setInterval(checkAuth, 60000);

    return () => clearInterval(interval);
  }, [router, redirectTo]);

  // Mostrar loading mientras se verifica
  if (isValid === null) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          <p className="mt-4 text-gray-600">Verificando sesión...</p>
        </div>
      </div>
    );
  }

  // Si el token no es válido, no renderizar nada (ya se redirigió)
  if (!isValid) {
    return null;
  }

  // Mostrar advertencia si la sesión está por expirar
  if (timeRemaining !== null && timeRemaining < 5 && timeRemaining > 0) {
    return (
      <>
        <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-4">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-yellow-700">
                Tu sesión expirará en {timeRemaining} minuto(s). Por favor, guarda tu trabajo.
              </p>
            </div>
          </div>
        </div>
        {children}
      </>
    );
  }

  return <>{children}</>;
}
