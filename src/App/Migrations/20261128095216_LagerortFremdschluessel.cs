using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDatabase.App.Migrations
{
    /// <inheritdoc />
    public partial class LagerortFremdschluessel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lagerort",
                table: "ProduktInstanzen");

            migrationBuilder.AddColumn<int>(
                name: "LagerortId",
                table: "ProduktInstanzen",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProduktInstanzen_LagerortId",
                table: "ProduktInstanzen",
                column: "LagerortId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProduktInstanzen_Lagerorte_LagerortId",
                table: "ProduktInstanzen",
                column: "LagerortId",
                principalTable: "Lagerorte",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProduktInstanzen_Lagerorte_LagerortId",
                table: "ProduktInstanzen");

            migrationBuilder.DropIndex(
                name: "IX_ProduktInstanzen_LagerortId",
                table: "ProduktInstanzen");

            migrationBuilder.DropColumn(
                name: "LagerortId",
                table: "ProduktInstanzen");

            migrationBuilder.AddColumn<string>(
                name: "Lagerort",
                table: "ProduktInstanzen",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
