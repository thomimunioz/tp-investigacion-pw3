using ChatbotTienda.Entidades;
using ChatbotTienda.Entidades.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Datos de prueba para la demo (card 10).
/// Crea un Administrador, 3 Clientes y 9 conversaciones de ejemplo
/// (resueltas, derivadas, con fallback y una activa) con sus valoraciones.
/// Solo siembra si la tabla Usuarios está vacía, así que es seguro llamarlo en cada arranque.
///
/// Usuarios de demo:
///   admin@tienda.com  / Admin123!   (Administrador)
///   lucia@demo.com    / Cliente123!
///   martin@demo.com   / Cliente123!
///   sofia@demo.com    / Cliente123!
/// </summary>
public static class DatosDemo
{
    private const string NoEntendi =
        "No logré entender tu consulta. Puedo ayudarte con pedidos, pagos, envíos, devoluciones y horarios, o te puedo derivar con un agente.";

    // Mismas RespuestaBase que el HasData del ChatbotDbContext.
    private static readonly Dictionary<string, string> Resp = new()
    {
        ["saludo"] = "¡Hola! Soy el asistente virtual de la tienda. ¿En qué puedo ayudarte?",
        ["consultar_estado_pedido"] = "Podés ver el estado de tu pedido en la sección Mis pedidos, con tu número de seguimiento.",
        ["medios_de_pago"] = "Aceptamos tarjetas de crédito y débito, transferencia y efectivo. Consultá las cuotas disponibles al finalizar la compra.",
        ["politica_devoluciones"] = "Podés pedir un cambio o devolución dentro de los 10 días de recibido el producto, sin uso y en su empaque original.",
        ["costo_envio"] = "El costo del envío depende de tu código postal y se calcula al finalizar la compra. Hay envío gratis a partir de cierto monto.",
        ["horarios_atencion"] = "Atendemos de lunes a viernes de 9 a 18 hs.",
        ["hablar_con_humano"] = "Entiendo. Te voy a comunicar con un agente para que continúe la atención.",
        ["despedida"] = "¡De nada! Si necesitás algo más, acá estoy. ¡Que tengas un buen día!",
    };

    public static async Task SembrarAsync(ChatbotDbContext db)
    {
        if (await db.Usuarios.AnyAsync()) return;

        var intenciones = await db.Intenciones.ToDictionaryAsync(i => i.Nombre, i => i.Id);
        var hasher = new PasswordHasher<Usuario>();

        Usuario NuevoUsuario(string nombre, string email, string password, RolUsuario rol, int altaHaceDias)
        {
            var u = new Usuario { Nombre = nombre, Email = email, Rol = rol, FechaAlta = DateTime.UtcNow.AddDays(-altaHaceDias) };
            u.PasswordHash = hasher.HashPassword(u, password);
            return u;
        }

        var admin = NuevoUsuario("Administrador", "admin@tienda.com", "Admin123!", RolUsuario.Administrador, 30);
        var lucia = NuevoUsuario("Lucía Gómez", "lucia@demo.com", "Cliente123!", RolUsuario.Cliente, 20);
        var martin = NuevoUsuario("Martín Pérez", "martin@demo.com", "Cliente123!", RolUsuario.Cliente, 15);
        var sofia = NuevoUsuario("Sofía Díaz", "sofia@demo.com", "Cliente123!", RolUsuario.Cliente, 10);

        // Atajos para armar los mensajes: U = usuario (con su intención), B = respuesta del bot.
        (EmisorMensaje, string, string?) U(string texto, string? intencion) => (EmisorMensaje.Usuario, texto, intencion);
        (EmisorMensaje, string, string?) B(string intencion) => (EmisorMensaje.Bot, Resp[intencion], null);
        (EmisorMensaje, string, string?) BNoEntendi() => (EmisorMensaje.Bot, NoEntendi, null);

        Conversacion Crear(Usuario usuario, string titulo, int haceDias, EstadoConversacion estado,
            (int puntaje, string? comentario)? valoracion,
            params (EmisorMensaje emisor, string texto, string? intencion)[] mensajes)
        {
            var inicio = DateTime.UtcNow.AddDays(-haceDias);
            var conv = new Conversacion { Usuario = usuario, Titulo = titulo, FechaInicio = inicio, Estado = estado };
            var fecha = inicio;
            foreach (var m in mensajes)
            {
                fecha = fecha.AddSeconds(30);
                conv.Mensajes.Add(new Mensaje
                {
                    Emisor = m.emisor,
                    Contenido = m.texto,
                    FechaEnvio = fecha,
                    IntencionId = m.intencion is null ? null : intenciones[m.intencion]
                });
            }
            conv.FechaUltimoMensaje = fecha;
            if (valoracion is { } v)
                conv.Valoracion = new Valoracion { Puntaje = v.puntaje, Comentario = v.comentario, Fecha = fecha.AddMinutes(1) };
            return conv;
        }

        var conversaciones = new List<Conversacion>
        {
            // ---- Resueltas ----
            Crear(lucia, "¿Dónde está mi pedido?", 6, EstadoConversacion.Resuelta, (5, "Rápido y claro"),
                U("Hola", "saludo"), B("saludo"),
                U("¿Dónde está mi pedido?", "consultar_estado_pedido"), B("consultar_estado_pedido"),
                U("Gracias, chau", "despedida"), B("despedida")),

            Crear(lucia, "¿Puedo pagar en cuotas?", 4, EstadoConversacion.Resuelta, (4, null),
                U("¿Puedo pagar en cuotas?", "medios_de_pago"), B("medios_de_pago"),
                U("Perfecto, gracias", "despedida"), B("despedida")),

            Crear(martin, "Costo de envío", 5, EstadoConversacion.Resuelta, (5, null),
                U("cuanto cuesta el envio?", "costo_envio"), B("costo_envio"),
                U("ok gracias", "despedida"), B("despedida")),

            Crear(martin, "Devolución de un producto", 3, EstadoConversacion.Resuelta, (3, "Esperaba más detalle sobre el plazo"),
                U("Quiero devolver un producto", "politica_devoluciones"), B("politica_devoluciones"),
                U("Gracias", "despedida"), B("despedida")),

            // ---- Resuelta, pero con un mensaje que cayó en fallback ----
            Crear(sofia, "¿Cuál es la capital de Francia?", 3, EstadoConversacion.Resuelta, (4, null),
                U("¿Cuál es la capital de Francia?", null), BNoEntendi(),
                U("¿Hasta qué hora atienden?", "horarios_atencion"), B("horarios_atencion"),
                U("Listo, gracias", "despedida"), B("despedida")),

            // ---- Derivadas ----
            Crear(martin, "Quiero hablar con una persona", 2, EstadoConversacion.Derivada, null,
                U("Quiero hablar con una persona", "hablar_con_humano"), B("hablar_con_humano")),

            Crear(sofia, "Esto no me sirve", 1, EstadoConversacion.Derivada, null,
                U("Hola", "saludo"), B("saludo"),
                U("Esto no me sirve, necesito ayuda de alguien real", "hablar_con_humano"), B("hablar_con_humano")),

            // ---- Fallback que terminó en derivación ----
            Crear(martin, "asdfgh", 1, EstadoConversacion.Derivada, (1, "No pudo resolver mi consulta"),
                U("asdfgh", null), BNoEntendi(),
                U("Quiero hablar con alguien", "hablar_con_humano"), B("hablar_con_humano")),

            // ---- Activa (conversación en curso) ----
            Crear(lucia, "¿Hasta qué hora atienden?", 0, EstadoConversacion.Activa, null,
                U("Hola", "saludo"), B("saludo"),
                U("¿Hasta qué hora atienden?", "horarios_atencion"), B("horarios_atencion")),
        };

        db.Usuarios.AddRange(admin, lucia, martin, sofia);
        db.Conversaciones.AddRange(conversaciones);
        await db.SaveChangesAsync();
    }
}
