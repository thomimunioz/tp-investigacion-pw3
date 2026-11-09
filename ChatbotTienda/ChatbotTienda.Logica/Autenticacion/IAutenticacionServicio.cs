using ChatbotTienda.Entidades;

namespace ChatbotTienda.Logica.Autenticacion
{
    public interface IAutenticacionServicio
    {
        /// <summary>Crea un usuario con rol Cliente. Devuelve null si el email ya existe.</summary>
        Task<Usuario?> RegistrarAsync(string nombre, string email, string password);

        /// <summary>Devuelve el usuario si las credenciales son correctas, o null.</summary>
        Task<Usuario?> ValidarCredencialesAsync(string email, string password);
    }
}
