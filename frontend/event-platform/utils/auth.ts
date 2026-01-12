/**
 * Utilidades para manejo de autenticación y tokens JWT
 */

export interface TokenPayload {
  exp: number;
  iat?: number;
  name?: string;
  role?: string;
  [key: string]: any;
}

/**
 * Decodifica un token JWT sin verificar la firma (solo para lectura del payload)
 */
export function decodeToken(token: string): TokenPayload | null {
  try {
    const base64Url = token.split('.')[1];
    if (!base64Url) return null;
    
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    
    return JSON.parse(jsonPayload);
  } catch (error) {
    console.error('Error al decodificar token:', error);
    return null;
  }
}

/**
 * Verifica si un token JWT ha expirado
 */
export function isTokenExpired(token: string): boolean {
  const payload = decodeToken(token);
  if (!payload || !payload.exp) {
    return true;
  }
  
  // exp está en segundos, Date.now() está en milisegundos
  const expirationTime = payload.exp * 1000;
  const currentTime = Date.now();
  
  // Considerar expirado si falta menos de 1 minuto (para evitar problemas de sincronización)
  return currentTime >= (expirationTime - 60000);
}

/**
 * Obtiene el tiempo restante hasta la expiración del token en minutos
 */
export function getTokenTimeRemaining(token: string): number {
  const payload = decodeToken(token);
  if (!payload || !payload.exp) {
    return 0;
  }
  
  const expirationTime = payload.exp * 1000;
  const currentTime = Date.now();
  const remaining = expirationTime - currentTime;
  
  return Math.max(0, Math.floor(remaining / 60000)); // Convertir a minutos
}

/**
 * Verifica si hay un token válido en localStorage
 */
export function hasValidToken(): boolean {
  const token = localStorage.getItem('jwt_token');
  if (!token) {
    return false;
  }
  
  return !isTokenExpired(token);
}

/**
 * Obtiene el token del localStorage si es válido
 */
export function getValidToken(): string | null {
  const token = localStorage.getItem('jwt_token');
  if (!token || isTokenExpired(token)) {
    return null;
  }
  
  return token;
}

/**
 * Limpia el token del localStorage
 */
export function clearToken(): void {
  localStorage.removeItem('jwt_token');
}

/**
 * Guarda un token en localStorage
 */
export function saveToken(token: string): void {
  localStorage.setItem('jwt_token', token);
}

/**
 * Obtiene el rol del usuario desde el token
 */
export function getUserRole(token: string): string | null {
  const payload = decodeToken(token);
  if (!payload) {
    return null;
  }
  
  // Buscar el claim de rol en diferentes formatos
  return payload.role || 
         payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
         null;
}
