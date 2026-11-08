namespace ChatbotTienda.Logica.Chatbot;

public interface IChatbotServicio
{
    Task<ResultadoChatbot> ResponderAsync(int conversacionId, int usuarioId, string texto);
}
