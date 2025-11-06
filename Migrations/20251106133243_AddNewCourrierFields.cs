using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JconsultGC.Migrations
{
    /// <inheritdoc />
    public partial class AddNewCourrierFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DossierDeClassement",
                table: "Courriers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HeureRecu",
                table: "Courriers",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nature",
                table: "Courriers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrdreNumero",
                table: "Courriers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistreNumero",
                table: "Courriers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceConcerne",
                table: "Courriers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtilisateursEnCopie",
                table: "Courriers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DossierDeClassement",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "HeureRecu",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "Nature",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "OrdreNumero",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "RegistreNumero",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "ServiceConcerne",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "UtilisateursEnCopie",
                table: "Courriers");
        }
    }
}
