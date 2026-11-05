using ChatbotTienda.Entidades.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatbotTienda.Entidades
{
    public class Mensaje
    {
        public int Id { get; set; }
        public int ConversacionId { get; set; }
        public Conversacion Conversacion { get; set; } = null!;
        public EmisorMensaje Emisor { get; set; }
        public string Contenido { get; set; } = "";
        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
        public int? IntencionId { get; set; }
        public Intencion? Intencion { get; set; }
    }
}
