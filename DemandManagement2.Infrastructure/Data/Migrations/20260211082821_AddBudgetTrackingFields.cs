using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemandManagement2.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CapExAmount",
                table: "Assessments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OpExAmount",
                table: "Assessments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "BudgetEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DemandRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetEntries_DemandRequests_DemandRequestId",
                        column: x => x.DemandRequestId,
                        principalTable: "DemandRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetEntries_DemandRequestId",
                table: "BudgetEntries",
                column: "DemandRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetEntries");

            migrationBuilder.DropColumn(
                name: "CapExAmount",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "OpExAmount",
                table: "Assessments");
        }
    }
}
