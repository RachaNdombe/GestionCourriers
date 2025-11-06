using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JconsultGC.Migrations
{
    /// <inheritdoc />
    public partial class AddCourrierFieldsForIndexateur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Confidentialite",
                table: "Courriers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ModeEnvoiId",
                table: "Courriers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priorite",
                table: "Courriers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Courriers_ModeEnvoiId",
                table: "Courriers",
                column: "ModeEnvoiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courriers_ModeEnvois_ModeEnvoiId",
                table: "Courriers",
                column: "ModeEnvoiId",
                principalTable: "ModeEnvois",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courriers_ModeEnvois_ModeEnvoiId",
                table: "Courriers");

            migrationBuilder.DropIndex(
                name: "IX_Courriers_ModeEnvoiId",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "Confidentialite",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "ModeEnvoiId",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "Priorite",
                table: "Courriers");
        }
    }
}
