
namespace ChatbotTienda.Logica.Conversaciones;

public class FinalizarConversacionResultado
{
    public bool Exito { get; set; }

    public string Mensaje { get; set; } = "";

    public static FinalizarConversacionResultado Correcto()
    {
        return new FinalizarConversacionResultado
        {
            Exito = true,
            Mensaje = "La conversación fue finalizada y valorada correctamente."
        };
    }

    public static FinalizarConversacionResultado Error(string mensaje)
    {
        return new FinalizarConversacionResultado
        {
            Exito = false,
            Mensaje = mensaje
        };
    }
}
