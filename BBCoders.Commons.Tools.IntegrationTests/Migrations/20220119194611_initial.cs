using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBCoders.Commons.Tools.IntegrationTests.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScheduleSites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ScheduleSiteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleSites", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fingerprint",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FingerprintId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NmlsId = table.Column<long>(type: "bigint", nullable: false),
                    StateId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdatedById = table.Column<long>(type: "bigint", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RenewalDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fingerprint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fingerprint_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ScheduleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FingerPrintId = table.Column<long>(type: "bigint", nullable: true),
                    ScheduleDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ScheduleSiteId = table.Column<long>(type: "bigint", nullable: false),
                    ActionId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastUpdatedById = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Actions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Actions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Schedules_Fingerprint_FingerPrintId",
                        column: x => x.FingerPrintId,
                        principalTable: "Fingerprint",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Schedules_ScheduleSites_ScheduleSiteId",
                        column: x => x.ScheduleSiteId,
                        principalTable: "ScheduleSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ActionId",
                table: "Actions",
                column: "ActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fingerprint_FingerprintId",
                table: "Fingerprint",
                column: "FingerprintId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fingerprint_StateId",
                table: "Fingerprint",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ActionId",
                table: "Schedules",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_FingerPrintId",
                table: "Schedules",
                column: "FingerPrintId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ScheduleId",
                table: "Schedules",
                column: "ScheduleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ScheduleSiteId",
                table: "Schedules",
                column: "ScheduleSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSites_ScheduleSiteId",
                table: "ScheduleSites",
                column: "ScheduleSiteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_States_StateId",
                table: "States",
                column: "StateId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Actions");

            migrationBuilder.DropTable(
                name: "Fingerprint");

            migrationBuilder.DropTable(
                name: "ScheduleSites");

            migrationBuilder.DropTable(
                name: "States");
        }
    }
}
