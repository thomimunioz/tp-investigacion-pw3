using ChatbotTienda.Entidades;
using Microsoft.EntityFrameworkCore;

public class ChatbotDbContext : DbContext
{
    public ChatbotDbContext(DbContextOptions<ChatbotDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Conversacion> Conversaciones => Set<Conversacion>();
    public DbSet<Mensaje> Mensajes => Set<Mensaje>();
    public DbSet<Intencion> Intenciones => Set<Intencion>();
    public DbSet<Valoracion> Valoraciones => Set<Valoracion>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);
        });

        mb.Entity<Conversacion>(e =>
        {
            e.Property(c => c.Titulo).HasMaxLength(200);
            e.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
            e.HasOne(c => c.Usuario).WithMany(u => u.Conversaciones)
             .HasForeignKey(c => c.UsuarioId);
        });

        mb.Entity<Mensaje>(e =>
        {
            e.Property(m => m.Emisor).HasConversion<string>().HasMaxLength(20);
            e.HasOne(m => m.Conversacion).WithMany(c => c.Mensajes)
             .HasForeignKey(m => m.ConversacionId);
            e.HasOne(m => m.Intencion).WithMany()
             .HasForeignKey(m => m.IntencionId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        mb.Entity<Intencion>(e =>
        {
            e.HasIndex(i => i.Nombre).IsUnique();
            e.Property(i => i.Nombre).HasMaxLength(100).IsRequired();
        });

        mb.Entity<Valoracion>(e =>
        {
            e.HasIndex(v => v.ConversacionId).IsUnique(); // una valoración por conversación
            e.ToTable(t => t.HasCheckConstraint("CK_Valoracion_Puntaje", "[Puntaje] BETWEEN 1 AND 5"));
            e.HasOne(v => v.Conversacion).WithOne(c => c.Valoracion)
             .HasForeignKey<Valoracion>(v => v.ConversacionId);
        });

        mb.Entity<Intencion>().HasData(
            new Intencion
            {
                Id = 1,
                Nombre = "saludo",
                Descripcion = "El usuario saluda o inicia la conversación.",
                RespuestaBase = "¡Hola! Soy el asistente virtual de la tienda. ¿En qué puedo ayudarte?",
                RequiereDerivacion = false,
                Activa = true
            },
            new Intencion
            {
                Id = 2,
                Nombre = "consultar_estado_pedido",
                Descripcion = "Quiere saber dónde está su pedido o cuándo llega.",
                RespuestaBase = "Podés ver el estado de tu pedido en la sección Mis pedidos, con tu número de seguimiento.",
                RequiereDerivacion = false,
                Activa = true
            },
            new Intencion
            {
                Id = 3,
                Nombre = "medios_de_pago",
                Descripcion = "Pregunta cómo puede pagar o si hay cuotas.",
                RespuestaBase = "Aceptamos tarjetas de crédito y débito, transferencia y efectivo. Consultá las cuotas disponibles al finalizar la compra.",
                RequiereDerivacion = false,
                Activa = true
            },
            new Intencion
            {
                Id = 4,
                Nombre = "politica_devoluciones",
                Descripcion = "Quiere devolver o cambiar un producto.",
                RespuestaBase = "Podés pedir un cambio o devolución dentro de los 10 días de recibido el producto, sin uso y en su empaque original.",
                RequiereDerivacion = false,
                Activa = true
            },
            new Intencion
            {
                Id = 5,
                Nombre = "costo_envio",
                Descripcion = "Pregunta cuánto cuesta el envío o si es gratis.",
                RespuestaBase = "El costo del envío depende de tu código postal y se calcula al finalizar la compra. Hay envío gratis a partir de cierto monto.",
                RequiereDerivacion = false,
                Activa = true
            },
            new Intencion
            {
                Id = 6,
                Nombre = "horarios_atencion",
                Descripcion = "Pregunta en qué horario atiende la tienda.",
                RespuestaBase = "Atendemos de lunes a viernes de 9 a 18 hs.",
                RequiereDerivacion = false,
                Activa = true
            },
            new Intencion
            {
                Id = 7,
                Nombre = "hablar_con_humano",
                Descripcion = "Pide hablar con una persona o expresa que el bot no lo ayuda.",
                RespuestaBase = "Entiendo. Te voy a comunicar con un agente para que continúe la atención.",
                RequiereDerivacion = false,
                Activa = true
            },
            new Intencion
            {
                Id = 8,
                Nombre = "despedida",
                Descripcion = "Se despide o agradece.",
                RespuestaBase = "¡De nada! Si necesitás algo más, acá estoy. ¡Que tengas un buen día!",
                RequiereDerivacion = false,
                Activa = true
            }
        );
    }
}