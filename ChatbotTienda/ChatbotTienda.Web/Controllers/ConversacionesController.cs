using System.Security.Claims;
using ChatbotTienda.Entidades;
using ChatbotTienda.Entidades.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatbotTienda.Web.Controllers
    {
    
    [Authorize]
    public class ConversacionesController : Controller
        {

        private readonly ChatbotDbContext _context;
        public ConversacionesController(ChatbotDbContext context)
            {
            this._context = context;
            }

        // GET: /Conversaciones
        public async Task<IActionResult> Index()
            {
            var usuarioId = ObtenerUsuarioIdActual();
            if (usuarioId == null)
                {
                return RedirectToAction("Login", "Auth");
                }

            var conversaciones = await _context.Conversaciones
            .Where(c => c.UsuarioId == usuarioId.Value)
            .OrderByDescending(c => c.FechaUltimoMensaje)
            .ToListAsync();

            return View(conversaciones);
            }

        // POST: /Conversaciones/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear()
            {
            var usuarioId = ObtenerUsuarioIdActual();
            if (usuarioId == null)
                return Unauthorized();

            var nuevaConversacion = new Conversacion
                {
                UsuarioId = usuarioId.Value,
                Titulo = $"Consulta {DateTime.Now:dd/MM/yyyy HH:mm}",
                FechaInicio = DateTime.UtcNow,
                FechaUltimoMensaje = DateTime.UtcNow,
                Estado = EstadoConversacion.Activa
                };

            this._context.Conversaciones.Add(nuevaConversacion);
            
            await _context.SaveChangesAsync();
           
            return RedirectToAction("Index", "Chat", new
                {
                id = nuevaConversacion.Id
                });
            }

        private int? ObtenerUsuarioIdActual()
            {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int id))
                return id;

            return null;
            }
        }
    }