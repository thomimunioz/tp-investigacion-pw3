using System;
using System.Collections.Generic;
using System.Text;

namespace ChatbotTienda.Entidades
{
    public class Valoracion
    {
        public int Id { get; set; }
        public int ConversacionId { get; set; }
        public Conversacion Conversacion { get; set; } = null!;
        public int Puntaje { get; set; }
        public string? Comentario { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
