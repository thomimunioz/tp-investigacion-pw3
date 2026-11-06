
using ChatbotTienda.Entidades.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatbotTienda.Logica.Estadisticas;

public class EstadisticasServicio : IEstadisticasServicio
{
    private readonly ChatbotDbContext _context;

    public EstadisticasServicio(ChatbotDbContext context)
    {
        _context = context;
    }

    public async Task<EstadisticasDto> ObtenerEstadisticasAsync()
    {
        
        int totalConversaciones = await _context.Conversaciones.CountAsync();

        int conversacionesResueltas = await _context.Conversaciones
            .CountAsync(c => c.Estado == EstadoConversacion.Resuelta);

        decimal tasaContencion = totalConversaciones == 0
            ? 0
            : (decimal)conversacionesResueltas / totalConversaciones * 100;


        
        int totalValoraciones = await _context.Valoraciones.CountAsync();

        int valoracionesPositivas = await _context.Valoraciones
            .CountAsync(v => v.Puntaje >= 4);

        decimal csat = totalValoraciones == 0
            ? 0
            : (decimal)valoracionesPositivas / totalValoraciones * 100;


        
        int totalMensajesUsuario = await _context.Mensajes
            .CountAsync(m => m.Emisor == EmisorMensaje.Usuario);

        int mensajesSinIntencion = await _context.Mensajes
            .CountAsync(m =>
                m.Emisor == EmisorMensaje.Usuario &&
                m.IntencionId == null);

        decimal tasaFallback = totalMensajesUsuario == 0
            ? 0
            : (decimal)mensajesSinIntencion / totalMensajesUsuario * 100;


        
        var mensajesPorIntencion = await _context.Mensajes
            .Where(m =>
                m.Emisor == EmisorMensaje.Usuario &&
                m.IntencionId != null)
            .GroupBy(m => m.Intencion!.Nombre)
            .Select(g => new MensajePorIntencionDto
            {
                Intencion = g.Key,
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .ToListAsync();


        return new EstadisticasDto
        {
            TasaContencion = tasaContencion,
            CsAT = csat,
            TasaFallback = tasaFallback,
            TotalConversaciones = totalConversaciones,
            TotalMensajesUsuario = totalMensajesUsuario,
            TotalValoraciones = totalValoraciones,
            MensajesPorIntencion = mensajesPorIntencion
        };
    }
}
