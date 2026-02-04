using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallTrackMVP.Web.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CallLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    CagriNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CagriTuru = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TeknisyenAdi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MevcutCagriSaat = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    GuncellenenTarih = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncellenenCagriSaat = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    GuncellemeNedeni = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallLogs_AppUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_UserName",
                table: "AppUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CreatedByUserId",
                table: "CallLogs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_Tarih_CagriNo",
                table: "CallLogs",
                columns: new[] { "Tarih", "CagriNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallLogs");

            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
