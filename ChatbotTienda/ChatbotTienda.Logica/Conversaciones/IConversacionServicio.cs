
namespace ChatbotTienda.Logica.Conversaciones;

public interface IConversacionServicio
{
    Task<FinalizarConversacionResultado> FinalizarYValorarAsync(
        int conversacionId,
        int usuarioId,
        int puntaje,
        string? comentario
    );
    Task FinalizarYValorarAsync(int conversacionId, int estrellas, string comentario);
}
