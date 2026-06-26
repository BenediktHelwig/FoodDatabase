using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDatabase.App.Migrations
{
    /// <inheritdoc />
    public partial class AddNährwertEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nährwerte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LebensmittelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kalorien = table.Column<int>(type: "INTEGER", nullable: false),
                    Fett = table.Column<double>(type: "REAL", nullable: false),
                    GesättigteFettsäuren = table.Column<double>(type: "REAL", nullable: false),
                    Kohlenhydrate = table.Column<double>(type: "REAL", nullable: false),
                    Zucker = table.Column<double>(type: "REAL", nullable: false),
                    Protein = table.Column<double>(type: "REAL", nullable: false),
                    Ballaststoffe = table.Column<double>(type: "REAL", nullable: false),
                    Salz = table.Column<double>(type: "REAL", nullable: false),
                    StandardMengeEinheit = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nährwerte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nährwerte_LebensmittelKatalog_LebensmittelId",
                        column: x => x.LebensmittelId,
                        principalTable: "LebensmittelKatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Nährwerte_LebensmittelId",
                table: "Nährwerte",
                column: "LebensmittelId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nährwerte");
        }
    }
}
