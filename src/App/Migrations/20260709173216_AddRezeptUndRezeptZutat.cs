using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDatabase.App.Migrations
{
    /// <inheritdoc />
    public partial class AddRezeptUndRezeptZutat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NährwertId",
                table: "LebensmittelKatalog",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Rezepte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Beschreibung = table.Column<string>(type: "TEXT", nullable: true),
                    Zubereitung = table.Column<string>(type: "TEXT", nullable: true),
                    Portionen = table.Column<int>(type: "INTEGER", nullable: false),
                    ZubereitungszeitMinuten = table.Column<int>(type: "INTEGER", nullable: false),
                    Schwierigkeitsgrad = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezepte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RezeptZutaten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RezeptId = table.Column<int>(type: "INTEGER", nullable: false),
                    LebensmittelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Menge = table.Column<double>(type: "REAL", nullable: false),
                    Einheit = table.Column<string>(type: "TEXT", nullable: false),
                    Notizen = table.Column<string>(type: "TEXT", nullable: true),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RezeptZutaten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RezeptZutaten_LebensmittelKatalog_LebensmittelId",
                        column: x => x.LebensmittelId,
                        principalTable: "LebensmittelKatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RezeptZutaten_Rezepte_RezeptId",
                        column: x => x.RezeptId,
                        principalTable: "Rezepte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LebensmittelKatalog_NährwertId",
                table: "LebensmittelKatalog",
                column: "NährwertId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezepte_Name",
                table: "Rezepte",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RezeptZutaten_LebensmittelId",
                table: "RezeptZutaten",
                column: "LebensmittelId");

            migrationBuilder.CreateIndex(
                name: "IX_RezeptZutaten_RezeptId_Position",
                table: "RezeptZutaten",
                columns: new[] { "RezeptId", "Position" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LebensmittelKatalog_Nährwerte_NährwertId",
                table: "LebensmittelKatalog",
                column: "NährwertId",
                principalTable: "Nährwerte",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LebensmittelKatalog_Nährwerte_NährwertId",
                table: "LebensmittelKatalog");

            migrationBuilder.DropTable(
                name: "RezeptZutaten");

            migrationBuilder.DropTable(
                name: "Rezepte");

            migrationBuilder.DropIndex(
                name: "IX_LebensmittelKatalog_NährwertId",
                table: "LebensmittelKatalog");

            migrationBuilder.DropColumn(
                name: "NährwertId",
                table: "LebensmittelKatalog");
        }
    }
}
