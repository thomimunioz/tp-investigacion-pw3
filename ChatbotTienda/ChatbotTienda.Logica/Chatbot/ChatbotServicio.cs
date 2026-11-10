using System.Text;
using ChatbotTienda.Entidades;
using ChatbotTienda.Entidades.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace ChatbotTienda.Logica.Chatbot;

public class ChatbotServicio : IChatbotServicio
{
    // Cantidad de mensajes anteriores que se le reenvían al modelo (memoria a corto plazo).
    private const int MensajesDeHistorial = 10;

    private const string IntencionNinguna = "ninguna";

    // Respuesta cuando el mensaje no corresponde a ninguna intención (fallback):
    // dice lo que el bot SÍ puede hacer y ofrece un agente.
    private const string RespuestaNoEntendi =
        "No tengo información sobre eso. Puedo ayudarte con el estado de tu pedido, medios de pago, costos de envío, " +
        "cambios y devoluciones, horarios de atención y consultas sobre productos. Si querés, también te puedo comunicar con un agente.";

    private readonly ChatbotDbContext _context;
    private readonly IChatClient _chatClient;
    private readonly ILogger<ChatbotServicio> _logger;

    public ChatbotServicio(ChatbotDbContext context, IChatClient chatClient, ILogger<ChatbotServicio> logger)
    {
        _context = context;
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<ResultadoChatbot> ResponderAsync(int conversacionId, int usuarioId, string texto)
    {
        texto = texto.Trim();
        if (texto == "")
        {
            return ResultadoChatbot.Fallo("El mensaje no puede estar vacío.");
        }

        // 1. Buscar la conversación del usuario
        var conversacion = await _context.Conversaciones
            .FirstOrDefaultAsync(c => c.Id == conversacionId && c.UsuarioId == usuarioId);

        if (conversacion == null)
        {
            return ResultadoChatbot.Fallo("Conversación no encontrada o sin acceso.");
        }

        if (conversacion.Estado != EstadoConversacion.Activa)
        {
            return ResultadoChatbot.Fallo("Esta conversación ya no está activa.");
        }

        // 2. Leer el catálogo de intenciones y los últimos mensajes
        var intenciones = await _context.Intenciones
            .Where(i => i.Activa)
            .ToListAsync();

        var historial = await _context.Mensajes
            .Where(m => m.ConversacionId == conversacionId)
            .OrderByDescending(m => m.FechaEnvio)
            .Take(MensajesDeHistorial)
            .ToListAsync();
        historial.Reverse(); // del más viejo al más nuevo

        // 3. Armar la lista de mensajes para el modelo: instrucciones + historial + mensaje nuevo
        var mensajes = new List<ChatMessage>();
        mensajes.Add(new ChatMessage(ChatRole.System, ArmarInstrucciones(intenciones)));

        foreach (var mensaje in historial)
        {
            var rol = mensaje.Emisor == EmisorMensaje.Usuario ? ChatRole.User : ChatRole.Assistant;
            mensajes.Add(new ChatMessage(rol, mensaje.Contenido));
        }

        mensajes.Add(new ChatMessage(ChatRole.User, texto));

        // 4. Pedirle al modelo que CLASIFIQUE el mensaje (devuelve solo el nombre de la intención)
        ChatResponse<RespuestaBot> respuestaModelo;
        try
        {
            respuestaModelo = await _chatClient.GetResponseAsync<RespuestaBot>(
                mensajes,
                new ChatOptions { Temperature = 0 });
        }
        catch (HttpRequestException)
        {
            return ResultadoChatbot.Fallo("El asistente no está disponible. Revisá que Ollama esté corriendo.");
        }

        string nombreIntencion = IntencionNinguna;
        if (respuestaModelo.TryGetResult(out var respuestaBot) && !string.IsNullOrWhiteSpace(respuestaBot.Intencion))
        {
            nombreIntencion = respuestaBot.Intencion.Trim();
        }
        else
        {
            // Queda en la consola de dotnet run, para saber qué devolvió el modelo
            _logger.LogWarning("No se pudo leer la respuesta del modelo: {Texto}", respuestaModelo.Text);
        }

        // 5. Buscar la intención en el catálogo (null = fallback) y elegir la respuesta.
        // La respuesta sale de la base de datos, nunca del modelo: el bot solo dice lo que está cargado.
        var intencion = intenciones.FirstOrDefault(i => i.Nombre == nombreIntencion);
        var textoRespuesta = intencion != null ? intencion.RespuestaBase : RespuestaNoEntendi;

        if (intencion != null && intencion.RequiereDerivacion)
        {
            conversacion.Estado = EstadoConversacion.Derivada;
        }

        // 6. Guardar los dos mensajes y actualizar la conversación
        var mensajeUsuario = new Mensaje
        {
            ConversacionId = conversacion.Id,
            Emisor = EmisorMensaje.Usuario,
            Contenido = texto,
            IntencionId = intencion?.Id,
            FechaEnvio = DateTime.UtcNow
        };

        var mensajeBot = new Mensaje
        {
            ConversacionId = conversacion.Id,
            Emisor = EmisorMensaje.Bot,
            Contenido = textoRespuesta,
            FechaEnvio = DateTime.UtcNow
        };

        _context.Mensajes.Add(mensajeUsuario);
        _context.Mensajes.Add(mensajeBot);
        conversacion.FechaUltimoMensaje = mensajeBot.FechaEnvio;

        await _context.SaveChangesAsync();

        return ResultadoChatbot.Correcto(textoRespuesta, intencion?.Nombre, conversacion.Estado, mensajeBot.FechaEnvio);
    }

    // Mensaje de sistema: le explica al modelo que solo tiene que clasificar y le pasa el catálogo de intenciones
    private static string ArmarInstrucciones(List<Intencion> intenciones)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sos el clasificador de mensajes del asistente virtual de una tienda online.");
        sb.AppendLine("Tu única tarea es decidir a qué intención de la lista corresponde el último mensaje del usuario " +
                      "y devolver su nombre exacto en \"intencion\". No respondas al usuario.");
        sb.AppendLine($"Si el mensaje no corresponde exactamente a ninguna intención, aunque sea sobre la tienda " +
                      $"(por ejemplo facturación, sucursales o promociones) o sobre otro tema, devolvé \"{IntencionNinguna}\".");
        sb.AppendLine("Tené en cuenta los mensajes anteriores: si el asistente ofreció comunicarlo con un agente y el usuario acepta, " +
                      "la intención es la de hablar con una persona.");
        sb.AppendLine();
        sb.AppendLine("Intenciones:");

        // La RespuestaBase va como contexto para que el modelo entienda de qué trata cada intención
        // (el modelo no la escribe: la usa la aplicación para responder)
        foreach (var intencion in intenciones)
        {
            sb.AppendLine($"- {intencion.Nombre}: {intencion.Descripcion} (Lo que se le responde: {intencion.RespuestaBase})");
        }

        sb.AppendLine($"- {IntencionNinguna}: El mensaje no corresponde a ninguna de las intenciones anteriores.");

        return sb.ToString();
    }
}
