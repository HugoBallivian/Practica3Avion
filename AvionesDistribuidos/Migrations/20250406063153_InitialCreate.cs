using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvionesDistribuidos.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Paises",
                columns: table => new
                {
                    Id_Pais = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Codigo_Iso = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paises", x => x.Id_Pais);
                });

            migrationBuilder.CreateTable(
                name: "Ciudades",
                columns: table => new
                {
                    Id_Ciudad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Pais = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ciudades", x => x.Id_Ciudad);
                    table.ForeignKey(
                        name: "FK_Ciudades_Paises_Id_Pais",
                        column: x => x.Id_Pais,
                        principalTable: "Paises",
                        principalColumn: "Id_Pais",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Destinos",
                columns: table => new
                {
                    Id_Destino = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Ciudad = table.Column<int>(type: "int", nullable: false),
                    Aeropuerto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion_Corta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinos", x => x.Id_Destino);
                    table.ForeignKey(
                        name: "FK_Destinos_Ciudades_Id_Ciudad",
                        column: x => x.Id_Ciudad,
                        principalTable: "Ciudades",
                        principalColumn: "Id_Ciudad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ciudades_Id_Pais",
                table: "Ciudades",
                column: "Id_Pais");

            migrationBuilder.CreateIndex(
                name: "IX_Destinos_Id_Ciudad",
                table: "Destinos",
                column: "Id_Ciudad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Destinos");

            migrationBuilder.DropTable(
                name: "Ciudades");

            migrationBuilder.DropTable(
                name: "Paises");
        }
    }
}
