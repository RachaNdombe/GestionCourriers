using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JconsultGC.Migrations
{
    /// <inheritdoc />
    public partial class AddSignatureAndModificationCourrier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomSignataire",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "SignatureImage",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "Titre",
                table: "Signatures");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Signatures",
                newName: "DateCreation");

            migrationBuilder.AddColumn<string>(
                name: "CheminSignature",
                table: "Signatures",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModification",
                table: "Signatures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Signatures",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstParDefaut",
                table: "Signatures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "Signatures",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Signatures",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Signatures",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ModificationsCourrier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourrierId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChampModifie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AncienneValeur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NouvelleValeur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RaisonModification = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModificationsCourrier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModificationsCourrier_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModificationsCourrier_Courriers_CourrierId",
                        column: x => x.CourrierId,
                        principalTable: "Courriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_UserId",
                table: "Signatures",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModificationsCourrier_CourrierId",
                table: "ModificationsCourrier",
                column: "CourrierId");

            migrationBuilder.CreateIndex(
                name: "IX_ModificationsCourrier_UserId",
                table: "ModificationsCourrier",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Signatures_AspNetUsers_UserId",
                table: "Signatures",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Signatures_AspNetUsers_UserId",
                table: "Signatures");

            migrationBuilder.DropTable(
                name: "ModificationsCourrier");

            migrationBuilder.DropIndex(
                name: "IX_Signatures_UserId",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "CheminSignature",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "DateModification",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "EstParDefaut",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "Nom",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Signatures");

            migrationBuilder.RenameColumn(
                name: "DateCreation",
                table: "Signatures",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "NomSignataire",
                table: "Signatures",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "SignatureImage",
                table: "Signatures",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Titre",
                table: "Signatures",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }
    }
}
