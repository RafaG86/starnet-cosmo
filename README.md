# Starnet Cosmo 🚀

> **CRM Personal de Prospección & Centro de Automatización para STARNET**  
> Aplicación de escritorio desarrollada en **.NET 9 / ASP.NET Core** con almacenamiento local en **SQLite**, diseñada para capturar, calificar y gestionar prospectos (Leads) de iglesias para el ecosistema **EkklesiApp**, **ChordSync** y **SermonSync**.

---

## 🎯 Propósito
Este proyecto es una herramienta personal de aprendizaje y productividad para:
1. **Gestión de Leads Eclesiales:** Organizar prospectos según la metodología comercial oficial de STARNET (**T-O-N-D-M** para calificación y **E-V-P-R-C** para objeciones).
2. **API Abierta para Automatizaciones:** Disponer de endpoints REST locales (Kestrel) para que scrapers (Google Maps, redes sociales, webhooks, bots de WhatsApp) puedan inyectar leads directamente a la base de datos local SQLite.
3. **Cockpit de Ventas en Vivo:** Interfaz de escritorio con scripts directos a WhatsApp, calculadora de planes, matriz de objeciones y exportador automático de expedientes para **STARNET HUB**.

---

## 🛠️ Tecnologías
- **Framework:** .NET 9 (C# 13)
- **Base de Datos:** SQLite con Entity Framework Core 9 (migraciones automáticas)
- **Servidor Integrado:** ASP.NET Core Kestrel + Swagger / OpenAPI
- **Interfaz de Escritorio:** Desktop Host (WebView2)
- **Automatización:** REST API, Webhooks, soporte para scrapers en Python/Node.js

---

## 📂 Estructura del Proyecto
- `StarnetCosmo.Core/`: Entidades del dominio (`Lead`, `Interaccion`), contexto de BD (`StarnetDbContext`), migraciones y lógica de negocio.
- `StarnetCosmo.Api/`: Controladores REST, documentación Swagger y endpoints de ingesta masiva (`/api/leads/bulk`).
- `StarnetCosmo.Desktop/`: Interfaz de usuario de escritorio y dashboard.
- `tools/automations/`: Scripts de prueba y scrapers automatizados para inyección de prospectos.

---

## 🚀 Inicio Rápido
```bash
# Restaurar dependencias
dotnet restore

# Compilar proyecto
dotnet build

# Ejecutar aplicación
dotnet run --project StarnetCosmo.Desktop
```
*API local y documentación Swagger disponibles al iniciar en:* `http://localhost:5100/swagger`
