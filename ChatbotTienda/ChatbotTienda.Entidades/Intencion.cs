using System;
using System.Collections.Generic;
using System.Text;

namespace ChatbotTienda.Entidades
{
    public class Intencion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string RespuestaBase { get; set; } = "";
        public bool RequiereDerivacion { get; set; }
        public bool Activa { get; set; } = true;
    }
}
