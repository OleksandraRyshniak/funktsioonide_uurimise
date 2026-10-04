using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace funktsioonide_uurimise.Migrations
{
    /// <inheritdoc />
    public partial class lisaKumerus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kaanupunktid",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kumerus",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nogusus",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kaanupunktid",
                table: "FunktsiooniUurimised");

            migrationBuilder.DropColumn(
                name: "Kumerus",
                table: "FunktsiooniUurimised");

            migrationBuilder.DropColumn(
                name: "Nogusus",
                table: "FunktsiooniUurimised");
        }
    }
}
