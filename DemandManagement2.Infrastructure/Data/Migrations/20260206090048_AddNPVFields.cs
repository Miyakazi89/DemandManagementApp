using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemandManagement2.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNPVFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AnnualBenefit",
                table: "Assessments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedNPV",
                table: "Assessments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                table: "Assessments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialCost",
                table: "Assessments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProjectYears",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnualBenefit",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "CalculatedNPV",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "DiscountRate",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "InitialCost",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "ProjectYears",
                table: "Assessments");
        }
    }
}
