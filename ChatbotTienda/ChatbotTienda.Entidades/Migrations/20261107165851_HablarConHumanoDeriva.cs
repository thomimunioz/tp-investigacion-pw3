using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatbotTienda.Entidades.Migrations
{
    /// <inheritdoc />
    public partial class HablarConHumanoDeriva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Intenciones",
                keyColumn: "Id",
                keyValue: 7,
                column: "RequiereDerivacion",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Intenciones",
                keyColumn: "Id",
                keyValue: 7,
                column: "RequiereDerivacion",
                value: false);
        }
    }
}
