using ChatbotTienda.Entidades.Enums;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatbotTienda.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public RolUsuario Rol { get; set; } = RolUsuario.Cliente;
        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
        public List<Conversacion> Conversaciones { get; set; } = new();
    }
}
