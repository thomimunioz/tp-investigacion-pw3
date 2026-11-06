
namespace ChatbotTienda.Web.Models;

public class EstadisticasViewModel
{
    public decimal TasaContencion { get; set; }

    public decimal Csat { get; set; }

    public decimal TasaFallback { get; set; }

    public int TotalConversaciones { get; set; }

    public int TotalMensajesUsuario { get; set; }

    public int TotalValoraciones { get; set; }

    public List<MensajePorIntencionViewModel> MensajesPorIntencion { get; set; } = new();
}

public class MensajePorIntencionViewModel
{
    public string Intencion { get; set; } = "";

    public int Cantidad { get; set; }
}
