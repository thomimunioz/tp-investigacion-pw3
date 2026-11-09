using System.Security.Claims;
using ChatbotTienda.Logica.Chatbot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatbotTienda.Web.Models;

namespace ChatbotTienda.Web.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly ChatbotDbContext _context;
    private readonly IChatbotServicio _chatbotServicio;

    public ChatController(ChatbotDbContext context, IChatbotServicio chatbotServicio)
    {
        _context = context;
        _chatbotServicio = chatbotServicio;
    }

    // GET: /Chat/Index/{id}.
    public async Task<IActionResult> Index(int id)
    {
        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId == null)
            return RedirectToAction("Login", "Auth");

        var conversacion = await _context.Conversaciones
            .Include(c => c.Mensajes)
            .Include(c => c.Valoracion)
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId.Value);

        if (conversacion == null)
            return NotFound("La conversación no existe o no tenés permiso para verla.");

        return View(conversacion);
    }

    // POST: /Chat/Enviar.
    [HttpPost]
    public async Task<IActionResult> Enviar([FromBody] EnviarMensajeDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Mensaje))
            return BadRequest(new { ok = false, error = "El mensaje no puede estar vacío." });

        int convId = dto.ConversacionId ?? 0;
        if (convId <= 0)
            return BadRequest(new { ok = false, error = "ID de conversación inválido." });

        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId == null)
            return Unauthorized();

        var resultado = await _chatbotServicio.ResponderAsync(convId, usuarioId.Value, dto.Mensaje);
        if (!resultado.Exito)
            return BadRequest(new { ok = false, error = resultado.Error });

        return Json(new
        {
            ok = true,
            respuesta = resultado.Respuesta,
            intencion = resultado.Intencion,
            fecha = resultado.FechaEnvio.ToLocalTime().ToString("HH:mm"),
            estado = resultado.Estado.ToString()
        });
    }

    // POST: /Chat/Finalizar
    // ACÁ ESTÁ LA MAGIA NUEVA QUE GUARDA EN LA BASE DE DATOS
    [HttpPost]
    public async Task<IActionResult> Finalizar(int conversacionId, int estrellas, string comentario)
    {
        // Buscamos el chat
        var conversacion = await _context.Conversaciones.FindAsync(conversacionId);
        if (conversacion != null)
        {
            // Lo marcamos como Resuelta
            conversacion.Estado = ChatbotTienda.Entidades.Enums.EstadoConversacion.Resuelta;

            // Armamos la valoración con los datos que llegan de la vista
            var valoracion = new ChatbotTienda.Entidades.Valoracion
            {
                ConversacionId = conversacionId,
                Puntaje = estrellas,
                Comentario = comentario ?? "", // Si no escriben nada, guarda vacío
                Fecha = DateTime.UtcNow
            };

            // Guardamos en la base de datos
            _context.Valoraciones.Add(valoracion);
            await _context.SaveChangesAsync();
        }

        // Te redirige a la lista general de conversaciones
        return RedirectToAction("Index", "Conversaciones");
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int id))
            return id;

        return null;
    }
}