using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDatabase.App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lagerorte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lagerorte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LebensmittelKatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Einheit = table.Column<string>(type: "TEXT", nullable: false),
                    Kategorie = table.Column<string>(type: "TEXT", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LebensmittelKatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProduktInstanzen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LebensmittelKatalogId = table.Column<int>(type: "INTEGER", nullable: false),
                    Menge = table.Column<decimal>(type: "TEXT", nullable: false),
                    MindestbestandMenge = table.Column<decimal>(type: "TEXT", nullable: false),
                    Verfallsdatum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Einkaufsdatum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Lagerort = table.Column<string>(type: "TEXT", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduktInstanzen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProduktInstanzen_LebensmittelKatalog_LebensmittelKatalogId",
                        column: x => x.LebensmittelKatalogId,
                        principalTable: "LebensmittelKatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lagerorte_Name",
                table: "Lagerorte",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LebensmittelKatalog_Name",
                table: "LebensmittelKatalog",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProduktInstanzen_LebensmittelKatalogId",
                table: "ProduktInstanzen",
                column: "LebensmittelKatalogId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lagerorte");

            migrationBuilder.DropTable(
                name: "ProduktInstanzen");

            migrationBuilder.DropTable(
                name: "LebensmittelKatalog");
        }
    }
}
