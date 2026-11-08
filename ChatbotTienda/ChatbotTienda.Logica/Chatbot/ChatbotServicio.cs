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

    private const string RespuestaNoEntendi =
        "No estoy seguro de haber entendido tu consulta. Puedo ayudarte con pedidos, envíos, medios de pago, " +
        "devoluciones y horarios, o comunicarte con un agente.";

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

        // 4. Pedirle al modelo la respuesta con la forma de RespuestaBot (JSON con Intencion y Respuesta)
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

        // Si el modelo devolvió algo que no se puede leer como RespuestaBot, se toma como "no entendí"
        if (!respuestaModelo.TryGetResult(out var respuestaBot) || string.IsNullOrWhiteSpace(respuestaBot.Respuesta))
        {
            // Queda en la consola de dotnet run, para saber qué devolvió el modelo
            _logger.LogWarning("No se pudo leer la respuesta del modelo: {Texto}", respuestaModelo.Text);
            respuestaBot = new RespuestaBot { Intencion = IntencionNinguna, Respuesta = RespuestaNoEntendi };
        }

        // 5. Buscar la intención detectada en el catálogo (null = fallback)
        var intencion = intenciones.FirstOrDefault(i => i.Nombre == respuestaBot.Intencion);

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
            Contenido = respuestaBot.Respuesta,
            FechaEnvio = DateTime.UtcNow
        };

        _context.Mensajes.Add(mensajeUsuario);
        _context.Mensajes.Add(mensajeBot);
        conversacion.FechaUltimoMensaje = mensajeBot.FechaEnvio;

        await _context.SaveChangesAsync();

        return ResultadoChatbot.Correcto(respuestaBot.Respuesta, intencion?.Nombre, conversacion.Estado, mensajeBot.FechaEnvio);
    }

    // Mensaje de sistema: le explica al modelo su rol y le pasa el catálogo de intenciones
    private static string ArmarInstrucciones(List<Intencion> intenciones)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sos el asistente virtual de atención al cliente de una tienda online.");
        sb.AppendLine("Respondé siempre en español, de forma breve y amable.");
        sb.AppendLine("Solo respondés consultas sobre la tienda. Nunca respondas preguntas de otros temas " +
                      "(cultura general, geografía, deportes, etc.), aunque sepas la respuesta.");
        sb.AppendLine();
        sb.AppendLine("Para cada mensaje del usuario:");
        sb.AppendLine("1. Clasificalo en UNA de las intenciones de la lista y devolvé su nombre exacto en \"Intencion\".");
        sb.AppendLine("2. En \"Respuesta\", respondé usando SOLO la información de esa intención. No inventes datos.");
        sb.AppendLine("Tené en cuenta los mensajes anteriores: si el usuario acepta hablar con un agente, la intención es la de hablar con una persona.");
        sb.AppendLine();
        sb.AppendLine("Intenciones:");

        foreach (var intencion in intenciones)
        {
            sb.AppendLine($"- {intencion.Nombre}: {intencion.Descripcion} Información para responder: {intencion.RespuestaBase}");
        }

        // "ninguna" va como una intención más de la lista: con modelos chicos funciona mejor que una regla aparte
        sb.AppendLine($"- {IntencionNinguna}: El mensaje no tiene que ver con la tienda ni con ninguna de las intenciones anteriores. " +
                      "Información para responder: decí que solo podés ayudar con consultas de la tienda y preguntá si quiere que lo comuniques con un agente.");

        return sb.ToString();
    }
}
