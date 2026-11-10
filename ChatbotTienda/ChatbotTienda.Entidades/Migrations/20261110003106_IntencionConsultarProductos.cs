using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatbotTienda.Entidades.Migrations
{
    /// <inheritdoc />
    public partial class IntencionConsultarProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Intenciones",
                columns: new[] { "Id", "Activa", "Descripcion", "Nombre", "RequiereDerivacion", "RespuestaBase" },
                values: new object[] { 9, true, "Pregunta por precios, stock, talles, colores o características de los productos.", "consultar_productos", false, "Los precios, talles y el stock actualizado de cada producto están en el catálogo de la web. Si no encontrás lo que buscás, te puedo comunicar con un agente." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Intenciones",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
