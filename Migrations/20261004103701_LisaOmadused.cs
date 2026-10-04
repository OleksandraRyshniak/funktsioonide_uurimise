using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace funktsioonide_uurimise.Migrations
{
    /// <inheritdoc />
    public partial class LisaOmadused : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kahanemine",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kasvamine",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Muutumispiirkond",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Paarsus",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Periood",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kahanemine",
                table: "FunktsiooniUurimised");

            migrationBuilder.DropColumn(
                name: "Kasvamine",
                table: "FunktsiooniUurimised");

            migrationBuilder.DropColumn(
                name: "Muutumispiirkond",
                table: "FunktsiooniUurimised");

            migrationBuilder.DropColumn(
                name: "Paarsus",
                table: "FunktsiooniUurimised");

            migrationBuilder.DropColumn(
                name: "Periood",
                table: "FunktsiooniUurimised");
        }
    }
}
