
using ChatbotTienda.Logica.Estadisticas;
using ChatbotTienda.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTienda.Web.Controllers;

public class EstadisticasController : Controller
{
    private readonly IEstadisticasServicio _estadisticasServicio;

    public EstadisticasController(
        IEstadisticasServicio estadisticasServicio)
    {
        _estadisticasServicio = estadisticasServicio;
    }

    public async Task<IActionResult> Index()
    {
        var estadisticas = await
            _estadisticasServicio.ObtenerEstadisticasAsync();

        var viewModel = new EstadisticasViewModel
        {
            TasaContencion = estadisticas.TasaContencion,
            Csat = estadisticas.CsAT,
            TasaFallback = estadisticas.TasaFallback,
            TotalConversaciones = estadisticas.TotalConversaciones,
            TotalMensajesUsuario = estadisticas.TotalMensajesUsuario,
            TotalValoraciones = estadisticas.TotalValoraciones,

            MensajesPorIntencion = estadisticas.MensajesPorIntencion
                .Select(m => new MensajePorIntencionViewModel
                {
                    Intencion = m.Intencion,
                    Cantidad = m.Cantidad
                })
                .ToList()
        };

        return View(viewModel);
    }
}
