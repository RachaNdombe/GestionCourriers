using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JconsultGC.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nom",
                table: "ModeEnvois");

            migrationBuilder.DropColumn(
                name: "DossierDeClassement",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "Nature",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "Nom",
                table: "CircuitCourriers");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "CircuitCourriers",
                newName: "Commentaire");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Services",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Services",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Services",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Services",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Services",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "ModeEnvois",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ModeEnvois",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Libelle",
                table: "ModeEnvois",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Ordre",
                table: "ModeEnvois",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "CircuitCourriers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "CircuitCourriers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NomActions",
                table: "CircuitCourriers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Obligatoire",
                table: "CircuitCourriers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrdreExecution",
                table: "CircuitCourriers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "CircuitCourriers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeCircuit",
                table: "CircuitCourriers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "CircuitCourriers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UsersCircuitId",
                table: "CircuitCourriers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CircuitCourriers_ServiceId",
                table: "CircuitCourriers",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CircuitCourriers_UsersCircuitId",
                table: "CircuitCourriers",
                column: "UsersCircuitId");

            migrationBuilder.AddForeignKey(
                name: "FK_CircuitCourriers_AspNetUsers_UsersCircuitId",
                table: "CircuitCourriers",
                column: "UsersCircuitId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CircuitCourriers_Services_ServiceId",
                table: "CircuitCourriers",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CircuitCourriers_AspNetUsers_UsersCircuitId",
                table: "CircuitCourriers");

            migrationBuilder.DropForeignKey(
                name: "FK_CircuitCourriers_Services_ServiceId",
                table: "CircuitCourriers");

            migrationBuilder.DropIndex(
                name: "IX_CircuitCourriers_ServiceId",
                table: "CircuitCourriers");

            migrationBuilder.DropIndex(
                name: "IX_CircuitCourriers_UsersCircuitId",
                table: "CircuitCourriers");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "ModeEnvois");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ModeEnvois");

            migrationBuilder.DropColumn(
                name: "Libelle",
                table: "ModeEnvois");

            migrationBuilder.DropColumn(
                name: "Ordre",
                table: "ModeEnvois");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "CircuitCourriers");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "CircuitCourriers");

            migrationBuilder.DropColumn(
                name: "NomActions",
                table: "CircuitCourriers");

            migrationBuilder.DropColumn(
                name: "Obligatoire",
                table: "CircuitCourriers");

            migrationBuilder.DropColumn(
                name: "OrdreExecution",
                table: "CircuitCourriers");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "CircuitCourriers");

            migrationBuilder.DropColumn(
                name: "TypeCircuit",
                table: "CircuitCourriers");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "CircuitCourriers");

            migrationBuilder.DropColumn(
                name: "UsersCircuitId",
                table: "CircuitCourriers");

            migrationBuilder.RenameColumn(
                name: "Commentaire",
                table: "CircuitCourriers",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Services",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Services",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "ModeEnvois",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DossierDeClassement",
                table: "Courriers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nature",
                table: "Courriers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "CircuitCourriers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }
    }
}
