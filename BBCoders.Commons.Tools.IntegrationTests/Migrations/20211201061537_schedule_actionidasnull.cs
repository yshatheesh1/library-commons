using Microsoft.EntityFrameworkCore.Migrations;

namespace BBCoders.Commons.Tools.IntegrationTests.Migrations
{
    public partial class schedule_actionidasnull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Actions_ActionId",
                table: "Schedules");

            migrationBuilder.AlterColumn<long>(
                name: "ActionId",
                table: "Schedules",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Actions_ActionId",
                table: "Schedules",
                column: "ActionId",
                principalTable: "Actions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Actions_ActionId",
                table: "Schedules");

            migrationBuilder.AlterColumn<long>(
                name: "ActionId",
                table: "Schedules",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Actions_ActionId",
                table: "Schedules",
                column: "ActionId",
                principalTable: "Actions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
