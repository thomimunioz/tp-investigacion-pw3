using ChatbotTienda.Entidades.Enums;

namespace ChatbotTienda.Logica.Chatbot;

public class ResultadoChatbot
{
    public bool Exito { get; set; }
    public string Error { get; set; } = "";
    public string Respuesta { get; set; } = "";
    public string? Intencion { get; set; }
    public EstadoConversacion Estado { get; set; }
    public DateTime FechaEnvio { get; set; }

    public static ResultadoChatbot Correcto(string respuesta, string? intencion, EstadoConversacion estado, DateTime fechaEnvio)
    {
        return new ResultadoChatbot
        {
            Exito = true,
            Respuesta = respuesta,
            Intencion = intencion,
            Estado = estado,
            FechaEnvio = fechaEnvio
        };
    }

    public static ResultadoChatbot Fallo(string error)
    {
        return new ResultadoChatbot
        {
            Exito = false,
            Error = error
        };
    }
}
