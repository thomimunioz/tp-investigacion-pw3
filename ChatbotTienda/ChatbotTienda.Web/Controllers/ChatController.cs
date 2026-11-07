using System.Security.Claims;
using ChatbotTienda.Entidades;
using ChatbotTienda.Entidades.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatbotTienda.Web.Models;

namespace ChatbotTienda.Web.Controllers;

[Authorize]
public class ChatController : Controller
    {
    private readonly ChatbotDbContext _context;

    public ChatController(ChatbotDbContext context)
        {
        _context = context;
        }

    // GET: /Chat/Index/{id}.
    public async Task<IActionResult> Index(int id)
        {

        var usuarioId = ObtenerUsuarioIdActual();
        if (usuarioId == null)
            return RedirectToAction("Login", "Auth");

        var conversacion = await _context.Conversaciones.Include(c => c.Mensajes).Include(c => c.Valoracion).FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId.Value);
        if (conversacion == null)
            return NotFound("La conversación no existe o no tenés permiso para verla.");

        return View(conversacion);
        }

    // POST: /Chat/Enviar.
    [HttpPost]
    public async Task<IActionResult> Enviar([FromBody] EnviarMensajeDto? dto)
        {
        try
            {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Mensaje))
                return BadRequest(new { ok = false, error = "El mensaje no puede estar vacío." });

            int convId = dto.ConversacionId ?? 0;
            if (convId <= 0)
                return BadRequest(new { ok = false, error = "ID de conversación inválido." });

            var usuarioId = ObtenerUsuarioIdActual();
            if (usuarioId == null)
                return Unauthorized();

            var conversacion = await _context.Conversaciones.Include(c => c.Mensajes).FirstOrDefaultAsync(c => c.Id == convId && c.UsuarioId == usuarioId.Value);
            if (conversacion == null)
                return NotFound(new { ok = false, error = "Conversación no encontrada o sin acceso." });

            if (conversacion.Estado != EstadoConversacion.Activa)
                return BadRequest(new { ok = false, error = "Esta conversación ya no está activa." });

            // 1. Guardar mensaje del usuario.
            var mensajeUsuario = new Mensaje
                {
                ConversacionId = conversacion.Id,
                Emisor = EmisorMensaje.Usuario,
                Contenido = dto.Mensaje.Trim(),
                FechaEnvio = DateTime.UtcNow
                };
            _context.Mensajes.Add(mensajeUsuario);

            // 2. Lógica de respuesta del bot/intenciones.
            string respuestaBot;
            string textoLower = dto.Mensaje.ToLower();
            if (textoLower.Contains("humano") || textoLower.Contains("persona") || textoLower.Contains("representante") || textoLower.Contains("agente"))
                {
                conversacion.Estado = EstadoConversacion.Derivada;
                respuestaBot = "Entiendo. Te voy a comunicar con un agente humano para que continúe con la atención.";
                }
            else if (textoLower.Contains("hola") || textoLower.Contains("buenas") || textoLower.Contains("buen dia"))
                {
                respuestaBot = "¡Hola! Soy el asistente virtual de la tienda. ¿En qué puedo ayudarte?";
                }
            else if (textoLower.Contains("pedido") || textoLower.Contains("donde esta") || textoLower.Contains("seguimiento"))
                {
                respuestaBot = "Podés ver el estado de tu pedido en la sección Mis pedidos, con tu número de seguimiento.";
                }
            else if (textoLower.Contains("pago") || textoLower.Contains("tarjeta") || textoLower.Contains("cuota") || textoLower.Contains("transferencia"))
                {
                respuestaBot = "Aceptamos tarjetas de crédito y débito, transferencia y efectivo. Consultá las cuotas disponibles al finalizar la compra.";
                }
            else if (textoLower.Contains("devol") || textoLower.Contains("cambio"))
                {
                respuestaBot = "Podés pedir un cambio o devolución dentro de los 10 días de recibido el producto, sin uso y en su empaque original.";
                }
            else if (textoLower.Contains("envio") || textoLower.Contains("costo") || textoLower.Contains("gratis"))
                {
                respuestaBot = "El costo del envío depende de tu código postal y se calcula al finalizar la compra. Hay envío gratis a partir de cierto monto.";
                }
            else if (textoLower.Contains("horario") || textoLower.Contains("atienden") || textoLower.Contains("abierto"))
                {
                respuestaBot = "Atendemos de lunes a viernes de 9 a 18 hs.";
                }
            else if (textoLower.Contains("chau") || textoLower.Contains("gracias") || textoLower.Contains("adios"))
                {
                respuestaBot = "¡De nada! Si necesitás algo más, acá estoy. ¡Que tengas un buen día!";
                }
            else
                {
                respuestaBot = "No estoy seguro de haber entendido tu consulta. Podés preguntarme por envíos, medios de pago, devoluciones o pedir hablar con un agente humano.";
                }

            // 3. Guardar mensaje del bot.
            var mensajeBot = new Mensaje
                {
                ConversacionId = conversacion.Id,
                Emisor = EmisorMensaje.Bot,
                Contenido = respuestaBot,
                FechaEnvio = DateTime.UtcNow
                };
            _context.Mensajes.Add(mensajeBot);

            conversacion.FechaUltimoMensaje = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new
                {
                ok = true,
                respuesta = respuestaBot,
                fecha = mensajeBot.FechaEnvio.ToLocalTime().ToString("HH:mm"),
                estado = conversacion.Estado.ToString()
                });
            } catch (Exception ex)
            {
            return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }

    private int? ObtenerUsuarioIdActual()
        {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int id))
            return id;

        return null;
        }
    }