
namespace ChatbotTienda.Logica.Estadisticas;

public interface IEstadisticasServicio
{
    Task<EstadisticasDto> ObtenerEstadisticasAsync();
}
