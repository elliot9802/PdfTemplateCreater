using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbContext.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class initial_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketTemplate",
                columns: table => new
                {
                    TicketTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShowEventInfo = table.Column<int>(type: "int", nullable: false),
                    TicketsHandlingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    FileStorageID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTemplate", x => x.TicketTemplateId);
                    table.ForeignKey(
                        name: "FK_TicketTemplate_FileStorage_FileStorageID",
                        column: x => x.FileStorageID,
                        principalTable: "FileStorage",
                        principalColumn: "FileStorageID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketTemplate_FileStorageID",
                table: "TicketTemplate",
                column: "FileStorageID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketTemplate");
        }
    }
}