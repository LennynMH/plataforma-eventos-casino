# 🎪 Plataforma de Eventos - MVP

Sistema de gestión de eventos desarrollado con arquitectura de microservicios, event-driven y Clean Architecture.

## 📋 Descripción

Plataforma de eventos online que permite gestionar eventos con múltiples zonas, precios y capacidad. El sistema está construido con:

- **Backend**: 2 microservicios .NET 9 (EventService, NotificationService)
- **Frontend**: Next.js 15 + React 18 + TypeScript
- **Arquitectura**: Clean Architecture + DDD + Event-Driven
- **Comunicación**: Síncrona (HTTP) y Asíncrona (RabbitMQ)
- **Persistencia**: PostgreSQL (una DB por servicio)
- **Cache**: Redis
- **Seguridad**: JWT Authentication + Authorization por roles

---

## 🎯 Contexto del Reto Técnico

Este proyecto fue desarrollado como parte del **Reto Técnico para Líder Técnico** con el objetivo de diseñar y construir una plataforma de eventos online completa.

### Objetivos del Reto

La plataforma debe permitir:
- ✅ Publicar y administrar eventos (con múltiples zonas, precios, capacidad, reglas)
- ✅ Búsqueda avanzada de eventos publicados con diferentes criterios de búsqueda y respuesta rápida
- ✅ Vender y reservar tickets con alta concurrencia (picos en preventa y lanzamientos)
- ✅ Gestionar usuarios y accesos (clientes, promotores, admins, staff)
- ✅ Procesar pagos con proveedores externos (PSP) y manejar reembolsos/compensaciones
- ✅ Entregar tickets (QR/Barcode) y validar el ingreso (check-in)
- ✅ Notificar al usuario (email/SMS/push/whatsapp) y mantener trazabilidad
- ✅ Operar el sistema con observabilidad, auditoría, seguridad y cumplimiento

### Requisitos del Sistema

La plataforma debe soportar:
- **Alta demanda** en momentos críticos (apertura de ventas / eventos populares)
- **Consistencia** (evitar sobreventa)
- **Auditabilidad** (trazabilidad de operaciones)
- **Disponibilidad** (fallas parciales sin tumbar todo)
- **Evolución** (agregar promociones, marketplace, integraciones, BI)

### Actores del Sistema

- **Cliente final**: realiza búsquedas, compra/reserva tickets, recibe confirmaciones, descarga ticket
- **Organizador/Promotor**: crea eventos, define zonas, precios, aforos, campañas
- **Administrador**: gobierno, configuración, catálogos, fraude, soporte
- **Staff de puerta**: valida tickets (check-in) en tiempo real o modo offline
- **Sistemas externos**: PSP de pagos, servicio de mensajería, antifraude, BI/CRM

---

## 📐 Requisitos del Reto Técnico

### 1. Arquitectura

- ✅ Diseño basado en microservicios y eventos con Broker de mensajería
- ✅ Comunicación asíncrona y síncrona entre los microservicios según cada caso de uso
- ✅ Motores de BD SQL y NoSQL según cada microservicio y tipo de uso
- ✅ Autenticación y autorización con JWT y estándares como OIDC y OAuth 2.0
- ✅ Diseño de arquitectura en Nube AWS o Híbrido
- ✅ Patrones de resiliencia y alta concurrencia

### 2. Backlog y ROADMAP

- ✅ Backlog definido con las principales tareas
- ✅ Marco de trabajo Scrum, con Sprint de 2 semanas
- ✅ Equipo formado por 2 Backend, 2 Frontend, 1 FullStack, 2 QA, 1 UX, 1 UI, 1 Scrum Master, 1 Product Owner y 1 Líder Técnico
- ✅ Áreas de soporte en TI: Arquitectura, Base de datos, Seguridad, Plataforma
- ✅ Proceso de desarrollo: DEV → QA → Staging → Production
- ✅ Generación de documentos técnicos y funcionales
- ✅ Plan de trabajo para primera versión en 6 meses

> **📌 Nota sobre el ROADMAP**: El roadmap detallado está disponible en `docs/BACKLOG_ROADMAP.md` y puede ser importado en el programa **Xmind** para visualización interactiva y gestión del plan de trabajo.

### 3. Stack Tecnológico Requerido

#### Herramientas y Programas
- ✅ Diagramas: Draw IO | Mermaid | Excalidraw
- ✅ IDE: Visual Studio | Visual Studio Code | Rider
- ✅ IDE BD: SSMS | pgAdmin | dbeaver | Mongo Atlas
- ✅ Gestión de contenedores: Docker Desktop o Podman Desktop

#### Stack Backend
- ✅ Framework .NET 9
- ✅ Librerías NET: EF Core, MassTransit, MediatR, AutoMapper, FluentValidation, Polly, MailKit
- ✅ Comunicación asíncrona: RabbitMQ
- ✅ Persistencia SQL: PostgreSQL
- ✅ Persistencia temporal: Redis Cache

#### Stack Frontend
- ✅ React 18+ y Next.js (TypeScript)
- ✅ Estilos con Tailwind CSS

#### Infraestructura Local
- ✅ docker-compose para levantar: api-event, api-notifications, db(s), rabbitmq

### 4. MVP - 2 APIs

#### API 1 — EventService ✅

**Responsable de:**
- ✅ Registrar eventos y zonas
- ✅ Publicar eventos y notificar a los otros microservicios de forma asíncrona

**Dominio:**
- ✅ Event (id, nombre, fecha, lugar, estado)
- ✅ Zone (id, eventId, nombre, precio, capacidad)

**Requerimientos funcionales:**
1. ✅ **Crear Evento (Admin)**: `POST /events`
   - Crea evento + zonas en una transacción
   - Publica mensaje EventCreated en cola (asíncrono)
2. ✅ **Listar Eventos**: `GET /events`
   - Incluye almacenamiento en cache con Redis
3. ✅ **Obtener detalle**: `GET /events/{id}`

#### API 2 — NotificationService ✅

**Responsable de:**
- ✅ Consumir mensajes del broker cuando se cree y publique un evento
- ✅ Persistir un registro de la notificación en su propia DB
- ✅ Envío de notificación por correo usando MailKit

**Requerimientos funcionales:**
1. ✅ **Consumir EventCreated**
   - Guarda un registro en DB con: eventId, nombre, timestamp, correlationId, payloadHash
2. ✅ **Notificar por correo**
   - Envía un correo con detalles del evento creado o publicado

### 5. Requisitos de Mensajería (Obligatorio) ✅

1. ✅ **Cola/event**: Tipo de mensaje mínimo con messageId, eventId, name, occurredAt, correlationId, version
2. ✅ **Idempotencia del consumidor**: NotificationService evita procesar el mismo messageId dos veces
3. ✅ **Reintentos**: Si falla el procesamiento, se reintenta con política simple (exponential backoff con Polly)
4. ✅ **DLQ / Dead Letter**: Si no se puede procesar tras N reintentos, se mueve a cola de muertos o se registra estado "Failed"

### 6. Requisitos de Persistencia (Obligatorio) ✅

- ✅ Cada API persiste datos en su DB
- ✅ DB por servicio (PostgreSQL separado)
- ✅ ORM: Entity Framework Core
- ✅ Scripts de inicialización: `db/init.sql` (o migraciones) para tablas mínimas
- ✅ Migraciones automáticas al iniciar

### 7. Requisitos de Seguridad (Bonus) ✅

1. ✅ **Autenticación JWT**: JWT local (issuer propio)
2. ✅ **Autorización por roles**: Admin puede POST /events, User puede GET /events
3. ✅ **Evitar IDOR**: Protección de recursos ajenos
4. ✅ **Manejo de errores seguro**: No filtrar stack traces ni detalles de DB
5. ✅ **Protección anti-abuso**: Rate limiting básico por IP/usuario (AspNetCoreRateLimit)
6. ✅ **Logs sin datos sensibles**: No loguear tokens, passwords, ni PII

### 8. Frontend (React) — Pantalla Mínima ✅

**Pantalla: "Registrar Evento"**
- ✅ Formulario con: Nombre del evento, Fecha, Lugar, Zonas (lista editable: nombre, precio, capacidad)
- ✅ Botón "Guardar"
- ✅ Consumir POST /events usando token JWT
- ✅ Validación mínima (campos obligatorios, capacidad > 0, precio >= 0)
- ✅ Manejo de loading / error

### 9. Entregables Finales ✅

#### A. Diagrama de Arquitectura ✅
- ✅ Diagrama en `docs/architecture.md` y `docs/architectura.drawio`
- ✅ Componentes y servicios: APIs, Broker, DB(s)
- ✅ Listado de microservicios
- ✅ Flujos: HTTP sync + eventos async
- ✅ Notas de seguridad: JWT, roles, boundaries
- ✅ Sustentación breve

#### B. Backlog y Plan de Trabajo General (ROADMAP) ✅
- ✅ Formato en `docs/BACKLOG_ROADMAP.md`
- ✅ Lista de tareas principales del Backlog
- ✅ **Importable en Xmind** para visualización y gestión interactiva

#### C. Código Fuente del Backend y Frontend ✅
- ✅ README con instrucciones para ejecutar el proyecto
- ✅ Scripts de BD para creación del modelo de datos e inicialización
- ✅ Instrucciones de migración y carga de datos iniciales (Seed)
- ✅ Archivos Dockerfile y Docker Compose

---

## ✅ Estado de Implementación

### MVP Completo - Todos los Requisitos Implementados

El MVP ha sido completado al 100% cumpliendo con todos los requisitos del reto técnico:

#### ✅ Punto 1: Contexto
- Documentación completa del contexto y objetivos

#### ✅ Punto 2: Arquitectura y MVP
- Arquitectura de microservicios implementada
- Event-driven architecture funcionando
- Clean Architecture + DDD aplicado

#### ✅ Punto 3: Stack Tecnológico
- Stack completo implementado según especificaciones
- Todas las herramientas y librerías configuradas

#### ✅ Punto 4: 2 APIs + Frontend
- EventService completamente funcional
- NotificationService completamente funcional
- Frontend React/Next.js operativo

#### ✅ Punto 5: Mensajería
- RabbitMQ con MassTransit configurado
- Idempotencia implementada (ProcessedMessage)
- Reintentos con Polly (exponential backoff)
- DLQ automático (MassTransit)

#### ✅ Punto 6: Persistencia
- EF Core como ORM
- DB por servicio (PostgreSQL separado)
- Migraciones automáticas
- Scripts de inicialización

#### ✅ Punto 7: Seguridad (Bonus)
- Autenticación JWT local
- Autorización por roles (Admin, User)
- Rate limiting (AspNetCoreRateLimit)
- Manejo seguro de errores
- Logs sin datos sensibles

#### ✅ Punto 8: Frontend
- Pantalla "Registrar Evento" completa
- Formulario con validaciones
- Manejo de loading/error/success
- Integración con JWT

#### ✅ Punto 9: Docker y Entregables
- Dockerfiles para cada servicio
- Docker Compose completo
- Documentación completa

### Características Implementadas

#### EventService
- ✅ Crear eventos con zonas (transacción)
- ✅ Listar eventos con paginación
- ✅ Obtener detalle de evento
- ✅ Cache con Redis
- ✅ Publicación de eventos a RabbitMQ
- ✅ Validación con FluentValidation
- ✅ Autenticación y autorización JWT
- ✅ Rate limiting
- ✅ Manejo de errores global

#### NotificationService
- ✅ Consumer de EventCreated (MassTransit)
- ✅ Idempotencia (evita duplicados)
- ✅ Reintentos automáticos (Polly)
- ✅ Dead Letter Queue
- ✅ Envío de emails (MailKit)
- ✅ Persistencia de logs de notificación
- ✅ Health checks

#### Frontend
- ✅ Pantalla de login
- ✅ Pantalla de registro de eventos
- ✅ Formulario con validaciones
- ✅ Gestión de zonas (agregar/eliminar)
- ✅ Manejo de estados (loading, error, success)
- ✅ Integración con API (JWT)
- ✅ UI moderna con Tailwind CSS

---

## 📁 Estructura del Proyecto

```
Casino/
├── backend/
│   ├── EventService/                    # Microservicio 1 - Gestión de Eventos
│   │   ├── Dockerfile                   # Dockerfile del servicio
│   │   ├── EventService.sln             # Solución .NET
│   │   ├── EventService.Domain/        # Entidades y eventos de dominio
│   │   ├── EventService.Application/   # Commands, Queries, DTOs, Validators
│   │   ├── EventService.Infrastructure/ # DbContext, Handlers, Cache, Migrations
│   │   └── EventService.API/            # Controllers, Program.cs, Middleware
│   │
│   └── NotificationService/             # Microservicio 2 - Notificaciones
│       ├── Dockerfile                   # Dockerfile del servicio
│       ├── NotificationService.sln     # Solución .NET
│       ├── NotificationService.Domain/ # Entidades
│       ├── NotificationService.Application/
│       ├── NotificationService.Infrastructure/ # DbContext, Consumers, Email, Migrations
│       └── NotificationService.API/     # Program.cs, Controllers
│
├── frontend/
│   └── event-platform/                  # Frontend Next.js + React
│       ├── app/                         # App Router
│       ├── components/                   # Componentes React
│       └── package.json
│
├── docs/                                 # Documentación
│   ├── architecture.md                  # Diagrama de arquitectura
│   ├── BACKLOG_ROADMAP.md               # Backlog y ROADMAP
│   ├── ANALISIS_PUNTO_5_MENSAJERIA.md  # Análisis Punto 5
│   ├── IMPLEMENTACION_PUNTO_6_PERSISTENCIA.md
│   ├── IMPLEMENTACION_PUNTO_7_SEGURIDAD.md
│   └── IMPLEMENTACION_PUNTO_8_FRONTEND.md
│
├── scripts/
│   └── db/                              # Scripts SQL de inicialización
│       ├── init-events.sql              # Script para EventService DB
│       └── init-notifications.sql      # Script para NotificationService DB
│
├── docker-compose.yml                   # ⭐ ORQUESTACIÓN COMPLETA
└── README.md                            # Este archivo
```

---

## 🚀 Inicio Rápido

### Prerrequisitos

- **Docker Desktop** o **Podman Desktop** instalado
- **.NET 9 SDK** (opcional, solo si quieres ejecutar sin Docker)
- **Node.js 20+** (opcional, solo si quieres ejecutar frontend sin Docker)

### Opción 1: Ejecutar con Docker Compose (Recomendado)

1. **Clonar el repositorio**:
```bash
git clone <url-del-repositorio>
cd Casino
```

2. **Levantar todos los servicios**:
```bash
docker-compose up -d
```

3. **Verificar que todos los servicios estén corriendo**:
```bash
docker-compose ps
```

4. **Ver logs de un servicio específico**:
```bash
docker-compose logs -f event-service
docker-compose logs -f notification-service
```

5. **Acceder a los servicios**:
   - Frontend: `http://localhost:3000` (si está configurado en docker-compose)
   - EventService API: `http://localhost:8070`
   - NotificationService API: `http://localhost:8071`
   - RabbitMQ Management: `http://localhost:15672` (admin/admin123)
   - Swagger EventService: `http://localhost:8070/swagger`

6. **Detener todos los servicios**:
```bash
docker-compose down
```

### Opción 2: Ejecutar sin Docker (Desarrollo Local)

#### Backend - EventService

```bash
cd backend/EventService

# Restaurar dependencias
dotnet restore

# Aplicar migraciones (asegúrate de que PostgreSQL esté corriendo)
cd EventService.API
dotnet ef database update --project ../EventService.Infrastructure

# Ejecutar
dotnet run --project EventService.API
```

#### Backend - NotificationService

```bash
cd backend/NotificationService

# Restaurar dependencias
dotnet restore

# Aplicar migraciones (asegúrate de que PostgreSQL esté corriendo)
cd NotificationService.API
dotnet ef database update --project ../NotificationService.Infrastructure

# Ejecutar
dotnet run --project NotificationService.API
```

#### Frontend

```bash
cd frontend/event-platform

# Instalar dependencias
npm install

# Ejecutar en modo desarrollo
npm run dev
```

---

## 🗄️ Base de Datos

### Migraciones EF Core

Las migraciones se aplican **automáticamente** al iniciar cada servicio. Si necesitas aplicarlas manualmente:

#### EventService

```bash
cd backend/EventService/EventService.API
dotnet ef database update --project ../EventService.Infrastructure
```

#### NotificationService

```bash
cd backend/NotificationService/NotificationService.API
dotnet ef database update --project ../NotificationService.Infrastructure
```

### Scripts de Inicialización

Los scripts SQL en `scripts/db/` se ejecutan automáticamente al crear los contenedores PostgreSQL:

- `scripts/db/init-events.sql` - Crea schema `events` para EventService
- `scripts/db/init-notifications.sql` - Crea schema `notifications` para NotificationService

### Datos Iniciales (Seed Data)

Actualmente no hay datos iniciales. Para agregar datos de ejemplo:

1. **Opción 1**: Crear una migración con datos seed
2. **Opción 2**: Ejecutar scripts SQL manualmente después de crear las tablas

**Ejemplo de migración con seed data**:

```bash
cd backend/EventService/EventService.Infrastructure
dotnet ef migrations add SeedInitialData
```

Luego editar la migración para agregar datos:

```csharp
migrationBuilder.InsertData(
    table: "Events",
    schema: "events",
    columns: new[] { "Id", "Name", "Date", "Location", "Status", "CreatedAt", "UpdatedAt" },
    values: new object[] { 
        Guid.NewGuid(), 
        "Concierto de Rock", 
        DateTime.UtcNow.AddDays(30), 
        "Estadio Nacional", 
        "Active",
        DateTime.UtcNow,
        DateTime.UtcNow
    });
```

---

## 🔐 Autenticación y Autorización

### Credenciales de Demo

- **Admin**: 
  - Usuario: `admin`
  - Contraseña: `admin`
  - Permisos: Puede crear eventos (POST /events) y listar eventos

- **User**: 
  - Usuario: `user`
  - Contraseña: `user`
  - Permisos: Solo puede listar eventos (GET /events)

### Obtener Token JWT

```bash
curl -X POST http://localhost:8070/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin"}'
```

Respuesta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Usar Token en Requests

```bash
curl -X GET http://localhost:8070/api/events \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## 🐳 Docker Compose

### Servicios Incluidos

El `docker-compose.yml` en la raíz orquesta:

- **postgres-events** (Puerto 5432) - Base de datos para EventService
- **postgres-notifications** (Puerto 5433) - Base de datos para NotificationService
- **rabbitmq** (Puertos 5672, 15672) - Message Broker
- **redis** (Puerto 6379) - Cache
- **event-service** (Puerto 8070) - API EventService
- **notification-service** (Puerto 8071) - API NotificationService

### Dockerfiles

Cada servicio tiene su propio Dockerfile:

- `backend/EventService/Dockerfile`
- `backend/NotificationService/Dockerfile`

Los Dockerfiles usan multi-stage build para optimizar el tamaño de las imágenes.

---

## 📡 Endpoints de la API

### EventService (Puerto 8070)

#### Autenticación
- `POST /api/auth/login` - Iniciar sesión y obtener JWT token

#### Eventos
- `POST /api/events` - Crear evento (requiere rol Admin)
- `GET /api/events` - Listar eventos (requiere rol User o Admin)
- `GET /api/events/{id}` - Obtener detalle de evento (requiere rol User o Admin)

**Swagger**: `http://localhost:8070/swagger`

### NotificationService (Puerto 8071)

- `GET /api/notifications` - Listar logs de notificaciones
- `GET /api/notifications/{id}` - Obtener log específico
- `GET /health` - Health check

---

## 🧪 Probar el Flujo Completo

1. **Iniciar servicios**:
```bash
docker-compose up -d
```

2. **Esperar a que todos los servicios estén listos** (30-60 segundos)

3. **Acceder al frontend**: `http://localhost:3000`

4. **Login**:
   - Usuario: `admin`
   - Contraseña: `admin`

5. **Crear un evento**:
   - Llenar el formulario
   - Agregar al menos una zona
   - Hacer clic en "Guardar"

6. **Verificar**:
   - Evento creado en EventService
   - Mensaje publicado en RabbitMQ (ver en `http://localhost:15672`)
   - NotificationService consume el mensaje
   - Email enviado (si SMTP configurado)
   - Log guardado en NotificationService DB

---

## 🔧 Configuración

### Variables de Entorno

Las configuraciones están en `appsettings.json` de cada servicio. Para Docker, se pueden sobrescribir con variables de entorno en `docker-compose.yml`.

#### EventService

```json
{
  "ConnectionStrings": {
    "EventDb": "Host=postgres-events;Port=5432;Database=eventdb;Username=eventuser;Password=eventpass123",
    "Redis": "redis:6379"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "admin",
    "Password": "admin123"
  },
  "JWT": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration12345678901234567890",
    "Issuer": "EventService",
    "Audience": "EventService"
  }
}
```

#### NotificationService

```json
{
  "ConnectionStrings": {
    "NotificationDb": "Host=postgres-notifications;Port=5432;Database=notificationdb;Username=notificationuser;Password=notificationpass123"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "admin",
    "Password": "admin123"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@eventplatform.com",
    "FromName": "Plataforma de Eventos"
  }
}
```

### Configurar Email (NotificationService)

Para que NotificationService envíe emails, configura las credenciales SMTP en `appsettings.json` o como variables de entorno en `docker-compose.yml`.

**Gmail**:
1. Habilitar "Contraseñas de aplicaciones" en tu cuenta de Google
2. Generar una contraseña de aplicación
3. Usar esa contraseña en `Email:SmtpPassword`

---

## 📚 Documentación

### Documentos Técnicos

- **`docs/architecture.md`** - Arquitectura completa del sistema con diagramas Mermaid
- **`docs/BACKLOG_ROADMAP.md`** - Backlog y ROADMAP de 6 meses (12 sprints)
  - ⭐ **Importable en Xmind**: El roadmap puede ser importado en el programa Xmind para visualización interactiva y gestión del plan de trabajo
  - Incluye: Backlog completo, estimaciones, asignaciones, estados de implementación
- **`docs/architectura.drawio`** - Diagrama de arquitectura en formato DrawIO
- **`docs/RESUMEN_FLUJO_COMPLETO.md`** - Resumen del flujo completo del sistema
- **`Reto Técnico Lider Técnico.md`** - Documento original del reto técnico con todos los requisitos

### Documentos de Implementación (si existen)

- `docs/ANALISIS_PUNTO_5_MENSAJERIA.md` - Análisis detallado de mensajería
- `docs/IMPLEMENTACION_PUNTO_6_PERSISTENCIA.md` - Implementación de persistencia
- `docs/IMPLEMENTACION_PUNTO_7_SEGURIDAD.md` - Implementación de seguridad
- `docs/IMPLEMENTACION_PUNTO_8_FRONTEND.md` - Implementación de frontend

### Diagramas

Los diagramas están disponibles en:
- **Formato Mermaid**: `docs/architecture.md`
  - Arquitectura general
  - Flujos de secuencia
  - Patrones de resiliencia
  - Seguridad JWT
- **Formato DrawIO**: `docs/architectura.drawio`
  - Abrir con [draw.io](https://app.diagrams.net/) o Visual Studio Code con extensión DrawIO

### Backlog y ROADMAP

El roadmap completo está disponible en `docs/BACKLOG_ROADMAP.md` e incluye:
- Visión del producto
- Equipo y metodología (Scrum, 2 semanas por sprint, 6 meses)
- Backlog principal con todos los EPICs y User Stories
- Estados de implementación (✅ COMPLETADO)
- Métricas de éxito
- Proceso de desarrollo
- Priorización (Must Have, Should Have, Could Have)

> **💡 Importar en Xmind**: El roadmap puede ser importado en Xmind para una visualización interactiva del plan de trabajo, facilitando la gestión de sprints y seguimiento del progreso.

---

## 🛠️ Desarrollo

### Estructura de Clean Architecture

Cada microservicio sigue Clean Architecture:

```
Service/
├── Domain/           # Entidades, eventos de dominio (sin dependencias)
├── Application/      # Commands, Queries, DTOs, Validators (depende de Domain)
├── Infrastructure/   # DbContext, Handlers, Consumers (depende de Application y Domain)
└── API/              # Controllers, Program.cs (depende de Application e Infrastructure)
```

### Agregar Nueva Migración

#### EventService

```bash
cd backend/EventService/EventService.Infrastructure
dotnet ef migrations add NombreMigracion --startup-project ../EventService.API
```

#### NotificationService

```bash
cd backend/NotificationService/NotificationService.Infrastructure
dotnet ef migrations add NombreMigracion --startup-project ../NotificationService.API
```

### Testing

```bash
# EventService
cd backend/EventService
dotnet test

# NotificationService
cd backend/NotificationService
dotnet test
```

---

## 🐛 Troubleshooting

### Los servicios no inician

1. **Verificar que Docker esté corriendo**
2. **Ver logs**: `docker-compose logs -f <nombre-servicio>`
3. **Verificar puertos**: Asegúrate de que los puertos no estén en uso
4. **Reconstruir imágenes**: `docker-compose build --no-cache`

### Error de conexión a base de datos

1. **Verificar que PostgreSQL esté corriendo**: `docker-compose ps`
2. **Verificar health checks**: Espera a que los servicios estén "healthy"
3. **Ver logs de PostgreSQL**: `docker-compose logs postgres-events`

### Error de conexión a RabbitMQ

1. **Verificar que RabbitMQ esté corriendo**: `docker-compose ps`
2. **Acceder a Management UI**: `http://localhost:15672` (admin/admin123)
3. **Verificar colas**: Debe existir la cola `event-created-queue`

### Frontend no se conecta al backend

1. **Verificar que EventService esté corriendo**: `http://localhost:8070/health`
2. **Verificar CORS**: El backend tiene CORS configurado para permitir todos los orígenes
3. **Verificar URL en frontend**: Debe apuntar a `http://localhost:8070`

---

## 📦 Tecnologías Utilizadas

### Backend
- .NET 9.0
- Entity Framework Core 9.0.0
- MassTransit 8.3.0 (RabbitMQ)
- MediatR 12.4.1
- AutoMapper 13.0.1
- FluentValidation 11.11.0
- Polly 8.4.2
- MailKit 4.8.0
- AspNetCoreRateLimit 5.0.0
- PostgreSQL 16
- Redis 7
- RabbitMQ 3

### Frontend
- Next.js 15.1.0
- React 18.3.1
- TypeScript 5.7.2
- Tailwind CSS 3.4.17

---

## 📝 Notas Importantes

- ⚠️ **Credenciales de demo**: Las credenciales `admin/admin` y `user/user` son solo para desarrollo. En producción, implementar autenticación real.
- ⚠️ **Email**: Configurar credenciales SMTP reales para que NotificationService envíe emails.
- ⚠️ **JWT Secret**: Cambiar el `JWT:SecretKey` en producción.
- ⚠️ **Rate Limiting**: Los límites actuales son para desarrollo. Ajustar según necesidades de producción.
- ⚠️ **Roadmap**: El roadmap detallado está en `docs/BACKLOG_ROADMAP.md` y puede ser importado en Xmind para gestión interactiva.

---

## 📊 Resumen del Proyecto

Este proyecto fue desarrollado como parte del **Reto Técnico para Líder Técnico** y cumple con todos los requisitos especificados:

- ✅ **Arquitectura**: Microservicios, event-driven, Clean Architecture + DDD
- ✅ **MVP Completo**: 2 APIs (.NET 9) + Frontend (Next.js/React)
- ✅ **Mensajería**: RabbitMQ con idempotencia, reintentos y DLQ
- ✅ **Persistencia**: PostgreSQL (DB por servicio) con EF Core
- ✅ **Seguridad**: JWT, roles, rate limiting, manejo seguro de errores
- ✅ **Documentación**: Arquitectura, roadmap, implementación
- ✅ **Docker**: Dockerfiles y Docker Compose completos

### Entregables Completados

- ✅ Diagrama de arquitectura (`docs/architecture.md` y `docs/architectura.drawio`)
- ✅ Backlog y ROADMAP (`docs/BACKLOG_ROADMAP.md` - importable en Xmind)
- ✅ Código fuente completo (Backend + Frontend)
- ✅ README con instrucciones de ejecución
- ✅ Scripts de BD y migraciones
- ✅ Dockerfiles y Docker Compose

---

## 📄 Licencia

Este proyecto es parte de un reto técnico.

---

## 👤 Autor

Desarrollado como parte del Reto Técnico para Líder Técnico.

---

## 📅 Información del Proyecto

**Estado**: ✅ MVP Completo - Todos los requisitos implementados  
**Última actualización**: 2026-01-12  
**Versión**: 1.0.0  
**Documentación del Reto**: `Reto Técnico Lider Técnico.md`
