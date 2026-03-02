using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemandManagement2.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectResourceSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ProjectResources",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ProjectResources");
        }
    }
}
