using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JconsultGC.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToCourrier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Courriers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourrierHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourrierId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourrierHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourrierHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CourrierHistories_Courriers_CourrierId",
                        column: x => x.CourrierId,
                        principalTable: "Courriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courriers_CreatedById",
                table: "Courriers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CourrierHistories_CourrierId",
                table: "CourrierHistories",
                column: "CourrierId");

            migrationBuilder.CreateIndex(
                name: "IX_CourrierHistories_UserId",
                table: "CourrierHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courriers_AspNetUsers_CreatedById",
                table: "Courriers",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courriers_AspNetUsers_CreatedById",
                table: "Courriers");

            migrationBuilder.DropTable(
                name: "CourrierHistories");

            migrationBuilder.DropIndex(
                name: "IX_Courriers_CreatedById",
                table: "Courriers");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Courriers");
        }
    }
}
