
namespace ChatbotTienda.Logica.Estadisticas;

public class EstadisticasDto
{
    public decimal TasaContencion { get; set; }

    public decimal CsAT { get; set; }

    public decimal TasaFallback { get; set; }

    public int TotalConversaciones { get; set; }

    public int TotalMensajesUsuario { get; set; }

    public int TotalValoraciones { get; set; }

    public List<MensajePorIntencionDto> MensajesPorIntencion { get; set; } = new();
}

public class MensajePorIntencionDto
{
    public string Intencion { get; set; } = "";

    public int Cantidad { get; set; }
}
