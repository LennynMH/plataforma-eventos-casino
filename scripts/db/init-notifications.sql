-- Script de inicialización para NotificationService Database
-- Este script se ejecuta automáticamente al crear el contenedor PostgreSQL
-- Las tablas se crearán mediante EF Core Migrations al iniciar la aplicación

-- Crear schema si no existe
CREATE SCHEMA IF NOT EXISTS notifications;

-- Nota: Las tablas (NotificationLogs, ProcessedMessages) se crearán automáticamente mediante
-- EF Core Migrations cuando la aplicación NotificationService se inicie.
-- Ver: backend/NotificationService/NotificationService.Infrastructure/Migrations/
