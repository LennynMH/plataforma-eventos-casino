# Plataforma de Eventos - Frontend

Frontend desarrollado con Next.js 15, React 18, TypeScript y Tailwind CSS.

## Características

- ✅ Pantalla de Login con JWT
- ✅ Formulario de Registro de Eventos
- ✅ Validación de campos obligatorios
- ✅ Manejo de estados (loading, error, success)
- ✅ Lista editable de zonas (agregar/eliminar)
- ✅ Integración con EventService API

## Requisitos

- Node.js >= 20.9.0
- npm o yarn

## Instalación

```bash
cd frontend/event-platform
npm install
```

## Configuración

Crea un archivo `.env.local` en la raíz del proyecto con la siguiente configuración:

```env
# URL del backend API
# Para desarrollo local
API_URL=http://localhost:8070
```

**Nota:** En Docker, la variable `API_URL` se configura automáticamente mediante `docker-compose.yml`.

## Desarrollo

```bash
npm run dev
```

La aplicación estará disponible en `http://localhost:3000`

**Importante:** Asegúrate de que el backend (EventService) esté corriendo en `http://localhost:8070` antes de iniciar el frontend.

## Credenciales de Demo

- **Admin**: usuario: `admin`, contraseña: `admin`
- **User**: usuario: `user`, contraseña: `user`

## Estructura

```
frontend/event-platform/
├── app/
│   ├── layout.tsx      # Layout principal
│   ├── page.tsx         # Página principal con login/registro
│   └── globals.css      # Estilos globales
├── components/
│   ├── LoginForm.tsx              # Formulario de login
│   └── EventRegistrationForm.tsx # Formulario de registro de eventos
└── next.config.ts       # Configuración de Next.js
```

## API Endpoints

- `POST /api/auth/login` - Iniciar sesión
- `POST /api/events` - Crear evento (requiere token JWT)

## Variables de Entorno

| Variable | Descripción | Valor por defecto (desarrollo) |
|----------|-------------|--------------------------------|
| `API_URL` | URL del backend API | `http://localhost:8070` |

## Notas

- El token JWT se guarda en `localStorage`
- La aplicación se conecta al backend mediante la variable `API_URL`
- En desarrollo local, crear `.env.local` con `API_URL=http://localhost:8070`
- En Docker, la variable se configura automáticamente en `docker-compose.yml`
