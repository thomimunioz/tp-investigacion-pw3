namespace ChatbotTienda.Logica.Chatbot;

// Forma de la respuesta que le pedimos al modelo (salida estructurada).
// El modelo tiene que devolver un JSON con estas dos propiedades.
// "required" hace que el esquema JSON que se le manda al modelo las marque como obligatorias:
// sin eso, a veces devolvía solo la intención y dejaba la respuesta afuera.
public class RespuestaBot
{
    public required string Intencion { get; set; }
    public required string Respuesta { get; set; }
}
