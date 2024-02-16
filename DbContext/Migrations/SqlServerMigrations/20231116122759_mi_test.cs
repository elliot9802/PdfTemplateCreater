using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbContext.Migrations.SqlServerMigrations
{
    /// <inheritdoc />
    public partial class mi_test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketTemplate",
                columns: table => new
                {
                    TicketTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketsHandlingJson = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    ShowEventInfo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTemplate", x => x.TicketTemplateId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketTemplate");
        }
    }
}