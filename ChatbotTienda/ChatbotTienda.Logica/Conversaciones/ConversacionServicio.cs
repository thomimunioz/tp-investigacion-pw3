
using ChatbotTienda.Entidades;
using ChatbotTienda.Entidades.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatbotTienda.Logica.Conversaciones;

public class ConversacionServicio : IConversacionServicio
{
    private readonly ChatbotDbContext _context;

    public ConversacionServicio(ChatbotDbContext context)
    {
        _context = context;
    }

    public async Task<FinalizarConversacionResultado> FinalizarYValorarAsync(
        int conversacionId,
        int usuarioId,
        int puntaje,
        string? comentario)
    {
        // 1. Validar el puntaje
        if (puntaje < 1 || puntaje > 5)
        {
            return FinalizarConversacionResultado.Error(
                "El puntaje debe estar entre 1 y 5.");
        }

        // 2. Buscar la conversación del usuario
        var conversacion = await _context.Conversaciones
            .Include(c => c.Valoracion)
            .FirstOrDefaultAsync(c =>
                c.Id == conversacionId &&
                c.UsuarioId == usuarioId);

        if (conversacion == null)
        {
            return FinalizarConversacionResultado.Error(
                "No se encontró la conversación.");
        }

        // 3. Evitar valoraciones duplicadas
        if (conversacion.Valoracion != null)
        {
            return FinalizarConversacionResultado.Error(
                "Esta conversación ya fue valorada.");
        }

        // 4. Verificar que siga activa
        if (conversacion.Estado != EstadoConversacion.Activa)
        {
            return FinalizarConversacionResultado.Error(
                "La conversación ya no se encuentra activa.");
        }

        // 5. Crear la valoración
        var valoracion = new Valoracion
        {
            ConversacionId = conversacion.Id,
            Puntaje = puntaje,
            Comentario = string.IsNullOrWhiteSpace(comentario)
                ? null
                : comentario.Trim(),
            Fecha = DateTime.UtcNow
        };

        // 6. Cambiar el estado de la conversación
        conversacion.Estado = EstadoConversacion.Resuelta;

        // 7. Guardar ambos cambios
        _context.Valoraciones.Add(valoracion);

        await _context.SaveChangesAsync();

        return FinalizarConversacionResultado.Correcto();
    }

    public async Task FinalizarYValorarAsync(int conversacionId, int estrellas, string comentario)
    {
        // Buscamos el chat actual
        var conversacion = await _context.Conversaciones.FindAsync(conversacionId);
        if (conversacion != null)
        {
            // 1. Cerramos el chat (fijate que el Enum se llame así, si no cambialo por el que usen)
            conversacion.Estado = ChatbotTienda.Entidades.Enums.EstadoConversacion.Resuelta;

            // 2. Creamos la valoración
            var valoracion = new ChatbotTienda.Entidades.Valoracion
            {
                ConversacionId = conversacionId,
                Puntaje = estrellas,
                Comentario = comentario,
                Fecha = DateTime.UtcNow
            };

            _context.Valoraciones.Add(valoracion);
            await _context.SaveChangesAsync();
        }
    }
}
