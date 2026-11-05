using ChatbotTienda.Entidades.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatbotTienda.Entidades
{
    public class Conversacion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public string Titulo { get; set; } = "";
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
        public DateTime FechaUltimoMensaje { get; set; } = DateTime.UtcNow;
        public EstadoConversacion Estado { get; set; } = EstadoConversacion.Activa;
        public List<Mensaje> Mensajes { get; set; } = new();
        public Valoracion? Valoracion { get; set; }
    }
}
