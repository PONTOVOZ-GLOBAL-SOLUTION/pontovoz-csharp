using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontoVoz.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextoBusca",
                table: "Paradas",
                type: "TEXT",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TextoBusca",
                table: "Linhas",
                type: "TEXT",
                maxLength: 320,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TextoBusca",
                table: "Destinos",
                type: "TEXT",
                maxLength: 130,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextoBusca",
                table: "Paradas");

            migrationBuilder.DropColumn(
                name: "TextoBusca",
                table: "Linhas");

            migrationBuilder.DropColumn(
                name: "TextoBusca",
                table: "Destinos");
        }
    }
}
