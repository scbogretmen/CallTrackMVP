using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallTrackMVP.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCallLogUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CallLogUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CallLogId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuncellenenTarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenenCagriSaat = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    GuncellemeNedeni = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallLogUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallLogUpdates_CallLogs_CallLogId",
                        column: x => x.CallLogId,
                        principalTable: "CallLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CallLogUpdates_CallLogId",
                table: "CallLogUpdates",
                column: "CallLogId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallLogUpdates");
        }
    }
}
