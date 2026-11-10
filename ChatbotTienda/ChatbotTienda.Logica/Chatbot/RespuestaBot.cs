namespace ChatbotTienda.Logica.Chatbot;

// Lo que le pedimos al modelo (salida estructurada): solo el nombre de la intención.
// El modelo NO redacta la respuesta: el texto sale de Intencion.RespuestaBase en la base de datos,
// así el bot nunca inventa datos (precios, stock, etc.).
// "required" hace que el esquema JSON marque la propiedad como obligatoria.
public class RespuestaBot
{
    public required string Intencion { get; set; }
}
