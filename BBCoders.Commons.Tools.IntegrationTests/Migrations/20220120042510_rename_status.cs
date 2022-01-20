using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBCoders.Commons.Tools.IntegrationTests.Migrations
{
    public partial class rename_status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Statuses",
                table: "Statuses");

            migrationBuilder.RenameTable(
                name: "Statuses",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_Statuses_StatusId",
                table: "Status",
                newName: "IX_Status_StatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Status",
                table: "Status",
                columns: new[] { "Id1", "Id2" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Status",
                table: "Status");

            migrationBuilder.RenameTable(
                name: "Status",
                newName: "Statuses");

            migrationBuilder.RenameIndex(
                name: "IX_Status_StatusId",
                table: "Statuses",
                newName: "IX_Statuses_StatusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Statuses",
                table: "Statuses",
                columns: new[] { "Id1", "Id2" });
        }
    }
}
