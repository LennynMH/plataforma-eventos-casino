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

## Desarrollo

```bash
npm run dev
```

La aplicación estará disponible en `http://localhost:3000`

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

## Notas

- El token JWT se guarda en `localStorage`
- La aplicación se conecta a `http://localhost:8070` (EventService)
- Para producción, actualizar las URLs en `next.config.ts` y los componentes
