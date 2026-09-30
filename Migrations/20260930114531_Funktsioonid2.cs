using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace funktsioonide_uurimise.Migrations
{
    /// <inheritdoc />
    public partial class Funktsioonid2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KriitilisedPunktid",
                table: "FunktsiooniUurimised",
                newName: "Positiivsus");

            migrationBuilder.AddColumn<string>(
                name: "Negatiivsus",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nullkohad",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Negatiivsus",
                table: "FunktsiooniUurimised");

            migrationBuilder.DropColumn(
                name: "Nullkohad",
                table: "FunktsiooniUurimised");

            migrationBuilder.RenameColumn(
                name: "Positiivsus",
                table: "FunktsiooniUurimised",
                newName: "KriitilisedPunktid");
        }
    }
}
