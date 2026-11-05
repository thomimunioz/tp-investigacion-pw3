using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChatbotTienda.Entidades.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Intenciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RespuestaBase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiereDerivacion = table.Column<bool>(type: "bit", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intenciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Conversaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimoMensaje = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mensajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversacionId = table.Column<int>(type: "int", nullable: false),
                    Emisor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntencionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensajes_Conversaciones_ConversacionId",
                        column: x => x.ConversacionId,
                        principalTable: "Conversaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mensajes_Intenciones_IntencionId",
                        column: x => x.IntencionId,
                        principalTable: "Intenciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Valoraciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversacionId = table.Column<int>(type: "int", nullable: false),
                    Puntaje = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Valoraciones", x => x.Id);
                    table.CheckConstraint("CK_Valoracion_Puntaje", "[Puntaje] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_Valoraciones_Conversaciones_ConversacionId",
                        column: x => x.ConversacionId,
                        principalTable: "Conversaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Intenciones",
                columns: new[] { "Id", "Activa", "Descripcion", "Nombre", "RequiereDerivacion", "RespuestaBase" },
                values: new object[,]
                {
                    { 1, true, "El usuario saluda o inicia la conversación.", "saludo", false, "¡Hola! Soy el asistente virtual de la tienda. ¿En qué puedo ayudarte?" },
                    { 2, true, "Quiere saber dónde está su pedido o cuándo llega.", "consultar_estado_pedido", false, "Podés ver el estado de tu pedido en la sección Mis pedidos, con tu número de seguimiento." },
                    { 3, true, "Pregunta cómo puede pagar o si hay cuotas.", "medios_de_pago", false, "Aceptamos tarjetas de crédito y débito, transferencia y efectivo. Consultá las cuotas disponibles al finalizar la compra." },
                    { 4, true, "Quiere devolver o cambiar un producto.", "politica_devoluciones", false, "Podés pedir un cambio o devolución dentro de los 10 días de recibido el producto, sin uso y en su empaque original." },
                    { 5, true, "Pregunta cuánto cuesta el envío o si es gratis.", "costo_envio", false, "El costo del envío depende de tu código postal y se calcula al finalizar la compra. Hay envío gratis a partir de cierto monto." },
                    { 6, true, "Pregunta en qué horario atiende la tienda.", "horarios_atencion", false, "Atendemos de lunes a viernes de 9 a 18 hs." },
                    { 7, true, "Pide hablar con una persona o expresa que el bot no lo ayuda.", "hablar_con_humano", false, "Entiendo. Te voy a comunicar con un agente para que continúe la atención." },
                    { 8, true, "Se despide o agradece.", "despedida", false, "¡De nada! Si necesitás algo más, acá estoy. ¡Que tengas un buen día!" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversaciones_UsuarioId",
                table: "Conversaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Intenciones_Nombre",
                table: "Intenciones",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_ConversacionId",
                table: "Mensajes",
                column: "ConversacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_IntencionId",
                table: "Mensajes",
                column: "IntencionId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_ConversacionId",
                table: "Valoraciones",
                column: "ConversacionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mensajes");

            migrationBuilder.DropTable(
                name: "Valoraciones");

            migrationBuilder.DropTable(
                name: "Intenciones");

            migrationBuilder.DropTable(
                name: "Conversaciones");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
