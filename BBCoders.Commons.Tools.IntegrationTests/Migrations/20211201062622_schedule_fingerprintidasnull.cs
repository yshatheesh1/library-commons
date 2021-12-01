using Microsoft.EntityFrameworkCore.Migrations;

namespace BBCoders.Commons.Tools.IntegrationTests.Migrations
{
    public partial class schedule_fingerprintidasnull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Fingerprint_FingerPrintId",
                table: "Schedules");

            migrationBuilder.AlterColumn<long>(
                name: "FingerPrintId",
                table: "Schedules",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Fingerprint_FingerPrintId",
                table: "Schedules",
                column: "FingerPrintId",
                principalTable: "Fingerprint",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Fingerprint_FingerPrintId",
                table: "Schedules");

            migrationBuilder.AlterColumn<long>(
                name: "FingerPrintId",
                table: "Schedules",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Fingerprint_FingerPrintId",
                table: "Schedules",
                column: "FingerPrintId",
                principalTable: "Fingerprint",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
