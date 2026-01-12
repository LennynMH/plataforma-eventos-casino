-- Script de inicialización para EventService Database
-- Este script se ejecuta automáticamente al crear el contenedor PostgreSQL
-- Las tablas se crearán mediante EF Core Migrations al iniciar la aplicación

-- Crear schema si no existe
CREATE SCHEMA IF NOT EXISTS events;

-- Nota: Las tablas (Events, Zones) se crearán automáticamente mediante
-- EF Core Migrations cuando la aplicación EventService se inicie.
-- Ver: backend/EventService/EventService.Infrastructure/Migrations/
