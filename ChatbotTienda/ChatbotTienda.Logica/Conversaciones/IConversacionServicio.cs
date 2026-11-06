
namespace ChatbotTienda.Logica.Conversaciones;

public interface IConversacionServicio
{
    Task<FinalizarConversacionResultado> FinalizarYValorarAsync(
        int conversacionId,
        int usuarioId,
        int puntaje,
        string? comentario
    );
}
