# ChatbotTienda

Aplicación demo del Trabajo Práctico de Investigación de Programación Web III (UNLaM, 2.º cuatrimestre 2026), grupo 5: **Procesamiento de lenguaje natural (chatbots)**.

Es un chatbot de atención al cliente para una tienda online. Clasifica cada mensaje en una intención (estado del pedido, medios de pago, envíos, devoluciones, etc.), responde con la información cargada para esa intención, deriva a un agente humano cuando hace falta y permite valorar la conversación. Un panel para administradores muestra las métricas del bot: tasa de contención, CSAT y tasa de fallback.

## Tecnologías

- .NET 10, ASP.NET Core MVC
- Entity Framework Core 10 sobre SQL Server
- Microsoft.Extensions.AI (`IChatClient`) + OllamaSharp
- Ollama con el modelo `llama3.2`, que corre en la máquina local

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server Express (`localhost`) u otro SQL Server
- Herramienta de Entity Framework: `dotnet tool install --global dotnet-ef`
- [Ollama](https://ollama.com/download) y el modelo descargado: `ollama pull llama3.2`

## Estructura

| Proyecto | Contenido |
|---|---|
| `ChatbotTienda.Entidades` | Entidades, `DbContext` y migraciones |
| `ChatbotTienda.Logica` | Servicios con la lógica de negocio (autenticación, conversaciones, chatbot, métricas) |
| `ChatbotTienda.Web` | Aplicación MVC: controladores, vistas y ViewModels |

## Cómo correrlo

Todos los comandos se ejecutan desde la carpeta raíz del repositorio.

1. **Configurar la base de datos.** Si tu SQL Server no es `localhost`, cambiá la cadena `ChatbotTienda` en `ChatbotTienda/ChatbotTienda.Web/appsettings.json`.
2. **Crear la base con las migraciones:**
   ```
   dotnet ef database update --project ChatbotTienda/ChatbotTienda.Entidades --startup-project ChatbotTienda/ChatbotTienda.Web
   ```
3. **Levantar Ollama** (si no quedó corriendo como servicio): `ollama serve`
4. **Levantar la aplicación:**
   ```
   dotnet run --project ChatbotTienda/ChatbotTienda.Web --launch-profile https
   ```
   Abrir https://localhost:7091.

## Forma de trabajo

- Cada tarjeta de Trello se trabaja en su propio branch (`feature/nombre-corto`) y se integra a `main` con un Pull Request revisado por otro integrante.
- No se borran los branches después de mergear.
